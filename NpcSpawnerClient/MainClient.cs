using CitizenFX.Core;
using CitizenFX.Core.Native;
using NpcSpawnerClient.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NpcSpawnerClient
{
    public class MainClient : BaseScript
    {
        private static List<NPC> NPCs = new List<NPC>();
        private static Config Config = new Config();
        private static Vector3 firstPos;
        private static bool firstSpawn = true;
        private static bool positionTickIsOn = false;

        public MainClient()
        {
            EventHandlers.Add("npcspawner:spawnNPC", new Action<dynamic>(SpawnNPC));
            EventHandlers.Add("npcspawner:deleteNPC", new Action<int>(DeleteNPC));
            EventHandlers.Add("playerSpawned", new Action(OnPlayerSpawned));
            EventHandlers.Add("npcspawner:onConfigLoaded", new Action<dynamic>(OnConfigLoaded));
            EventHandlers.Add("onClientResourceStart", new Action<string>(OnClientResourceStart));
            EventHandlers.Add("onClientResourceStop", new Action<string>(OnClientResourceStop));
        }

        private void OnClientResourceStart(string resourceName)
        {
            if (API.GetCurrentResourceName() != resourceName) return;

            TriggerServerEvent("npcspawner:onPlayerLoaded");

            Tick += OnTick;

            if (!object.ReferenceEquals(Game.PlayerPed.Position, null)) {
                if (!positionTickIsOn)
                {
                    Tick += OnPositionUpdate;
                    positionTickIsOn = true;
                }
            }
        }

        private void OnClientResourceStop(string resourceName)
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                NPCs[i].DeleteNPC();
                if (Config.EnableDebugMode)
                    Debug.WriteLine($"{Config.DebugPrefix} {NPCs[i].Name} has been deleted");
                NPCs.RemoveAt(i);
            }
        }

        private void OnConfigLoaded(dynamic data)
        {
            Config.DebugPrefix = data.DebugPrefix;
            Config.EnableDebugMode = data.EnableDebugMode;
            Config.NameAboveTheHead = data.NameAboveTheHead;
        }

        private void OnPlayerSpawned()
        {
            if (firstSpawn)
            {
                TriggerServerEvent("npcspawner:onPlayerLoaded");
                if (!positionTickIsOn)
                {
                    Tick += OnPositionUpdate;
                    positionTickIsOn = true;
                }
                firstSpawn = false;
            }

            Vector3 playerPos = Game.PlayerPed.Position;

            firstPos = playerPos;
        }

        private async void SpawnNPC(dynamic data)
        {
            NPC npc = new NPC();
            npc.Name = data.Name;
            npc.NameColor = new Color() { 
                R = data.NameColor.R,
                G = data.NameColor.G,
                B = data.NameColor.B,
                Alpha = data.NameColor.Alpha
            };
            npc.NameSize = data.NameSize;
            npc.Model = data.Model;
            npc.Position = new Vector3(data.Position.X,
                                        data.Position.Y,
                                        data.Position.Z);
            npc.Heading = data.Heading;
            npc.IsInvincible = data.IsInvincible;
            npc.IsPositionFrozen = data.IsPositionFrozen;
            npc.BlockPermanentEvents = data.BlockPermanentEvents;
            npc.HashKey = data.HashKey;
            npc.Code = data.Code;

            Model model = new Model("csb_mweather");

            while (!model.IsLoaded)
            {
                await model.Request(1000);
            }

            int npcHandle = API.CreatePed(4, (uint)model.Hash, npc.Position.X, npc.Position.Y, npc.Position.Z, npc.Heading, false, false);
            npc.PedHandle = npcHandle;

            API.SetEntityAsMissionEntity(npcHandle, false, false);
            API.SetBlockingOfNonTemporaryEvents(npcHandle, true);
            API.FreezeEntityPosition(npcHandle, true);
            API.SetEntityInvincible(npcHandle, true);

            if (Config.EnableDebugMode)
                Debug.WriteLine($"{Config.DebugPrefix} {data.Name} has been spawned");

            NPCs.Add(npc);
        }

        private void DeleteNPC(int code)
        {
            for (int i = 0; i < NPCs.Count; i++)
            {
                if (NPCs[i].Code == code)
                {
                    NPCs[i].DeleteNPC();
                    if (Config.EnableDebugMode)
                        Debug.WriteLine($"{Config.DebugPrefix} {NPCs[i].Name} has been deleted");
                    NPCs.RemoveAt(i);
                }
            }
        }

        private async Task OnPositionUpdate()
        {
            
            Vector3 playerPos = Game.PlayerPed.Position;

            if (firstPos == playerPos) { return; }
            else
            {
                firstPos = playerPos;
                TriggerServerEvent("npcspawner:onPositionUpdate", firstPos);
            }

            await Delay(500);
        }

        private async Task OnTick()
        {
            if (NPCs.Count == 0) return;

            if (Config.NameAboveTheHead)
            {
                for (int i = 0; i < NPCs.Count; i++)
                {
                    Draw3DText(NPCs[i].Position + new Vector3(0.0f, 0.0f, 2.0f),
                               NPCs[i].Name,
                               1.0f,
                               NPCs[i].NameColor.R, NPCs[i].NameColor.G, NPCs[i].NameColor.B, NPCs[i].NameColor.Alpha);
                }
            }

            await Task.FromResult(0);
        }

        private void Draw3DText(Vector3 coords, string text, float size, int r, int g, int b, int alpha)
        {
            float screenX = 0.0f, screenY = 0.0f;
            bool onScreen = API.World3dToScreen2d(coords.X, coords.Y, coords.Z, ref screenX, ref screenY);
            Vector3 camCoords = API.GetGameplayCamCoords();
            float distance = API.GetDistanceBetweenCoords(camCoords.X, camCoords.Y, camCoords.Z, coords.X, coords.Y, coords.Z, true);

            float scale = (size / distance) * 2;
            float fov = (1 / API.GetGameplayCamFov()) * 100;
            scale = scale * fov;

            if (onScreen)
            {
                API.SetTextScale(0.0f * scale, 0.55f * scale);
                API.SetTextFont(0);
                API.SetTextColour(r, g, b, alpha);
                API.SetTextDropshadow(0, 0, 0, 0, 255);
                API.SetTextDropShadow();
                API.SetTextOutline();
                API.SetTextEntry("STRING");
                API.SetTextCentre(true);
                API.AddTextComponentString(text);
                API.DrawText(screenX, screenY);
            }
        }
    }
}
