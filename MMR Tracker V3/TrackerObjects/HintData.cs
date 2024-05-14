using Octokit;
using System;
using System.Collections.Generic;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class HintData
    {

        [Serializable]
        public class HintObject(InstanceData.TrackerInstance Parent) : CheckableLocation(Parent)
        {
            private InstanceData.TrackerInstance _parent = Parent;
            public new RandomizedState RandomizedState
            {
                get { return _RandomizedState; }
                set { _RandomizedState = value == RandomizedState.Randomized ? RandomizedState.Randomized : RandomizedState.ForcedJunk; }
            }
            private RandomizedState _RandomizedState = RandomizedState.Randomized;
            public string HintText { get; set; }
            public Dictionary<string, string> ParsedHintData { get; set; } = [];
            public List<string> FoolishLocations { get; set; } = [];
            public string SpoilerHintText { get; set; }
            public override string ToString()
            {
                return DisplayName ?? ID;
            }
            public LogicDictionaryData.DictionaryHintEntries GetDictEntry()
            {
                return GetParent().LogicDictionary.HintSpots[ID];
            }

            public override string GetName()
            {
                return GetDictEntry().Name ?? ID;
            }

            public override LogicDictionaryData.DictionaryCheckableLocationEntry GetAbstractDictEntry() => GetDictEntry();

            public override CheckableLocationTypes LocationType() => CheckableLocationTypes.Hint;
        }
        public class RemoteLocationHint(ItemData.ItemObject _ItemObject, string RemoteLocation, int _PlayerID)
        {
            public ItemData.ItemObject Item = _ItemObject;
            public string Location = RemoteLocation;
            public int RemotePlayerID = _PlayerID;
            string PlayerNumber(int Player)
            {
                if (Item.GetParent().GetParentContainer().netConnection is null) { return Player.ToString(); }
                if (Item.GetParent().GetParentContainer().netConnection.PlayerNames.TryGetValue(Player, out string name))
                {
                    return $"{Player} ({name})";
                }
                return Player.ToString();
            }
            public override string ToString()
            {
                return $"{Item.GetDictEntry().GetName()}: {Location} [Player: {PlayerNumber(RemotePlayerID)}] ";
            }
        }
    }
}
