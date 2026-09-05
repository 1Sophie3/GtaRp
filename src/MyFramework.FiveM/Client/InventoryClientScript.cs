using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class InventoryClientScript : BaseScript
    {
        private bool _isInventoryOpen = false;

        public InventoryClientScript()
        {
            // Event vom Server empfangen (Inventar-Daten laden & HUD öffnen)
            EventHandlers["inventory:client:open"] += new Action<string>(OpenInventory);

            // NUI Callbacks registrieren (Frontend -> C# Client)
            API.RegisterNuiCallbackType("closeInventory");
            EventHandlers["__cfx_nui:closeInventory"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                ToggleInventoryFocus(false);
                cb(new { ok = true });
            });

            API.RegisterNuiCallbackType("triggerItemAction");
            EventHandlers["__cfx_nui:triggerItemAction"] += new Action<IDictionary<string, object>, CallbackDelegate>((data, cb) =>
            {
                string action = data["action"].ToString();
                int itemId = Convert.ToInt32(data["itemId"]);

                // Event an den Server senden (z. B. Item nutzen/wegwerfen)
                TriggerServerEvent("inventory:server:itemAction", action, itemId);
                cb(new { ok = true });
            });

            // Keybinding: Taste "I" öffnet/schließt das Inventar
            RegisterCommand("openInventory", new Action(() =>
            {
                if (!_isInventoryOpen)
                {
                    TriggerServerEvent("inventory:server:requestOpen");
                }
                else
                {
                    CloseInventory();
                }
            }), false);
            
            API.RegisterKeyMapping("openInventory", "Inventar öffnen", "keyboard", "I");
        }

        private void OpenInventory(string inventoryJson)
        {
            ToggleInventoryFocus(true);
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openInventory",
                data = JsonConvert.DeserializeObject(inventoryJson)
            }));
        }

        private void CloseInventory()
        {
            ToggleInventoryFocus(false);
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "closeInventory"
            }));
        }

        private void ToggleInventoryFocus(bool enable)
        {
            _isInventoryOpen = enable;
            API.SetNuiFocus(enable, enable); // Mauszeiger & Tastatur-Fokus für NUI steuern
        }
    }
}