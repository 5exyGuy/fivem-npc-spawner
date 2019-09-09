using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcSpawnerClient.Utility
{
    class NPC
    {
        public string Name { get; set; }
        public Color NameColor { get; set; }
        public float NameSize { get; set; }
        public string Model { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public bool IsInvincible { get; set; }
        public bool IsPositionFrozen { get; set; }
        public bool BlockPermanentEvents { get; set; }
        // Extra
        public uint HashKey { get; set; }
        public int Code { get; set; }
        public int PedHandle { get; set; }

        public NPC()
        {
        }

        public void DeleteNPC()
        {
            int handle = PedHandle;

            API.DeletePed(ref handle);

            PedHandle = handle;
        }
    }
}
