using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class DeathClientScript : BaseScript
    {
        public DeathClientScript()
        {
            // Vom Server getriggerte Events für den Todesbildschirm
            EventHandlers["death:client:startEffect"] += new Action<int>(StartDeathEffect);
            EventHandlers["death:client:stopEffect"] += new Action(StopDeathEffect);
            EventHandlers["death:client:updateTimer"] += new Action<int>(UpdateDeathTimer);
        }

        private void StartDeathEffect(int seconds)
        {
            // GTA V Bildschirmeffekt aktivieren
            Function.Call(Hash.ANIMPOSTFX_PLAY, "DeathFailMPDark", 0, true);

            // NUI Nachricht an Vue 3 Frontend senden
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "startDeathEffect",
                seconds = seconds
            }));
        }

        private void StopDeathEffect()
        {
            // GTA V Bildschirmeffekt stoppen
            Function.Call(Hash.ANIMPOSTFX_STOP, "DeathFailMPDark");

            // Vue 3 Frontend ausblenden
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "stopDeathEffect"
            }));
        }

        private void UpdateDeathTimer(int secondsRemaining)
        {
            // Countdown-Timer im Vue 3 Frontend aktualisieren
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "updateDeathTimer",
                seconds = secondsRemaining
            }));
        }
    }
}