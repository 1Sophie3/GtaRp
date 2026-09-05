using System;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using Newtonsoft.Json;

namespace MyFramework.FiveM.Client
{
    public class BankClientScript : BaseScript
    {
        private bool isBankOpen = false;

        public BankClientScript()
        {
            // Register NUI Callbacks
            RegisterNuiCallbackType("bank:close");
            RegisterNuiCallbackType("bank:deposit");
            RegisterNuiCallbackType("bank:withdraw");
            RegisterNuiCallbackType("bank:transfer");

            // Attach Handlers
            EventHandlers["__cfx_nui:bank:close"] += new Action<dynamic, CallbackDelegate>(OnNuiClose);
            EventHandlers["__cfx_nui:bank:deposit"] += new Action<dynamic, CallbackDelegate>(OnNuiDeposit);
            EventHandlers["__cfx_nui:bank:withdraw"] += new Action<dynamic, CallbackDelegate>(OnNuiWithdraw);
            EventHandlers["__cfx_nui:bank:transfer"] += new Action<dynamic, CallbackDelegate>(OnNuiTransfer);

            // Server Event Listeners
            EventHandlers["bank:client:openMenu"] += new Action<int, int, string, dynamic>(OpenMenu);
            EventHandlers["bank:client:updateBalances"] += new Action<int, int>(UpdateBalances);
            EventHandlers["bank:client:closeMenu"] += new Action(CloseMenu);
        }

        private void OpenMenu(int cash, int bankBalance, string accountNumber, dynamic transactions)
        {
            isBankOpen = true;

            // Set UI Focus
            API.SetNuiFocus(true, true);

            // Forward Data to Vue
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "openBank",
                payload = new
                {
                    cash,
                    bankBalance,
                    accountNumber,
                    transactions
                }
            }));
        }

        private void UpdateBalances(int cash, int bankBalance)
        {
            API.SendNuiMessage(JsonConvert.SerializeObject(new
            {
                action = "updateBalances",
                payload = new { cash, bankBalance }
            }));
        }

        private void CloseMenu()
        {
            isBankOpen = false;
            API.SetNuiFocus(false, false);
            API.SendNuiMessage(JsonConvert.SerializeObject(new { action = "closeBank" }));
        }

        #region NUI Callbacks

        private void OnNuiClose(dynamic data, CallbackDelegate cb)
        {
            CloseMenu();
            TriggerServerEvent("bank:server:requestClose");
            cb("ok");
        }

        private void OnNuiDeposit(dynamic data, CallbackDelegate cb)
        {
            int amount = Convert.ToInt32(data.amount);
            TriggerServerEvent("bank:server:deposit", amount);
            cb("ok");
        }

        private void OnNuiWithdraw(dynamic data, CallbackDelegate cb)
        {
            int amount = Convert.ToInt32(data.amount);
            TriggerServerEvent("bank:server:withdraw", amount);
            cb("ok");
        }

        private void OnNuiTransfer(dynamic data, CallbackDelegate cb)
        {
            string targetAccount = Convert.ToString(data.targetAccount);
            int amount = Convert.ToInt32(data.amount);
            string description = Convert.ToString(data.description);

            TriggerServerEvent("bank:server:transfer", targetAccount, amount, description);
            cb("ok");
        }

        #endregion
    }
}