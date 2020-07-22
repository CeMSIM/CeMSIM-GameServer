﻿using System;
using System.Numerics;
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

        public static void PingUDP(int _fromClient, Packet _packet)
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


        public static void PingTCP(int _fromClient, Packet _packet)
        {
            // Digest the packet
            string _msg = _packet.ReadString();

            Console.WriteLine($"Client {Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} sends a TCP ping with msg {_msg}");


            // Create response
            // we reply the client with the same mesage appended with a check message
            string _replyMsg = _msg + " - server read";
            ServerSend.TCPPingReply(_fromClient, _msg);
        }

        /// <summary>
        /// In response to client's SpawnRequest packet.
        /// Send the player into the game (simulation) and reply with the spawn detail
        /// </summary>
        /// <param name="_fromClient"></param>
        /// <param name="_packet"></param>
        public static void SpawnRequest(int _fromClient, Packet _packet)
        {
            string _username = _packet.ReadString();

            Console.WriteLine($"client{_fromClient}: Spawn player.");

            // send back the packet with necessary inforamation about player locations
            Server.clients[_fromClient].SendIntoGame(_username);
        }

        /// <summary>
        /// Handle the user control on the player and respond with the updated player status
        /// </summary>
        /// <param name="_fromClient"></param>
        /// <param name="_packet"></param>
        public static void PlayerMovement(int _fromClient, Packet _packet)
        {
            bool[] _inputs = new bool[_packet.ReadInt32()];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = _packet.ReadBool();
            }

            Quaternion _rotation = _packet.ReadQuaternion();

            Console.WriteLine($"client{_fromClient}: move packet received.");

            Server.clients[_fromClient].player.SetInput(_inputs, _rotation);
        }

    }
}
