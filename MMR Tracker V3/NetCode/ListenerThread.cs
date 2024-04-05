using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.NetCode.NetData;
using static MMR_Tracker_V3.TrackerObjects.InstanceData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.NetCode
{
    public class ListenerThread(NetSessionData Data)
    {
        public Dictionary<string, string> LocationDataToProcess = new Dictionary<string, string>();
        public Dictionary<int, Dictionary<string, int>> ItemDataToProcess = new Dictionary<int, Dictionary<string, int>>();

        public async Task<string> OpenListenThread()
        {
            string ExitReason = "Unknown";
            while (Data.InstanceContainer.netConnection.ServerConnection is not null && Data.InstanceContainer.netConnection.ServerConnection.Connected)
            {
                Debug.WriteLine("Listening for data");
                string dataReceived;
                try
                {
                    byte[] buffer = new byte[Data.InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize];
                    int bytesRead = await Data.InstanceContainer.netConnection.ServerConnection.GetStream().ReadAsync(buffer, 0, Data.InstanceContainer.netConnection.ServerConnection.ReceiveBufferSize);
                    dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                }
                catch (Exception e) { ExitReason = "Server Closed"; break; }

                NetData.NetPacket packet;
                try { packet = JsonConvert.DeserializeObject<NetData.NetPacket>(dataReceived); }
                catch { ExitReason = "Bad Packet From Server"; break; }

                if (packet is null) { ExitReason = "Bad Packet From Server"; break; }
                HandlePacket(packet);
            }
            Debug.WriteLine("Closing Listen Thread");
            return ExitReason;
        }

        public void HandlePacket(NetData.NetPacket? packet)
        {
            if (packet.packetType != NetData.PacketType.ChatMessage && !Data.ReceiveData) { return; }
            Debug.WriteLine($"Got Packet of type {packet.packetType}");
            switch (packet.packetType)
            {
                case NetData.PacketType.OnlineSynedLocations:
                    ParseSharedLocationData(packet);
                    break;
                case NetData.PacketType.MultiWorldItems:
                    ParseMultiWorldData(packet);
                    break;
                case NetData.PacketType.ChatMessage:
                    string PlayerName = packet.ChatMessage.PlayerID < 0 ? "Server" : $"Player {packet.ChatMessage.PlayerID}";
                    Data.Logger?.Invoke($"{PlayerName}: {packet.ChatMessage.Message}", null);
                    break;
                default:
                    break;
            }
        }

        public void processStagedData()
        {
            switch (Data.InstanceContainer.netConnection.OnlineMode)
            {
                case OnlineMode.Coop:
                    ProcessSharedLocations();
                    break;
                case OnlineMode.Multiworld:
                    ProcessMultiworldItems();
                    break;
            }
        }

        public void ParseMultiWorldData(NetPacket packet)
        {
            foreach (var PlayerData in packet.ItemData)
            {
                foreach (var ItemData in PlayerData.Value)
                {
                    ItemDataToProcess.SetIfEmpty(PlayerData.Key, new Dictionary<string, int>());
                    ItemDataToProcess[PlayerData.Key][ItemData.Key] = ItemData.Value;
                }
            }
            if (Data.AutoProcessData)
            {
                ProcessMultiworldItems();
            }
            Data.Refresh?.Invoke();
        }

        public void ProcessMultiworldItems()
        {
            foreach (var item in Data.InstanceContainer.Instance.ItemPool)
            {
                foreach (var Player in ItemDataToProcess.Keys)
                {
                    if (item.Value.AmountAquiredOnline.ContainsKey(Player)) { item.Value.AmountAquiredOnline.Remove(Player); }
                }
            }

            foreach (var players in ItemDataToProcess)
            {
                foreach (var items in players.Value)
                {
                    var ValidItem = Data.InstanceContainer.Instance.GetItemToPlace(items.Key, false, true);
                    if (ValidItem is null) { continue; }
                    ValidItem.AmountAquiredOnline.SetIfEmpty(players.Key, 0);
                    ValidItem.AmountAquiredOnline[players.Key]++;
                }
            }
            ItemDataToProcess.Clear();
            Data.Refresh?.Invoke();
            Data.RefreshMainForm?.Invoke();
        }

        public void ParseSharedLocationData(NetPacket packet)
        {
            foreach (var i in packet.LocationData.Keys)
            {
                LocationDataToProcess[i] = packet.LocationData[i];
            }
            if (Data.AutoProcessData)
            {
                ProcessSharedLocations();
            }
            Data.Refresh?.Invoke();
        }

        public void ProcessSharedLocations()
        {
            MiscData.CheckState CheckAction = Data.AllowLocationChecking ? MiscData.CheckState.Checked : MiscData.CheckState.Marked;

            List<LocationData.LocationObject> LocationList = new List<LocationData.LocationObject>();
            foreach (var i in LocationDataToProcess)
            {
                if (!Data.InstanceContainer.Instance.LocationPool.ContainsKey(i.Key)) { continue; }
                var Location = Data.InstanceContainer.Instance.LocationPool[i.Key];
                if (Location.CheckState == MiscData.CheckState.Checked || Location.CheckState == CheckAction) { continue; }
                LocationList.Add(Location);
            }

            var CheckObjectOptions = new CheckItemSetting(CheckAction).SetEnforceMarkAction(true).SetCheckUnassignedLocations((O, C) =>
            {
                List<ManualCheckObjectResult> Results = [];
                foreach (var obj in O)
                {
                    LocationData.LocationObject location = obj as LocationData.LocationObject;
                    Results.Add(new ManualCheckObjectResult().SetLocation(location, LocationDataToProcess[location.ID]));
                }
                return Results;
            });

            LocationChecker.SetLocationsCheckState(LocationList, Data.InstanceContainer, CheckObjectOptions);

            LocationDataToProcess.Clear();
            Data.Refresh?.Invoke();
            Data.RefreshMainForm?.Invoke();

        }

        public void TrackerDataHandeling_CheckedObjectsUpdate(List<object> ObjectsUpdated, TrackerInstance Instance)
        {
            var LocationsUpdated = ObjectsUpdated.Where(x => x is LocationData.LocationObject lo && (lo.Randomizeditem.OwningPlayer > -1) || Data.InstanceContainer.netConnection.OnlineMode != OnlineMode.Multiworld)
                .Select(x => (LocationData.LocationObject)x);
            if (Data.InstanceContainer.netConnection.IsConnected()) { return; }
            if (!Data.SendData) { return; }

            NetData.NetPacket? packet;
            switch (Data.InstanceContainer.netConnection.OnlineMode)
            {
                case OnlineMode.Multiworld:
                    packet = PacketCreation.CreateMultiWorldPacket(Data.PlayerID, Data.Password, Instance, LocationsUpdated);
                    break;
                case OnlineMode.Coop:
                    packet = PacketCreation.CreateCoopPacket(Data.PlayerID, Data.Password, Instance, LocationsUpdated);
                    break;
                default: return;
            }
            if (packet == null) { return; }

            byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(packet.ToFormattedJson());
            Data.InstanceContainer.netConnection.ServerConnection.GetStream().Write(bytesToSend, 0, bytesToSend.Length);
        }
    }
}
