using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;
using NpcSpawnerServer.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NpcSpawnerServer
{
    public class MainServer : BaseScript
    {
        private static List<NPC> NPCs;
        private static Config Config;

        public MainServer()
        {
            EventHandlers.Add("npcspawner:onPlayerLoaded", new Action<Player>(OnPlayerLoaded));
            EventHandlers.Add("npcspawner:onPositionUpdate", new Action<Player, dynamic>(OnPositionUpdate));

            Initialize();
        }

        private void Initialize()
        {
            string npcsJson = API.LoadResourceFile(API.GetCurrentResourceName(), "npcs.json");
            string configJson = API.LoadResourceFile(API.GetCurrentResourceName(), "config.json");

            NPCs = new List<NPC>();
            NPCs = JsonConvert.DeserializeObject<List<NPC>>(npcsJson);

            Config = new Config();
            Config = JsonConvert.DeserializeObject<Config>(configJson);

            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].HashKey = (uint)API.GetHashKey(NPCs[i].Model);
                NPCs[i].Code = int.Parse(new Random().Next(111111, 999999).ToString() + i);
            }

            Debug.WriteLine($"{Config.Server.DebugPrefix} Resource has been successfuly initialized");
        }

        private void OnPositionUpdate([FromSource] Player player, dynamic position)
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                Vector3 npcPos = NPCs[i].Position; // NPC's position

                Vector3 playerPos = new Vector3(position.X, position.Y, position.Z); // Player's position

                float distance = Vector3.DistanceSquared(playerPos, npcPos); // Distance between player and npc

                // Checking if the player is within a certain distance between NPC
                if (distance <= Config.Server.VisibilityDistance)
                {
                    if (NPCs[i].isSpawned) continue;

                    player.TriggerEvent("npcspawner:spawnNPC", NPCs[i]);

                    NPCs[i].isSpawned = true;

                    if (Config.Server.EnableDebugMode)
                        Debug.WriteLine($"{Config.Server.DebugPrefix} {NPCs[i].Name} has been created for player {player.Name}");
                }
                else
                {
                    if (NPCs[i].isSpawned) NPCs[i].isSpawned = false;
                    else continue;

                    player.TriggerEvent("npcspawner:deleteNPC", NPCs[i].Code);

                    if (Config.Server.EnableDebugMode)
                        Debug.WriteLine($"{Config.Server.DebugPrefix} {NPCs[i].Name} has been removed for player {player.Name}");
                }
            }
        }

        private void OnPlayerLoaded([FromSource] Player player)
        {
            player.TriggerEvent("npcspawner:onConfigLoaded", Config.Client);
        }
    }
}
