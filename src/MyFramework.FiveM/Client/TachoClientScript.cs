using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class TachoClientScript : BaseScript
    {
        private bool isTachoActive = false;

        public TachoClientScript()
        {
            EventHandlers["Tacho:Update"] += new Action<string>(OnTachoUpdate);
            Tick += OnTick;
        }

        private void OnTachoUpdate(string payloadJson)
        {
            if (isTachoActive)
            {
                API.SendNuiMessage(JsonConvert.SerializeObject(new
                {
                    action = "updateTacho",
                    payload = JsonConvert.DeserializeObject(payloadJson)
                }));
            }
        }

        private async Task OnTick()
        {
            int ped = API.PlayerPedId();
            bool inVehicle = API.IsPedInAnyVehicle(ped, false);
            int vehicle = inVehicle ? API.GetVehiclePedIsIn(ped, false) : 0;
            bool isDriver = inVehicle && API.GetPedInVehicleSeat(vehicle, -1) == ped;

            if (isDriver && !isTachoActive)
            {
                isTachoActive = true;
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "showTacho", payload = true }));
            }
            else if (!isDriver && isTachoActive)
            {
                isTachoActive = false;
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "showTacho", payload = false }));
            }

            await Delay(1000);
        }
    }
}