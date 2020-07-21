using System;
namespace CeMSIM_BasicServer
{
    /// <summary>
    /// This class handles packets received from clients
    /// </summary>
    public class ServerHandle
    {

        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIdCheck = _packet.ReadInt32();
            string _username = _packet.ReadString();

            Console.WriteLine($"Client {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connects successfully and whose username is {_username}");

            // check whether the packet is from the client
            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine($"Client {_fromClient} has assumed with client id {_clientIdCheck} with username {_username}");
                return;
            }

        }

        public static void UDPPingReceived(int _fromClient, Packet _packet)
        {
            // Digest the packet
            int _clientIdCheck = _packet.ReadInt32();
            string _msg = _packet.ReadString();

            Console.WriteLine($"Client {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a UDP ping with msg {_msg}");

            // check whether the packet is from the client
            if (_clientIdCheck != _fromClient)
            {
                Console.WriteLine($"Client {_fromClient} has assumed with client id {_clientIdCheck} ");
                return;
            }

            // Create response
            // we reply the client with the same mesage appended with a check message
            string _replyMsg = _msg + " - server read";
            ServerSend.UDPPingReply(_fromClient, _msg);
        }


        public static void TCPPingReceived(int _fromClient, Packet _packet)
        {
            // Digest the packet
            string _msg = _packet.ReadString();

            Console.WriteLine($"Client {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a TCP ping with msg {_msg}");


            // Create response
            // we reply the client with the same mesage appended with a check message
            string _replyMsg = _msg + " - server read";
            ServerSend.TCPPingReply(_fromClient, _msg);
        }

    }
}
