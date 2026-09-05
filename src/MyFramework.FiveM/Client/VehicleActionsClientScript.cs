using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace MyFramework.FiveM.Client
{
    public class VehicleActionsClientScript : BaseScript
    {
        private bool seatbeltActive = false;
        private float lastSpeed = 0f;

        public VehicleActionsClientScript()
        {
            EventHandlers["updateSeatbelt"] += new Action<bool>((status) => seatbeltActive = status);

            Tick += MonitorCrashTick;
            Tick += VehicleSeatEnterTick;
        }

        // ==========================================
        // 1. Crash & Seatbelt Monitor
        // ==========================================
        private async Task MonitorCrashTick()
        {
            int ped = API.PlayerPedId();
            if (!API.IsPedInAnyVehicle(ped, false))
            {
                lastSpeed = 0f;
                seatbeltActive = false;
                await Delay(500);
                return;
            }

            int vehicle = API.GetVehiclePedIsIn(ped, false);
            if (API.GetPedInVehicleSeat(vehicle, -1) == ped)
            {
                float currentSpeed = API.GetEntitySpeed(vehicle); // M/S
                float bodyHealth = API.GetVehicleBodyHealth(vehicle);

                const float minCrashSpeed = 35.0f;
                const float decelerationThreshold = 35.0f;
                const float highSpeedThreshold = 42.0f;

                if (lastSpeed >= minCrashSpeed && (lastSpeed - currentSpeed) >= decelerationThreshold)
                {
                    if (!seatbeltActive)
                    {
                        TriggerServerEvent("handlePlayerCrash", bodyHealth);
                    }

                    if (lastSpeed >= highSpeedThreshold)
                    {
                        API.SetVehicleEngineOn(vehicle, false, true, true);
                        TriggerEvent("chat:addMessage", new { args = new[] { "System", "Fahrzeug stark beschädigt! Motor wurde abgeschaltet." } });
                    }
                }

                lastSpeed = currentSpeed;
            }

            await Delay(100);
        }

        // ==========================================
        // 2. Erweitertes Einsteigen auf Beifahrersitz (G-Taste / Keycode 47)
        // ==========================================
        private async Task VehicleSeatEnterTick()
        {
            // Key 47 = Control Detonate / G
            if (API.IsControlJustPressed(0, 47))
            {
                int ped = API.PlayerPedId();
                if (!API.IsPedInAnyVehicle(ped, false))
                {
                    Vector3 pos = API.GetEntityCoords(ped, true);
                    int vehicle = API.GetClosestVehicle(pos.X, pos.Y, pos.Z, 6.0f, 0, 70);

                    if (vehicle != 0)
                    {
                        if (API.GetVehicleDoorLockStatus(vehicle) > 1) return;

                        // Freie Sitzplätze ermitteln
                        int freeSeat = -1;
                        int maxSeats = API.GetVehicleMaxNumberOfPassengers(vehicle);

                        for (int i = 0; i < maxSeats; i++)
                        {
                            if (API.IsVehicleSeatFree(vehicle, i))
                            {
                                freeSeat = i;
                                break;
                            }
                        }

                        if (freeSeat != -1)
                        {
                            API.TaskEnterVehicle(ped, vehicle, 5000, freeSeat, 2.0f, 1, 0);
                        }
                    }
                }
            }

            await Task.FromResult(0);
        }
    }
}