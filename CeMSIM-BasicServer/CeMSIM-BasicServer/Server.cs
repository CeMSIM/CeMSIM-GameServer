using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace CeMSIM_BasicServer
{
    public class Server
    {
        public static int maxPlayers { get; private set; }///> maximum number of clients the server can handle simultaneously
        public static int port { get; private set; }
        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static Dictionary<int, Client> clients = new Dictionary<int, Client>(); ///> a dictionary storing clients and their ids.

        public delegate void PacketHandler(int _fromClient, Packet _packet);
        public static Dictionary<int, PacketHandler> packetHandlers;

        public static void Start(int _maxPlayers, int _port)
        {
            maxPlayers = _maxPlayers;
            port = _port;

            Console.WriteLine($"Server initializing...");

            InitializeServerData();

            // initialize tcpListener
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();

            // start to accept asynchronized connection req with a callback 
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server is listening on port {_port}");

        }

        /// <summary>
        /// Callback function called once a client request for a TCP connection
        /// </summary>
        /// <param name="_result"></param>
        private static void TCPConnectCallback(IAsyncResult _result)
        {
            // it's an async method, so we use EndAcceptTcpClient rather than AcceptTcpClient
            TcpClient _tcpClient = tcpListener.EndAcceptTcpClient(_result);

            // call the function itself in preparation for the next client 
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Connection request from ip:{_tcpClient.Client.RemoteEndPoint}");


            // find which position is available to the user.
            for (int i = 1; i <= maxPlayers; i++)
            {
                if (clients[i].tcp.socket == null)
                {
                    // if we find one service slot that is empty,
                    // the server let the client to take up the position
                    clients[i].tcp.connect(_tcpClient);
                    return;
                }
            }

            // reach here means all positions are occupied
            Console.WriteLine($"{_tcpClient.Client.RemoteEndPoint} failed to connect. Server fully occupied");
        }

        /// <summary>
        /// Callback function for UDP
        /// </summary>
        /// <param name="_result"></param>
        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);

                udpListener.BeginReceive(UDPReceiveCallback, null);

                /// Here, we discard the packet if its packet length is smaller than
                /// the 4-byte packet length (int32). However, UDP is less reliable
                /// than TCP, it's possible that the network lost the first half
                /// of the packet and we received the second half. For UDP, we are expected
                /// to accept lossing packets. And we may need to verify the packet intactivity
                /// ourselves.
                if (_data.Length < 4)
                {
                    return;
                }

                using (Packet _packet = new Packet(_data))
                {
                    // This line read the client id embedded in the packet.
                    // There may be a security issue here, but currently, it's our plan.
                    int _clientId = _packet.ReadInt32();
                    Console.WriteLine($"Received a UDP packet from client id claimed as {_clientId}");
                    if (_clientId == 0) // invalid client id.
                    {
                        return;
                    }

                    // check whether this is the first communication
                    if (clients[_clientId].udp.endPoint == null)
                    {
                        // build up the connection
                        clients[_clientId].udp.Connect(_clientEndPoint);
                        return;
                    }

                    // compare whether the packet is sent from a client we know
                    if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                    {
                        clients[_clientId].udp.HandleData(_packet);
                    }
                }
            }
            catch (Exception _e)
            {
                Console.WriteLine($"UDPReceiveCallback malfunctioning with exception {_e}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
        {
            try
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
            catch (Exception _e)
            {
                Console.WriteLine($"Cannot send UDP packet. Exception {_e}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= maxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }


            packetHandlers = new Dictionary<int, PacketHandler>
            {
                { (int)ClientPackets.welcomeReceived, ServerHandle.WelcomeReceived },
                { (int)ClientPackets.pingTCP, ServerHandle.TCPPingReceived},
                { (int)ClientPackets.pingUDP, ServerHandle.UDPPingReceived}
            };

            Console.WriteLine("Initialized Server Data");
        }

    }
}
