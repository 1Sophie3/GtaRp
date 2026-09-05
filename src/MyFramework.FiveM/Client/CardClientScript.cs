using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class CardClientScript : BaseScript
    {
        public CardClientScript()
        {
            EventHandlers["cards:client:showIdCard"] += new Action<string>(ShowIdCard);
            EventHandlers["cards:client:showLicense"] += new Action<string>(ShowLicense);
        }

        private void ShowIdCard(string jsonData)
        {
            var data = JsonConvert.DeserializeObject(jsonData);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "showIdCard",
                data = data
            }));
        }

        private void ShowLicense(string jsonData)
        {
            var data = JsonConvert.DeserializeObject(jsonData);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "showLicense",
                data = data
            }));
        }
    }
}