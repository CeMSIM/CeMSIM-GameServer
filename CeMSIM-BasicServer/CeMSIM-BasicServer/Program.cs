using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CeMSIM_BasicServer
{
    class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            int maxPlayers = 10;
            int port = 54321;

            Console.Title = "Game Server";
            Console.WriteLine("Hello World!");

            isRunning = true;
            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(maxPlayers, port);

        }

        private static void MainThread()
        {
            Console.WriteLine($"Server main thread started. Running at {Constants.TICKS_PER_SECOND} ticks per second. ");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while (_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();

                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    // sleep if GameLogic.Update takes much shorter time than a tick
                    if (_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }

                }
            }
        }
    }
}
