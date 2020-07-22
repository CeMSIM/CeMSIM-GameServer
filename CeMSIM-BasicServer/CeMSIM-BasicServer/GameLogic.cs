using System;
namespace CeMSIM_BasicServer
{
    public class GameLogic
    {
        public static void Update()
        {
            // calculate each player's position and update them
            // TODO: calling Update multiple times is valid but generates mutliple
            // small packets. There is an improvement here by shaping the 
            foreach (Client _client in Server.clients.Values)
            {
                if (_client.player != null)
                {
                    _client.player.Update();
                }
            }

            ThreadManager.UpdateMain();
        }
        public GameLogic()
        {
        }
    }
}
