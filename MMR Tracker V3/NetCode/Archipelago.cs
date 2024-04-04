using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using static System.Collections.Specialized.BitVector32;

namespace MMR_Tracker_V3.NetCode
{
    public class ArchipelagoConnector
    {
        public ArchipelagoSession Session;
        LoginResult result;
        List<string> errorMessage = [];
        public ArchipelagoConnector(string Game, string Slot, string Pass, string Address)
        {
            errorMessage = [];
            int Port = 38281;
            if (Address.Contains(":") && int.TryParse(Address.SplitOnce(':').Item2, out int NewPort))
            {
                Address = Address.SplitOnce(':').Item1;
                Port = NewPort;
            }
            if (string.IsNullOrWhiteSpace(Address)) { Address = "127.0.0.1"; }
            string Password = string.IsNullOrWhiteSpace(Pass) ? null : Pass;
            Session = ArchipelagoSessionFactory.CreateSession(Address, Port);
            try { result = Session.TryConnectAndLogin(Game, Slot, ItemsHandlingFlags.AllItems, null, ["Tracker"], null, Password, true); }
            catch (Exception ex) { result = new LoginFailure(ex.GetBaseException().Message); }
            if (!result.Successful)
            {
                LoginFailure failure = (LoginFailure)result;
                errorMessage.Add($"Failed to Connect to {Address} as {Slot}:");
                foreach (string error in failure.Errors) { errorMessage.Add($"-{error}"); }
                foreach (ConnectionRefusedError error in failure.ErrorCodes) { errorMessage.Add($"-{error}"); }
            }
        }
        public bool WasConnectionSuccess(out string[] Error) { Error = [.. errorMessage]; return result is LoginSuccessful; }
        public LoginSuccessful GetLoginSuccessInfo() { return (LoginSuccessful)result; }
        public LoginFailure GetLoginFailureInfo() { return (LoginFailure)result; }

        public string[] GetAllLocations()
        {
            var AllLocations = Session.Locations.AllLocations.ToArray();
            var AllLocationNames = AllLocations.Select(x => Session.Locations.GetLocationNameFromId(x));
            return [.. AllLocationNames];
        }
        public string[] GetAllItems()
        {
            var AllLocations = Session.Items.AllItemsReceived.ToArray();
            var AllLocationNames = AllLocations.Select(x => $"{Session.Items.GetItemName(x.Item)} ({Session.Players.GetPlayerAliasAndName(x.Player)})");
            return [.. AllLocationNames];
        }
    }
}
