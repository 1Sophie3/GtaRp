using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class FishingClientScript : BaseScript
    {
        private bool isFishing = false;
        private bool isPromptOpen = false;
        private int fishingRodProp = 0;

        // Tasten-Sperren während des Angelns (wie in disableControls)
        private readonly Control[] disabledControls = new Control[]
        {
            Control.Attack, Control.Aim, Control.SelectWeapon, 
            Control.VehicleAttack, Control.VehiclePassengerAttack, 
            Control.VehicleFlyAttack, Control.MeleeAttackLight, 
            Control.MeleeAttackHeavy, Control.MeleeAttackAlternate, 
            Control.Attack2
        };

        public FishingClientScript()
        {
            // Server Events
            EventHandlers["fishing:client:start"] += new Action(StartFishing);
            EventHandlers["fishing:client:stop"] += new Action(StopFishing);
            EventHandlers["fishing:client:showContinuePrompt"] += new Action(ShowContinuePrompt);

            // NUI Callback
            RegisterNuiCallbackType("fishing:promptResponse");
            EventHandlers["__cfx_nui:fishing:promptResponse"] += new Action<dynamic, CallbackDelegate>(OnPromptResponse);

            // Tick für Tastensteuerung und Input
            Tick += OnTick;
        }

        private async void StartFishing()
        {
            if (isFishing) return;
            isFishing = true;

            Ped playerPed = Game.Player.Ped;

            // Scenario/Animation starten
            API.TaskStartScenarioInPlace(playerPed.Handle, "WORLD_HUMAN_STAND_FISHING", 0, true);
        }

        private void StopFishing()
        {
            if (!isFishing && !isPromptOpen) return;

            Ped playerPed = Game.Player.Ped;

            // Tasks und Animationen beenden
            API.ClearPedTasks(playerPed.Handle);

            // Falls ein Prop angehängt wurde, entfernen
            if (fishingRodProp != 0 && API.DoesEntityExist(fishingRodProp))
            {
                API.DeleteEntity(ref fishingRodProp);
                fishingRodProp = 0;
            }

            if (isPromptOpen)
            {
                ClosePrompt();
            }

            isFishing = false;
        }

        private void ShowContinuePrompt()
        {
            Ped playerPed = Game.Player.Ped;

            // Scenario stoppen, während UI offen ist
            API.ClearPedTasks(playerPed.Handle);

            isPromptOpen = true;
            API.SetNuiFocus(true, true);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "showContinuePrompt"
            }));
        }

        private void ClosePrompt()
        {
            isPromptOpen = false;
            API.SetNuiFocus(false, false);
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "hideContinuePrompt"
            }));
        }

        private void OnPromptResponse(dynamic data, CallbackDelegate cb)
        {
            bool choice = Convert.ToBoolean(data.choice);

            ClosePrompt();
            TriggerServerEvent("fishing:server:continueResponse", choice);

            if (!choice)
            {
                StopFishing();
            }

            cb("ok");
        }

        private async Task OnTick()
        {
            // 1. Sperre Angriffs-Steuerung während des Angelns
            if (isFishing)
            {
                foreach (var control in disabledControls)
                {
                    Game.DisableControlThisFrame(0, control);
                }
            }

            // 2. Taste 'E' drücken, um die Abfrage zu bestätigen (falls UI offen ist)
            if (isPromptOpen && Game.IsControlJustPressed(0, Control.Context))
            {
                ClosePrompt();
                TriggerServerEvent("fishing:server:continueResponse", true);
            }

            await Task.FromResult(0);
        }
    }
}