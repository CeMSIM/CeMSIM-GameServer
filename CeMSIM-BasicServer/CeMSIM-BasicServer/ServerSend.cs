﻿using System;
namespace CeMSIM_BasicServer
{
    /// <summary>
    /// This class creates packets to be sent
    /// </summary>
    public class ServerSend
    {

        #region Send TCP Packets
        /// <summary>
        /// Send a packet to a particular client
        /// </summary>
        /// <param name="_toClient">Client id</param>
        /// <param name="_packet">The packet</param>
        private static void SendTCPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength(); // add the Data Length to the packet
            Server.clients[_toClient].tcp.SendData(_packet);

        }

        /// <summary>
        /// Multicast packet to all clients
        /// </summary>
        /// <param name="_packet">The packet to multicast</param>
        private static void MulticastTCPData(Packet _packet)
        {
            for (int i = 1; i < Server.maxPlayers; i++)
            {
                Server.clients[i].tcp.SendData(_packet);
            }
        }

        /// <summary>
        /// Multicast packet to all but one client.
        /// </summary>
        /// <param name="_exceptClient">The client to who no packet is sent</param>
        /// <param name="_packet"> The packet to multicast</param>
        private static void MulticastExceptOneTCPData(int _exceptClient, Packet _packet)
        {
            for (int i = 1; i < Server.maxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].tcp.SendData(_packet);
                }
            }
        }

        #endregion

        #region Send UDP Packets

        /// <summary>
        /// Send a packet to a particular client
        /// </summary>
        /// <param name="_toClient">Client id</param>
        /// <param name="_packet">The packet to send</param>
        private static void SendUDPData(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.clients[_toClient].udp.SendData(_packet);
        }

        /// <summary>
        /// Multicast packet to all clients
        /// </summary>
        /// <param name="_packet">The packet to multicast</param>
        private static void MulticastUDPData(Packet _packet)
        {
            for (int i = 1; i < Server.maxPlayers; i++)
            {
                Server.clients[i].udp.SendData(_packet);
            }
        }

        /// <summary>
        /// Multicast packet to all but one client.
        /// </summary>
        /// <param name="_exceptClient">The client to who no packet is sent</param>
        /// <param name="_packet"> The packet to multicast</param>
        private static void MulticastExceptOneUDPData(int _exceptClient, Packet _packet)
        {
            for (int i = 1; i < Server.maxPlayers; i++)
            {
                if (i != _exceptClient)
                {
                    Server.clients[i].udp.SendData(_packet);
                }
            }
        }

        #endregion



        /// <summary>
        /// Send a welcome packet to a particular client
        /// </summary>
        /// <param name="_toClient">id of the client to receive the packet</param>
        /// <param name="_msg"></param>
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                /*
                 * Packet format: <---|Data length|msg string|client id|
                 * Data length is taken cared by the SendTCPData function. 
                 * Here, user focuses on data.
                 * **/
                _packet.Write(_msg);
                _packet.Write(_toClient); // add the client id assigned to the client

                SendTCPData(_toClient, _packet);
            }
        }

        /// <summary>
        /// Reply client's TCP ping
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_msg"></param>
        public static void TCPPingReply(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.pingResponseTCP))
            {
                _packet.Write(_msg);

                SendTCPData(_toClient, _packet);
            }
        }


        /// <summary>
        /// Reply client's UDP ping
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_msg"></param>
        public static void UDPPingReply(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.pingResponseUDP))
            {
                _packet.Write(_msg);

                SendUDPData(_toClient, _packet);
            }
        }

        /// <summary>
        /// Inform the spawn of a player
        /// </summary>
        /// <param name="_toClient"></param>
        /// <param name="_player"></param>
        public static void SpawnPlayer(int _toClient, Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.username);
                _packet.Write(_player.position);
                _packet.Write(_player.rotation);

                SendTCPData(_toClient, _packet);
            }
        }

        public static void PlayerPosition(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerPosition))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.position);

                MulticastUDPData(_packet);
            }
        }

        public static void PlayerRotation(Player _player)
        {
            using (Packet _packet = new Packet((int)ServerPackets.PlayerRotation))
            {
                _packet.Write(_player.id);
                _packet.Write(_player.rotation);

                // no need to force update user's rotation
                MulticastExceptOneUDPData(_player.id, _packet);
            }
        }
    }
}
