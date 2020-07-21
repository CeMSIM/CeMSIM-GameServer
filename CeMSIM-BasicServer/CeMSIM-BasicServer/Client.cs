using System;
using System.Net;
using System.Net.Sockets;

namespace CeMSIM_BasicServer
{
    /// <summary>
    /// This class contains operations how a server can control a client to do.
    /// </summary>
    public class Client
    {
        public static int dataBufferSize = 4096; // 4KB
        public int id;
        public TCP tcp;
        public UDP udp;

        public Client(int _id)
        {
            id = _id;
            tcp = new TCP(id);
            udp = new UDP(id);

        }


        /// <summary>
        /// TCP connection between client-server
        /// </summary>
        public class TCP
        {
            public TcpClient socket;

            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;
            private Packet receivedData;

            public TCP(int _id)
            {
                id = _id;
            }

            /// <summary>
            /// Initialize TCP for the client and set callback function for packet receiver.
            /// </summary>
            /// <param name="_socket"></param>
            public void connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = Client.dataBufferSize;
                socket.SendBufferSize = Client.dataBufferSize;
                receivedData = new Packet();

                stream = socket.GetStream();
                receiveBuffer = new Byte[Client.dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);

                // send welcome message
                Console.WriteLine($"Sending welcome packet to client {id}");
                ServerSend.Welcome(id, $"Welcome. You are connected to server. Your client id is {id}");
            }


            public void SendData(Packet _packet)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error sending packet to client {id} via TCP. Exception {e}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);

                    // disconnect if no response
                    if (_byteLength <= 0)
                    {
                        // disconnect
                        return;
                    }

                    // start to process the data
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength); ///> copy the received data to the temporary data buffer

                    // process data
                    receivedData.Reset(HandleData(_data));

                    // prepare for the next data
                    stream.BeginRead(receiveBuffer, 0, Client.dataBufferSize, ReceiveCallback, null);

                }
                catch (Exception e)
                {
                    Console.WriteLine($"Server exception {e}");

                    // disconnect
                }
            }

            private bool HandleData(byte[] _data)
            {
                int _packetLength = 0;
                receivedData.SetBytes(_data); // append _data to packet            

                // Since the first segment is the data length, an int32 of size 4,
                // by checking whether the unread length >= 4, we know whether this
                // packet is a split packet of a big one or a standalone packet.
                if (receivedData.UnreadLength() >= 4)
                {
                    _packetLength = receivedData.ReadInt32();
                    if (_packetLength <= 0)
                    {
                        return true;
                    }
                }

                // After reading a packet based on the _packetLength, we finish processing
                // a packet. However, if the received data contains more than one packet,
                // we need to begin processing the next one. If the number of UnreadLength() is greater
                // than the current packet's length, there is another packet squeezed in current data payload.
                // Of course, we don't need to process a zero-data packet.
                while (_packetLength > 0 && _packetLength <= receivedData.UnreadLength())
                {
                    byte[] _packetBytes = receivedData.ReadBytes(_packetLength);


                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        // create a packet containing just the data
                        using (Packet _packet = new Packet(_packetBytes))
                        {
                            int _packetId = _packet.ReadInt32();

                            Console.WriteLine($"Receive a packet with id {_packetId}");
                            // call proper handling function based on packet id
                            Server.packetHandlers[_packetId](id, _packet);
                        }
                    });

                    _packetLength = 0;

                    if (receivedData.UnreadLength() >= 4)
                    {
                        _packetLength = receivedData.ReadInt32();
                        if (_packetLength <= 0)
                        {
                            return true;
                        }
                    }

                }

                if (_packetLength <= 1)
                {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// UDP connection between client-server
        /// </summary>
        public class UDP
        {
            public IPEndPoint endPoint;

            private int id;

            public UDP(int _id)
            {
                id = _id;
            }

            public void Connect(IPEndPoint _endPoint)
            {
                endPoint = _endPoint;
            }

            public void SendData(Packet _packet)
            {
                Server.SendUDPData(endPoint, _packet);
            }

            public void HandleData(Packet _packetData)
            {
                int _packetLength = _packetData.ReadInt32();
                byte[] _data = _packetData.ReadBytes(_packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet _packet = new Packet(_data))
                    {
                        int _packetId = _packet.ReadInt32();
                        Server.packetHandlers[_packetId](id, _packet);
                    }
                });

            }

        }

    }
}
