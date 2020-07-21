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
            }

        }

    }
}
