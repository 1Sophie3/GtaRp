using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class HousePromptData
    {
        public bool isLocked { get; set; }
        public bool isOwner { get; set; }
        public bool isRenter { get; set; }
        public bool hasKey { get; set; }
    }

    public class HouseClientScript : BaseScript
    {
        private bool isMenuOpen = false;
        private string currentLoadedIpl = null;
        private HousePromptData currentHousePrompt = null;

        public HouseClientScript()
        {
            // Events vom Server
            EventHandlers["Client:House:ShowInteractionPrompt"] += new Action<string>(OnShowInteractionPrompt);
            EventHandlers["Client:House:HideInteractionPrompt"] += new Action(OnHideInteractionPrompt);
            EventHandlers["Client:House:OpenMenu"] += new Action<string>(OnOpenMenu);
            EventHandlers["Client:House:ForceMenuClose"] += new Action(CloseMenu);
            EventHandlers["Client:RequestIpl"] += new Action<string>(OnRequestIpl);
            EventHandlers["Client:House:UnloadIpl"] += new Action(OnUnloadIpl);

            // NUI Callbacks
            RegisterNuiCallbackType("house:closeMenu");
            RegisterNuiCallbackType("house:action");
            RegisterNuiCallbackType("house:giveKey");

            EventHandlers["__cfx_nui:house:closeMenu"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                CloseMenu();
                cb("ok");
            });

            EventHandlers["__cfx_nui:house:action"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                string action = data.action?.ToString();
                dynamic actionData = data.data;

                HandleMenuAction(action, actionData);
                cb("ok");
            });

            EventHandlers["__cfx_nui:house:giveKey"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                if (data.targetId != null)
                {
                    int targetId = Convert.ToInt32(data.targetId);
                    TriggerServerEvent("Server:House:GiveKey", targetId);
                }
                CloseMenu();
                cb("ok");
            });

            Tick += OnTick;
        }

        private void OnShowInteractionPrompt(string houseDataJson)
        {
            try
            {
                currentHousePrompt = JsonConvert.DeserializeObject<HousePromptData>(houseDataJson);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HouseClient] Fehler beim Parsen des Prompt-JSONs: {ex.Message}");
            }
        }

        private void OnHideInteractionPrompt()
        {
            currentHousePrompt = null;
            CloseMenu();
        }

        private void OnOpenMenu(string houseDataJson)
        {
            if (isMenuOpen) return;

            isMenuOpen = true;
            API.SetNuiFocus(true, true);

            try
            {
                var houseData = JsonConvert.DeserializeObject(houseDataJson);
                API.SendNuiMessage(JsonConvert.SerializeObject(new
                {
                    action = "openHouseMenu",
                    payload = houseData
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[HouseClient] Fehler beim Parsen der Menüdaten: {ex.Message}");
            }
        }

        private void CloseMenu()
        {
            if (isMenuOpen)
            {
                isMenuOpen = false;
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeHouseMenu" }));
            }
        }

        private void HandleMenuAction(string action, dynamic data)
        {
            switch (action)
            {
                case "buy":
                    TriggerServerEvent("Server:House:Buy");
                    CloseMenu();
                    break;
                case "rent_1":
                    TriggerServerEvent("Server:House:Rent", 1);
                    CloseMenu();
                    break;
                case "rent_7":
                    TriggerServerEvent("Server:House:Rent", 7);
                    CloseMenu();
                    break;
                case "rent_30":
                    TriggerServerEvent("Server:House:Rent", 30);
                    CloseMenu();
                    break;
                case "enter":
                    TriggerServerEvent("Server:House:Enter");
                    CloseMenu();
                    break;
                case "toggleLock":
                    TriggerServerEvent("Server:House:ToggleLock");
                    CloseMenu();
                    break;
                case "changeLocks":
                    TriggerServerEvent("Server:House:ChangeLocks");
                    CloseMenu();
                    break;
                case "removeKey":
                    if (data != null)
                    {
                        int targetAccountId = Convert.ToInt32(data);
                        TriggerServerEvent("Server:House:RemoveKey", targetAccountId);
                    }
                    break;
            }
        }

        // IPL Management
        private void OnRequestIpl(string ipl)
        {
            if (!string.IsNullOrEmpty(currentLoadedIpl))
            {
                API.RemoveIpl(currentLoadedIpl);
            }

            API.RequestIpl(ipl);
            currentLoadedIpl = ipl;
        }

        private void OnUnloadIpl()
        {
            if (!string.IsNullOrEmpty(currentLoadedIpl))
            {
                API.RemoveIpl(currentLoadedIpl);
                currentLoadedIpl = null;
            }
        }

        // Key Binding Tick (E-Taste)
        private async Task OnTick()
        {
            // 38 = Context / 'E'
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                if (API.IsChatActive() || isMenuOpen)
                {
                    await Task.FromResult(0);
                    return;
                }

                if (currentHousePrompt != null)
                {
                    if (!currentHousePrompt.isLocked && !currentHousePrompt.isOwner && !currentHousePrompt.isRenter && !currentHousePrompt.hasKey)
                    {
                        TriggerServerEvent("Server:House:Enter");
                    }
                    else
                    {
                        TriggerServerEvent("Server:House:RequestMenuData");
                    }
                }
                else if (API.GetPlayerRoutingBucket(API.PlayerId()) != 0) // Ersatz für mp.players.local.dimension
                {
                    TriggerServerEvent("Server:House:Exit");
                }
            }

            await Task.FromResult(0);
        }
    }
}