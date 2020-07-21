using System;
namespace CeMSIM_BasicServer
{
    public class Constants
    {
        // Network Configuration
        public const int TCP_PORT = 54321;

        // Server Update Rate
        public const int TICKS_PER_SECOND = 30;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SECOND; // milliumsecond per tick

    }
}
