using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class NotificationClientScript : BaseScript
    {
        public NotificationClientScript()
        {
            // Event, das vom C#-Server aufgerufen wird
            EventHandlers["notifications:client:show"] += new Action<string, string, bool>(ShowNotification);
        }

        public void ShowNotification(string text, string type = "success", bool icon = true)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "showNotification",
                text = text,
                type = type,
                icon = icon
            }));
        }
    }
}