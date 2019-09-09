using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcSpawnerServer.Utility
{
    class Config
    {
        public Server Server { get; set; }
        public Client Client { get; set; }
    }

    class Server
    {
        public string DebugPrefix { get; set; }
        public bool EnableDebugMode { get; set; }
        public float VisibilityDistance { get; set; }
    }

    class Client
    {
        public string DebugPrefix { get; set; }
        public bool EnableDebugMode { get; set; }
        public bool NameAboveTheHead { get; set; }
    }
}
