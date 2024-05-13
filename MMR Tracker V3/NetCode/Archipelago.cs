using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Microsoft.FSharp.Control;
using MMR_Tracker_V3.SpoilerLogHandling;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using static MMR_Tracker_V3.NetCode.NetData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

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
            try { result = Session.TryConnectAndLogin(Game, Slot, ItemsHandlingFlags.AllItems, new Version(0,4,5), ["Tracker"], null, Password, true); }
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

    public class ArchipelagoConnectionHandler(NetSessionData Data)
    {
        Dictionary<string, string> APLocationIDLookup = null;
        Dictionary<string, string> APItemIDLookup = null;
        private void FillLocationLookupDict() 
        {
            APLocationIDLookup = [];
            foreach (var i in Data.InstanceContainer.Instance.LocationPool) 
            {
                foreach(var NetID in i.Value.GetDictEntry().SpoilerData.NetIDs)
                {
                    APLocationIDLookup[NetID] = i.Key;
                }
            }
        }
        private LocationData.LocationObject GetLocationByNetID(string netID)
        {
            if (netID is null || !APLocationIDLookup.TryGetValue(netID, out string LocID)) { return null; }  
            return Data.InstanceContainer.Instance.GetLocationByID(LocID);
        }
        private void FillItemLookupDict()
        {
            APItemIDLookup = [];
            foreach (var i in Data.InstanceContainer.Instance.ItemPool)
            {
                foreach(var NetID in i.Value.GetDictEntry().SpoilerData.NetIDs)
                {
                    APItemIDLookup[NetID] = i.Key;
                }
            }
        }
        private ItemData.ItemObject GetItemByNetID(string netID)
        {
            if (netID is null || !APItemIDLookup.TryGetValue(netID, out string LocID)) { return null; }
            return Data.InstanceContainer.Instance.GetItemByID(LocID);
        }
        public bool Connect(out List<string> Log)
        {
            Log = new List<string>();
            string ServerAddress = $"{Data.ServerAddress}:{Data.ServerPort}";
            Data.InstanceContainer.netConnection.ArchipelagoClient =
                new ArchipelagoConnector(Data.GameName, Data.SlotID, Data.Password, ServerAddress);

            if (!Data.InstanceContainer.netConnection.ArchipelagoClient.WasConnectionSuccess(out string[] Error))
            {
                Log.Add(string.Join("\n", Error));
                Data.InstanceContainer.netConnection.ArchipelagoClient = null;
                return false;
            }
            var APClient = Data.InstanceContainer.netConnection.ArchipelagoClient;
            var ConnectionInfo = APClient.GetLoginSuccessInfo();
            Log.Add($"Connected to {ServerAddress}");
            Data.InstanceContainer.netConnection.OnlineMode = OnlineMode.Archipelago;
            Data.InstanceContainer.netConnection.PlayerID = Data.InstanceContainer.netConnection.ArchipelagoClient.Session.ConnectionInfo.Slot;
            Data.InstanceContainer.netConnection.SlotID = Data.SlotID;
            Data.InstanceContainer.netConnection.GameName = Data.GameName;

            Data.InstanceContainer.netConnection.PlayerNames = APClient.Session.Players.AllPlayers.ToDictionary(x => x.Slot, x => x.Name);
            
            return true;
        }

        public void ActivateListers()
        {
            var APClient = Data.InstanceContainer.netConnection.ArchipelagoClient;
            APClient.Session.Items.ItemReceived += ArchipelagoItemReceived;
            APClient.Session.Locations.CheckedLocationsUpdated += ArchipelagoLocationChecked;
            APClient.Session.MessageLog.OnMessageReceived += ArchipelagoChatMessageReceived;
        }

        public void SyncWithArchipelagoData(bool FromListener = false)
        {
            if (APLocationIDLookup is null) { FillLocationLookupDict(); }
            if (APItemIDLookup is null) { FillItemLookupDict(); }
            var Instance = Data.InstanceContainer.Instance;
            var Session = Data.InstanceContainer.netConnection.ArchipelagoClient.Session;
            var AllItems = Session.Items.AllItemsReceived.Select(x => (Session.Items.GetItemName(x.Item), Session.Locations.GetLocationNameFromId(x.Location), x.Player)).ToArray();
            var AllLocations = Session.Locations.AllLocationsChecked.Select(x => Session.Locations.GetLocationNameFromId(x)).ToArray();
            foreach (var i in Data.InstanceContainer.Instance.ItemPool.Values) { i.AmountAquiredOnline = []; }
            List<LocationData.LocationObject> ToCheck = new List<LocationData.LocationObject>();
            foreach (var Entry in AllItems)
            {
                bool IsLocal = Entry.Player == Data.InstanceContainer.netConnection.PlayerID;
                var location = IsLocal ? GetLocationByNetID(Entry.Item2) : null;
                var Item = GetItemByNetID(Entry.Item1);
                if (!IsLocal && Item is not null)
                {
                    Item.AmountAquiredOnline.SetIfEmpty(Entry.Player, 0);
                    Item.AmountAquiredOnline[Entry.Player]++;
                }
                else if (IsLocal && location is not null && location.CheckState != MiscData.CheckState.Checked)
                {
                    location.Randomizeditem.Item = Entry.Item1;
                    ToCheck.Add(location);
                }
            }
            foreach (var Entry in AllLocations)
            {
                var location = GetLocationByNetID(Entry);
                if (location is null) { continue; }
                if (location.GetItemAtCheck() == null) { location.Randomizeditem.Item = "Archipelago Item"; }
                ToCheck.Add(location);
            }
            LocationChecker.CheckSelectedItems(ToCheck, Data.InstanceContainer, new CheckItemSetting(MiscData.CheckState.Checked));

            Hint[] HintData = [];

            //TODO Fix this eventually, GetHints breaks when called from a PacketReceived handler and GetHintsAsync causes an infinite loop
            //For now just don't call it from a PacketReceived handler
            if (!FromListener)
            {
                try { HintData = Session.DataStorage.GetHints(Session.ConnectionInfo.Slot); }
                catch (Exception e) { Debug.WriteLine(e); }

                Instance.GetParentContainer().netConnection.RemoteHints.Clear();
                List<LocationData.LocationObject> HintedLocations = [];
                foreach (var i in HintData)
                {
                    if (i.FindingPlayer == Session.ConnectionInfo.Slot)
                    {
                        var HintLocation = GetLocationByNetID(Session.Locations.GetLocationNameFromId(i.LocationId));
                        if (HintLocation is null) { continue; };
                        if (HintLocation.CheckState == MiscData.CheckState.Unchecked && HintLocation.GetItemAtCheck() is not null)
                        {
                            HintedLocations.Add(HintLocation);
                        }
                    }
                    else if (i.ReceivingPlayer == Session.ConnectionInfo.Slot && !i.Found)
                    {
                        var itemName = Session.Items.GetItemName(i.ItemId);
                        var itemObject = GetItemByNetID(itemName);
                        var locationName = Session.Locations.GetLocationNameFromId(i.LocationId);
                        Instance.GetParentContainer().netConnection.RemoteHints.Add(
                            new HintData.RemoteLocationHint(itemObject, locationName, i.FindingPlayer));
                    }
                }
                LocationChecker.CheckSelectedItems(HintedLocations, Data.InstanceContainer, new CheckItemSetting(MiscData.CheckState.Marked));
            }

            Data.RefreshMainForm();
        }
        public void ArchipelagoChatMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            if (!Data.ReceiveData) { return; }
            if (message is Archipelago.MultiClient.Net.MessageLog.Messages.HintItemSendLogMessage && Data.AutoProcessData)
            {
                SyncWithArchipelagoData(true);
            }
            Data.Logger?.Invoke(message.ToString(), null);
        }

        public void ArchipelagoLocationChecked(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            Debug.WriteLine("ArchipelagoLocationChecked");
            if (!Data.AutoProcessData || !Data.ReceiveData) { return; }
            SyncWithArchipelagoData(true);
        }

        public void ArchipelagoItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            Debug.WriteLine("ArchipelagoItemReceived");
            if (!Data.AutoProcessData || !Data.ReceiveData) { return; }
            SyncWithArchipelagoData(true);
        }

        public static bool CheckForLocalAPServer()
        {
            foreach (Process process in Process.GetProcesses())
            { 
                if (!String.IsNullOrEmpty(process.MainWindowTitle) && 
                    process.ProcessName == "ArchipelagoServer" && 
                    process.MainWindowTitle.EndsWith("ArchipelagoServer.exe"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
