using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class GarageClientScript : BaseScript
    {
        private bool isGarageOpen = false;

        public GarageClientScript()
        {
            EventHandlers["Client:Garage:ShowGarageBrowser"] += new Action<string, string, int>(OnShowGarageBrowser);
            EventHandlers["Client:Garage:HideGarageBrowser"] += new Action(HideGarageBrowser);

            RegisterNuiCallbackType("garage:close");
            RegisterNuiCallbackType("garage:spawn");
            RegisterNuiCallbackType("garage:store");

            EventHandlers["__cfx_nui:garage:close"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                TriggerServerEvent("Server:Garage:RequestCloseUI");
                cb("ok");
            });

            EventHandlers["__cfx_nui:garage:spawn"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                if (data.vehId != null)
                {
                    int vehId = Convert.ToInt32(data.vehId);
                    TriggerServerEvent("Server:Garage:SpawnVehicle", vehId);
                }
                cb("ok");
            });

            EventHandlers["__cfx_nui:garage:store"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                if (data.vehId != null)
                {
                    int vehId = Convert.ToInt32(data.vehId);
                    TriggerServerEvent("Server:Garage:StoreVehicle", vehId);
                }
                cb("ok");
            });

            Tick += OnTick;
        }

        private void OnShowGarageBrowser(string inGarageJson, string nearbyParkableVehiclesJson, int maxVehicles)
        {
            if (isGarageOpen) return;

            isGarageOpen = true;
            API.SetNuiFocus(true, true);
            API.DisplayRadar(false);
            API.DisplayHud(false);

            var payload = new
            {
                inGarageVehicles = JsonConvert.DeserializeObject(inGarageJson),
                nearbyParkableVehicles = JsonConvert.DeserializeObject(nearbyParkableVehiclesJson),
                maxCapacity = maxVehicles
            };

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openGarage",
                payload = payload
            }));
        }

        private void HideGarageBrowser()
        {
            if (isGarageOpen)
            {
                isGarageOpen = false;
                API.SetNuiFocus(false, false);
                API.DisplayRadar(true);
                API.DisplayHud(true);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeGarage" }));
            }
        }

        private async Task OnTick()
        {
            // Taste E (Keycode 38/Context)
            if (Game.IsControlJustPressed(0, Control.Context))
            {
                if (API.IsChatActive())
                {
                    await Task.FromResult(0);
                    return;
                }

                if (!isGarageOpen)
                {
                    TriggerServerEvent("Client:Garage:RequestOpenUI");
                }
                else
                {
                    TriggerServerEvent("Server:Garage:RequestCloseUI");
                }
            }

            await Task.FromResult(0);
        }
    }
}