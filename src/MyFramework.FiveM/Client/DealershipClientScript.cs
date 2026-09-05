using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class DealershipVehicleInfo
    {
        public string DisplayName { get; set; }
        public int Price { get; set; }
        public string Description { get; set; }
        public string Model { get; set; }
        public bool IsFactionBuyable { get; set; }
        public int PlayerFactionRank { get; set; }
    }

    public class DealershipClientScript : BaseScript
    {
        private bool isMenuOpen = false;
        private DealershipVehicleInfo currentVehicleInfo = null;

        public DealershipClientScript()
        {
            // Server Events
            EventHandlers["client:dealership:showMenu"] += new Action<string>(OnShowMenu);
            EventHandlers["client:dealership:closeMenu"] += new Action(CloseMenu);

            // NUI Callbacks
            RegisterNuiCallbackType("dealership:closeMenu");
            RegisterNuiCallbackType("dealership:buy");

            EventHandlers["__cfx_nui:dealership:closeMenu"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                CloseMenu();
                cb("ok");
            });

            EventHandlers["__cfx_nui:dealership:buy"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                bool forFaction = data.forFaction ?? false;

                if (currentVehicleInfo != null && !string.IsNullOrEmpty(currentVehicleInfo.Model))
                {
                    TriggerServerEvent("server:dealership:buyVehicle", currentVehicleInfo.Model, forFaction);
                }

                cb("ok");
            });
        }

        private void OnShowMenu(string vehicleInfoJson)
        {
            try
            {
                currentVehicleInfo = JsonConvert.DeserializeObject<DealershipVehicleInfo>(vehicleInfoJson);

                isMenuOpen = true;
                API.SetNuiFocus(true, true);

                API.SendNuiMessage(JsonConvert.SerializeObject(new
                {
                    action = "openDealership",
                    payload = currentVehicleInfo
                }));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DealershipClient] Fehler beim Öffnen des Menüs: {ex.Message}");
            }
        }

        private void CloseMenu()
        {
            if (isMenuOpen)
            {
                isMenuOpen = false;
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeDealership" }));
                currentVehicleInfo = null;
            }
        }
    }
}