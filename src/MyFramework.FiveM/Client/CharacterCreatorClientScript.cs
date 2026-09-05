using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace MyFramework.FiveM.Client
{
    public class CharacterCreatorClientScript : BaseScript
    {
        public CharacterCreatorClientScript()
        {
            // NUI / Client Trigger Handlers
            RegisterNuiCallbackType("client:charcreator-create");
            RegisterNuiCallbackType("client:charcreator-setgender");
            RegisterNuiCallbackType("charcreator-camera");
            RegisterNuiCallbackType("client:charcreator-resetcloths");
            RegisterNuiCallbackType("client:charcreator-preview");
            RegisterNuiCallbackType("client:charcreator-preview2");

            EventHandlers["__cfx_nui:client:charcreator-create"] += new Action<dynamic, CallbackDelegate>(OnCreateCharacter);
            EventHandlers["__cfx_nui:client:charcreator-setgender"] += new Action<dynamic, CallbackDelegate>(OnSetGender);
            EventHandlers["__cfx_nui:charcreator-camera"] += new Action<dynamic, CallbackDelegate>(OnSetCamera);
            EventHandlers["__cfx_nui:client:charcreator-resetcloths"] += new Action<dynamic, CallbackDelegate>(OnResetCloths);
            EventHandlers["__cfx_nui:client:charcreator-preview"] += new Action<dynamic, CallbackDelegate>(OnPreview);
            EventHandlers["__cfx_nui:client:charcreator-preview2"] += new Action<dynamic, CallbackDelegate>(OnPreview2);
        }

        private void OnCreateCharacter(dynamic data, CallbackDelegate cb)
        {
            string characterJson = data.ToString();
            // Server Event triggern, um Charakter zu speichern
            TriggerServerEvent("character:server:create", characterJson);
            
            // NUI ausblenden und Fokus zurücksetzen
            API.SetNuiFocus(false, false);
            cb("ok");
        }

        private void OnSetGender(dynamic data, CallbackDelegate cb)
        {
            string gender = data.ToString();
            uint modelHash = (gender == "Männlich") 
                ? (uint)Game.GenerateHash("mp_m_freemode_01") 
                : (uint)Game.GenerateHash("mp_f_freemode_01");

            // Player Model wechseln
            API.RequestModel(modelHash);
            // In Produktion mit async/await auf Model-Ladung warten
            API.SetPlayerModel(Game.Player.Handle, modelHash);
            API.SetModelAsNoLongerNeeded(modelHash);

            cb("ok");
        }

        private void OnSetCamera(dynamic data, CallbackDelegate cb)
        {
            int cameraPos = Convert.ToInt32(data);
            // Logik zur Ausrichtung der Kamera auf den Spieler/Kopf
            cb("ok");
        }

        private void OnResetCloths(dynamic data, CallbackDelegate cb)
        {
            int ped = API.PlayerPedId();
            API.SetPedDefaultComponentVariation(ped);
            cb("ok");
        }

        private void OnPreview(dynamic data, CallbackDelegate cb)
        {
            // Einzelne Modifikationen wie Haarstil, Kleidung oder Gesichtszüge anwenden
            cb("ok");
        }

        private void OnPreview2(dynamic data, CallbackDelegate cb)
        {
            // HeadOverlays und Farben aktualisieren
            cb("ok");
        }
    }
}