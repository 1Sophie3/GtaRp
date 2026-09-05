using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class FactionNpc
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public string EventName { get; set; }
    }

    public class FactionClientScript : BaseScript
    {
        private bool isMenuOpen = false;
        private long lastInteractionTime = 0;

        private readonly List<FactionNpc> factionNpcs = new List<FactionNpc>
        {
            new FactionNpc { Name = "LSPD", Position = new Vector3(447.213f, -980.714f, 30.689f), EventName = "LSPD_DutyNpc_Interact" },
            new FactionNpc { Name = "LSMD", Position = new Vector3(315.043f, -592.215f, 43.265f), EventName = "LSMD_DutyNpc_Interact" },
            new FactionNpc { Name = "LSCS", Position = new Vector3(460.0f, -570.0f, 28.0f), EventName = "LSCS_DutyNpc_Interact" },
            new FactionNpc { Name = "DrivingSchool", Position = new Vector3(-711.95f, -1307.68f, 5.11f), EventName = "DrivingSchool_DutyNpc_Interact" }
        };

        public FactionClientScript()
        {
            // Client Events
            EventHandlers["Faction:ShowMenu"] += new Action<string, bool>(ShowMenu);

            // NUI Handlers
            RegisterNuiCallbackType("faction:startDuty");
            RegisterNuiCallbackType("faction:endDuty");
            RegisterNuiCallbackType("faction:closeMenu");

            EventHandlers["__cfx_nui:faction:startDuty"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                TriggerServerEvent("Faction:StartDuty");
                CloseMenu();
                cb("ok");
            });

            EventHandlers["__cfx_nui:faction:endDuty"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                TriggerServerEvent("Faction:EndDuty");
                CloseMenu();
                cb("ok");
            });

            EventHandlers["__cfx_nui:faction:closeMenu"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                CloseMenu();
                cb("ok");
            });

            Tick += OnTick;
        }

        private void ShowMenu(string factionName, bool isOnDuty)
        {
            isMenuOpen = true;
            API.SetNuiFocus(true, true);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openFactionMenu",
                payload = new
                {
                    factionName,
                    isOnDuty
                }
            }));
        }

        private void CloseMenu()
        {
            if (isMenuOpen)
            {
                isMenuOpen = false;
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeFactionMenu" }));
            }
        }

        private async Task OnTick()
        {
            if (isMenuOpen)
            {
                await Task.FromResult(0);
                return;
            }

            Vector3 playerPos = API.GetEntityCoords(API.PlayerPedId(), true);

            foreach (var npc in factionNpcs)
            {
                float distance = API.GetDistanceBetweenCoords(playerPos.X, playerPos.Y, playerPos.Z, npc.Position.X, npc.Position.Y, npc.Position.Z, true);

                if (distance <= 2.5f)
                {
                    Draw3DText(npc.Position.X, npc.Position.Y, npc.Position.Z + 1.0f, "Drücke E, um zu interagieren");

                    // 0x45 / Control 38 = 'E'
                    if (Game.IsControlJustPressed(0, Control.Context) && (DateTime.Offset.Now.ToUnixTimeMilliseconds() - lastInteractionTime > 1000))
                    {
                        lastInteractionTime = DateTime.Offset.Now.ToUnixTimeMilliseconds();
                        TriggerServerEvent(npc.EventName);
                    }
                    break;
                }
            }

            await Task.FromResult(0);
        }

        private void Draw3DText(float x, float y, float z, string text)
        {
            float screenX = 0f, screenY = 0f;
            if (API.GetScreenCoordFromWorldCoord(x, y, z, ref screenX, ref screenY))
            {
                API.SetTextScale(0.4f, 0.4f);
                API.SetTextFont(4);
                API.SetTextProportional(true);
                API.SetTextColour(255, 255, 255, 185);
                API.SetTextOutline();
                API.SetTextEntry("STRING");
                API.AddTextComponentString(text);
                API.DrawText(screenX, screenY);
            }
        }
    }
}