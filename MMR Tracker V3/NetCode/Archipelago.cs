﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using MMR_Tracker_V3.SpoilerLogImporter;
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

    public class ArchipelagoConnectionHandler(NetSessionData Data)
    {
        Dictionary<string, string> APLocationIDLookup = null;
        Dictionary<string, string> APItemIDLookup = null;
        private void FillLocationLookupDict() 
        {
            APLocationIDLookup = [];
            foreach (var i in Data.InstanceContainer.Instance.LocationPool) 
            {
                var NetID = i.Value.GetDictEntry().SpoilerData.NetID;
                if (NetID is null) { continue; }
                APLocationIDLookup[NetID] = i.Key;
            }
        }
        private LocationData.LocationObject GetLocationByNetID(string netID)
        {
            if (!APLocationIDLookup.TryGetValue(netID, out string LocID)) { return null; }  
            return Data.InstanceContainer.Instance.GetLocationByID(LocID);
        }
        private void FillItemLookupDict()
        {
            APItemIDLookup = [];
            foreach (var i in Data.InstanceContainer.Instance.ItemPool)
            {
                var NetID = i.Value.GetDictEntry().SpoilerData.NetID;
                if (NetID is null) { continue; }
                APItemIDLookup[NetID] = i.Key;
            }
        }
        private ItemData.ItemObject GetItemByNetID(string netID)
        {
            if (!APItemIDLookup.TryGetValue(netID, out string LocID)) { return null; }
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

        public void ApplySpoilerFromAPData()
        {
            var SpoilerLog = SpoilerLogImporter.Archipelago.CreateGenericSpoilerLog(Data.InstanceContainer);
            var LogImported = SpoilerLogTools.ImportSpoilerLog(SpoilerLog, "", Data.InstanceContainer);
            Data.InstanceContainer.logicCalculation.CalculateLogic();
        }

        public void SyncWithArchipelagoData()
        {
            if (APLocationIDLookup is null) { FillLocationLookupDict(); }
            if (APItemIDLookup is null) { FillItemLookupDict(); }
            var Instance = Data.InstanceContainer.Instance;
            var Sess = Data.InstanceContainer.netConnection.ArchipelagoClient.Session;
            var AllItems = Sess.Items.AllItemsReceived.Select(x => (Sess.Items.GetItemName(x.Item), Sess.Locations.GetLocationNameFromId(x.Location), x.Player)).ToArray();
            var AllLocations = Sess.Locations.AllLocationsChecked.Select(x => Sess.Locations.GetLocationNameFromId(x)).ToArray();
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
            Data.RefreshMainForm();
        }
        public void ArchipelagoChatMessageReceived(Archipelago.MultiClient.Net.MessageLog.Messages.LogMessage message)
        {
            Data.Logger?.Invoke(message.ToString(), null);
        }

        public void ArchipelagoLocationChecked(System.Collections.ObjectModel.ReadOnlyCollection<long> newCheckedLocations)
        {
            Debug.WriteLine("ArchipelagoLocationChecked");
            if (!Data.AutoProcessData) { return; }
            SyncWithArchipelagoData();
        }

        public void ArchipelagoItemReceived(Archipelago.MultiClient.Net.Helpers.ReceivedItemsHelper helper)
        {
            Debug.WriteLine("ArchipelagoItemReceived");
            if (!Data.AutoProcessData) { return; }
            SyncWithArchipelagoData();
        }
    }
}