using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class VehicleWheelClientScript : BaseScript
    {
        private bool isWheelActive = false;
        private string hoveredAction = null;
        private dynamic currentMenuItems = null;

        public VehicleWheelClientScript()
        {
            EventHandlers["showVehicleWheelMenu"] += new Action<string, string>(OnShowVehicleWheelMenu);

            RegisterNuiCallbackType("wheel:hover");
            RegisterNuiCallbackType("wheel:action");
            RegisterNuiCallbackType("wheel:close");

            EventHandlers["__cfx_nui:wheel:hover"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                hoveredAction = data.action?.ToString();
                cb("ok");
            });

            EventHandlers["__cfx_nui:wheel:action"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                string action = data.action?.ToString();
                ExecuteAction(action);
                cb("ok");
            });

            EventHandlers["__cfx_nui:wheel:close"] += new Action<dynamic, CallbackDelegate>((data, cb) =>
            {
                HideWheelMenu();
                cb("ok");
            });

            Tick += OnTick;
        }

        private int GetTargetVehicle()
        {
            int ped = API.PlayerPedId();
            if (API.IsPedInAnyVehicle(ped, false))
            {
                return API.GetVehiclePedIsIn(ped, false);
            }

            Vector3 pos = API.GetEntityCoords(ped, true);
            return API.GetClosestVehicle(pos.X, pos.Y, pos.Z, 5.0f, 0, 70);
        }

        private void OnShowVehicleWheelMenu(string menuItemsJson, string centerText)
        {
            if (!isWheelActive) return;

            currentMenuItems = JsonConvert.DeserializeObject(menuItemsJson);
            API.SetNuiFocus(true, true);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openWheel",
                payload = new
                {
                    items = currentMenuItems,
                    centerText = centerText
                }
            }));
        }

        private void ExecuteAction(string action)
        {
            int veh = GetTargetVehicle();
            if (veh != 0)
            {
                TriggerServerEvent("vehicleWheelMenuAction", API.VehToNet(veh), action);
            }
            HideWheelMenu();
        }

        private void HideWheelMenu()
        {
            if (isWheelActive)
            {
                isWheelActive = false;
                hoveredAction = null;
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeWheel" }));
            }
        }

        private async Task OnTick()
        {
            // Key X (Control 73 = Vehicle Shuffle / Keycode 88)
            bool isXKeyDown = API.IsControlPressed(0, 73);
            int targetVehicle = GetTargetVehicle();

            if (isXKeyDown && targetVehicle != 0 && !isWheelActive)
            {
                isWheelActive = true;
                TriggerServerEvent("requestVehicleWheelMenu", API.VehToNet(targetVehicle));
            }
            else if ((!isXKeyDown || targetVehicle == 0) && isWheelActive)
            {
                if (!string.IsNullOrEmpty(hoveredAction))
                {
                    ExecuteAction(hoveredAction);
                }
                HideWheelMenu();
            }

            await Task.FromResult(0);
        }
    }
}