using System;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class AdminPanelClientScript : BaseScript
    {
        private bool isPanelOpen = false;
        
        // Spectate Variables
        private bool isSpectating = false;
        private int spectateTargetServerId = -1;
        private int spectateCamera = 0;

        // Noclip Variables
        private bool isNoclipActive = false;
        private int noclipCamera = 0;

        public AdminPanelClientScript()
        {
            // Server Events
            EventHandlers["adminPanel:show"] += new Action<int, int, string, string, string, string>(ShowAdminPanel);
            EventHandlers["admin:doFreeze"] += new Action<bool>(DoFreeze);
            EventHandlers["spectate:start"] += new Action<int>(StartSpectate);
            EventHandlers["spectate:stop"] += new Action(StopSpectate);
            EventHandlers["noclip:toggle"] += new Action(ToggleNoclip);

            // NUI Callbacks
            RegisterNuiCallbackType("admin:closePanel");
            RegisterNuiCallbackType("admin:goto");
            RegisterNuiCallbackType("admin:gethere");
            RegisterNuiCallbackType("admin:heal");
            RegisterNuiCallbackType("admin:revive");
            RegisterNuiCallbackType("admin:kick");
            RegisterNuiCallbackType("admin:ban");
            RegisterNuiCallbackType("admin:unban");
            RegisterNuiCallbackType("admin:giveMoney");
            RegisterNuiCallbackType("admin:giveWeapon");
            RegisterNuiCallbackType("admin:toggleSpectate");
            RegisterNuiCallbackType("admin:toggleFreeze");
            RegisterNuiCallbackType("admin:setPlayerDimension");
            RegisterNuiCallbackType("admin:toggleAduty");
            RegisterNuiCallbackType("admin:toggleInvisibility");
            RegisterNuiCallbackType("admin:spawnAdminVehicle");
            RegisterNuiCallbackType("admin:goBack");
            RegisterNuiCallbackType("admin:toggleGodMode");
            RegisterNuiCallbackType("admin:toggleNoClip");
            RegisterNuiCallbackType("admin:teleportToCoords");
            RegisterNuiCallbackType("admin:teleportToLocation");
            RegisterNuiCallbackType("admin:teleportToHouse");
            RegisterNuiCallbackType("admin:setHouseOwner");
            RegisterNuiCallbackType("admin:sendAdminChat");
            RegisterNuiCallbackType("admin:sendAnnouncement");
            RegisterNuiCallbackType("admin:spawnTempVehicle");
            RegisterNuiCallbackType("admin:createPersVehicle");
            RegisterNuiCallbackType("admin:createFactionVehicle");
            RegisterNuiCallbackType("admin:tptoVehicle");
            RegisterNuiCallbackType("admin:parkVehicleInAlta");
            RegisterNuiCallbackType("admin:deleteVehicleDB");
            RegisterNuiCallbackType("admin:fetchVehicle");
            RegisterNuiCallbackType("admin:repairVehicle");
            RegisterNuiCallbackType("admin:forceToggleLock");
            RegisterNuiCallbackType("admin:forceToggleEngine");
            RegisterNuiCallbackType("admin:support:updateStatus");
            RegisterNuiCallbackType("admin:support:setPriority");
            RegisterNuiCallbackType("admin:support:addComment");

            // Attach Callback Listeners
            RegisterNuiHandlers();

            Tick += OnTick;
        }

        private void ShowAdminPanel(int adminLevel, int ownAccountId, string playersJson, string housesJson, string tpLocationsJson, string ticketsJson)
        {
            isPanelOpen = true;
            API.SetNuiFocus(true, true);

            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openAdminPanel",
                payload = new
                {
                    adminLevel,
                    ownAccountId,
                    players = playersJson,
                    houses = housesJson,
                    tpLocations = tpLocationsJson,
                    tickets = ticketsJson
                }
            }));
        }

        private void CloseAdminPanel()
        {
            if (isPanelOpen)
            {
                isPanelOpen = false;
                API.SetNuiFocus(false, false);
                API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeAdminPanel" }));
            }
        }

        private void DoFreeze(bool freeze)
        {
            API.FreezeEntityPosition(API.PlayerPedId(), freeze);
        }

        #region Spectate System

        private void StartSpectate(int targetServerId)
        {
            int targetPed = API.GetPlayerPed(API.GetPlayerFromServerId(targetServerId));
            if (API.DoesEntityExist(targetPed))
            {
                isSpectating = true;
                spectateTargetServerId = targetServerId;
                
                int playerPed = API.PlayerPedId();
                API.SetEntityVisible(playerPed, false, false);
                API.SetEntityInvincible(playerPed, true);
                API.SetEntityCollision(playerPed, false, true);

                Vector3 pos = API.GetEntityCoords(playerPed, true);
                spectateCamera = API.CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", pos.X, pos.Y, pos.Z, 0, 0, 0, 50, true, 2);
                API.SetCamActive(spectateCamera, true);
                API.RenderScriptCams(true, false, 0, true, false);
            }
        }

        private void StopSpectate()
        {
            isSpectating = false;
            spectateTargetServerId = -1;

            int playerPed = API.PlayerPedId();
            API.SetEntityVisible(playerPed, true, false);
            API.SetEntityInvincible(playerPed, false);
            API.SetEntityCollision(playerPed, true, true);

            if (spectateCamera != 0)
            {
                API.DestroyCam(spectateCamera, false);
                spectateCamera = 0;
            }
            API.RenderScriptCams(false, false, 0, true, false);
        }

        #endregion

        #region Noclip System

        private void ToggleNoclip()
        {
            isNoclipActive = !isNoclipActive;
            int playerPed = API.PlayerPedId();

            API.FreezeEntityPosition(playerPed, isNoclipActive);
            API.SetEntityInvincible(playerPed, isNoclipActive);
            API.SetEntityVisible(playerPed, !isNoclipActive, false);
            API.SetEntityCollision(playerPed, !isNoclipActive, !isNoclipActive);

            if (isNoclipActive)
            {
                Vector3 pos = API.GetEntityCoords(playerPed, true);
                noclipCamera = API.CreateCamWithParams("DEFAULT_SCRIPTED_CAMERA", pos.X, pos.Y, pos.Z, 0, 0, 0, 50, true, 2);
                API.SetCamActive(noclipCamera, true);
                API.RenderScriptCams(true, false, 0, false, false);
            }
            else
            {
                if (noclipCamera != 0)
                {
                    API.DestroyCam(noclipCamera, false);
                    noclipCamera = 0;
                }
                API.RenderScriptCams(false, false, 0, true, false);
            }
        }

        #endregion

        private async Task OnTick()
        {
            // F5 - Keybind zum Öffnen
            if (Game.IsControlJustPressed(0, Control.SelectCharacterFranklin)) // F5 Default Equivalent
            {
                if (isPanelOpen)
                {
                    CloseAdminPanel();
                }
                else
                {
                    TriggerServerEvent("adminPanel:requestOpen");
                }
            }

            // Spectate Camera Tracking Loop
            if (isSpectating && spectateTargetServerId != -1)
            {
                int targetPed = API.GetPlayerPed(API.GetPlayerFromServerId(spectateTargetServerId));
                if (API.DoesEntityExist(targetPed))
                {
                    Vector3 targetPos = API.GetEntityCoords(targetPed, true);
                    Vector3 offsetPos = API.GetOffsetFromEntityInWorldCoords(targetPed, 0f, -5.0f, 2.0f);
                    API.SetCamCoord(spectateCamera, offsetPos.X, offsetPos.Y, offsetPos.Z);
                    API.PointCamAtCoord(spectateCamera, targetPos.X, targetPos.Y, targetPos.Z + 1.0f);
                }
            }

            await Task.FromResult(0);
        }

        #region NUI Callback Registration

        private void RegisterNuiHandlers()
        {
            EventHandlers["__cfx_nui:admin:closePanel"] += new Action<dynamic, CallbackDelegate>((data, cb) => { CloseAdminPanel(); cb("ok"); });
            EventHandlers["__cfx_nui:admin:goto"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:goto", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:gethere"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:gethere", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:heal"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:heal", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:revive"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:revivePlayer", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:kick"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:kick", Convert.ToInt32(data.targetId), Convert.ToString(data.reason)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:ban"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:ban", Convert.ToInt32(data.targetId), Convert.ToString(data.reason)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:unban"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:unban", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:giveMoney"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:giveMoney", Convert.ToInt32(data.targetId), Convert.ToInt32(data.amount), Convert.ToString(data.type)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:giveWeapon"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:giveWeapon", Convert.ToInt32(data.targetId), Convert.ToString(data.weapon)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:toggleSpectate"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleSpectate", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:toggleFreeze"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleFreeze", Convert.ToInt32(data.targetId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:setPlayerDimension"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:setPlayerDimension", Convert.ToInt32(data.targetId), Convert.ToInt32(data.dim)); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:toggleAduty"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleAduty"); cb("ok"); });
            EventHandlers["__cfx_nui:admin:toggleInvisibility"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleInvisibility"); cb("ok"); });
            EventHandlers["__cfx_nui:admin:spawnAdminVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:spawnAdminVehicle"); cb("ok"); });
            EventHandlers["__cfx_nui:admin:goBack"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:goBack"); cb("ok"); });
            EventHandlers["__cfx_nui:admin:toggleGodMode"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleGodMode"); cb("ok"); });
            EventHandlers["__cfx_nui:admin:toggleNoClip"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:toggleNoClip"); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:teleportToCoords"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:teleportToCoords", Convert.ToSingle(data.x), Convert.ToSingle(data.y), Convert.ToSingle(data.z)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:teleportToLocation"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:teleportToLocation", Convert.ToString(data.location), Convert.ToBoolean(data.withVeh)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:teleportToHouse"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:teleportToHouse", Convert.ToInt32(data.houseId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:setHouseOwner"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:setHouseOwner", Convert.ToInt32(data.houseId), Convert.ToInt32(data.ownerId)); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:sendAdminChat"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:sendAdminChat", Convert.ToString(data.message)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:sendAnnouncement"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:sendAnnouncement", Convert.ToString(data.message)); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:spawnTempVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:spawnTempVehicle", Convert.ToString(data.model)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:createPersVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:createPersVehicle", Convert.ToString(data.model), Convert.ToInt32(data.ownerId), Convert.ToInt32(data.color1), Convert.ToInt32(data.color2), Convert.ToString(data.plate)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:createFactionVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:createFactionVehicle", Convert.ToString(data.model), Convert.ToInt32(data.factionId), Convert.ToString(data.plate), Convert.ToInt32(data.color1), Convert.ToInt32(data.color2)); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:tptoVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:tptoVehicle", Convert.ToInt32(data.vehId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:parkVehicleInAlta"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:parkVehicleInAlta", Convert.ToInt32(data.vehId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:deleteVehicleDB"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:deleteVehicleDB", Convert.ToInt32(data.vehId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:fetchVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:fetchVehicle", Convert.ToInt32(data.vehId)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:repairVehicle"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:repairVehicle", Convert.ToInt32(data.vehId)); cb("ok"); });
            
            EventHandlers["__cfx_nui:admin:forceToggleLock"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:forceToggleLock", Convert.ToBoolean(data.status)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:forceToggleEngine"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:forceToggleEngine", Convert.ToBoolean(data.status)); cb("ok"); });

            EventHandlers["__cfx_nui:admin:support:updateStatus"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:support:updateStatus", Convert.ToInt32(data.ticketId), Convert.ToString(data.status)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:support:setPriority"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:support:setPriority", Convert.ToInt32(data.ticketId), Convert.ToBoolean(data.isPriority)); cb("ok"); });
            EventHandlers["__cfx_nui:admin:support:addComment"] += new Action<dynamic, CallbackDelegate>((data, cb) => { TriggerServerEvent("adminPanel:support:addComment", Convert.ToInt32(data.ticketId), Convert.ToString(data.comment)); cb("ok"); });
        }

        #endregion
    }
}