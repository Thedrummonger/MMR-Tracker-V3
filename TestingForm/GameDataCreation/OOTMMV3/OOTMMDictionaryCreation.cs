﻿using MMR_Tracker_V3.TrackerObjects;
using TDMUtils;
using static TestingForm.GameDataCreation.OOTMMV3.OOTMMDataClasses;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    public class OOTMMDictionaryCreation
    {
        public OOTMMDataGenerator generator;
        public List<OOTMMPoolLocation> OOTPool;
        public List<OOTMMPoolLocation> MMPool;
        public List<OOTMMPoolLocation> OOTHints;
        public List<OOTMMPoolLocation> MMHints;
        public Dictionary<string, OOTMMEntranceFileData> Entrances;
        public OOTMMDictionaryCreation(OOTMMDataGenerator Maingenerator)
        {
            generator = Maingenerator; 
            OOTPool = OOTMMUtility.DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.OOTPoolFile);
            MMPool = OOTMMUtility.DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.MMPoolFile);
            Entrances = OOTMMUtility.DeserializeYAMLFile<Dictionary<string, OOTMMEntranceFileData>>(OOTMMPaths.EntranceFile);
            OOTHints = OOTMMUtility.DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.OOTHintsFile);
            MMHints = OOTMMUtility.DeserializeCSVFile<List<OOTMMPoolLocation>>(OOTMMPaths.MMHintsFile);
        }
        public void CreateDictLocations()
        {
            foreach (var location in OOTPool) { AddPoolLocation(location, "OOT"); }
            foreach (var location in MMPool) { AddPoolLocation(location, "MM"); }
        }
        public void CreateDictHints()
        {
            foreach (var hint in OOTHints) { AddPoolHint(hint, "OOT"); }
            foreach (var hint in MMHints) { AddPoolHint(hint, "MM"); }
        }
        public void CreateDictItems()
        {
            foreach (var item in generator.extraData.items.Distinct())
            {
                LogicDictionaryData.DictionaryItemEntries ItemEntry = new LogicDictionaryData.DictionaryItemEntries()
                {
                    ID = item,
                    ItemTypes = ["item"],
                    Name = generator.extraData.names[item],
                    ValidStartingItem = true,
                    MaxAmountInWorld = -1,
                    SpoilerData = new MMRData.SpoilerlogReference() { SpoilerLogNames = [item, generator.extraData.names[item]] }
                };
                generator.dictionary.ItemList.Add(ItemEntry.ID, ItemEntry);
            }
        }
        public void CreateDictEntrances()
        {
            foreach (var logiEntry in generator.LogicFile.Logic.Select(x => x.Id))
            {
                if (!logiEntry.Contains("=>")) { continue; }
                var Data = logiEntry.TrimSplit("=>");
                string Area = Data[0];
                string Destination = Data[1];
                var EntData = Entrances.FirstOrDefault(x => x.Value.areas is not null && x.Value.areas[0] == Area && x.Value.areas[1] == Destination);
                bool IsRandomizable = !EntData.Equals(default(KeyValuePair<string, OOTMMEntranceFileData>));
                LogicDictionaryData.DictionaryEntranceEntries EntranceEntry = new LogicDictionaryData.DictionaryEntranceEntries()
                {
                    Area = Area,
                    Exit = Destination,
                    EntrancePairID = IsRandomizable && !string.IsNullOrWhiteSpace(EntData.Value.reverse) ? new EntranceData.EntranceAreaPair
                    {
                        Area = Entrances[EntData.Value.reverse].areas[0],
                        Exit = Entrances[EntData.Value.reverse].areas[1]
                    } : null,
                    ID = logiEntry,
                    RandomizableEntrance = IsRandomizable,
                    SpoilerData = new MMRData.SpoilerlogReference
                    {
                        SpoilerLogNames = IsRandomizable ? [EntData.Key] : [],
                        Tags = IsRandomizable ? [EntData.Value.type] : [],
                    }
                };
                generator.dictionary.EntranceList.Add(EntranceEntry.ID, EntranceEntry);
            }
        }
        private void AddPoolHint(OOTMMPoolLocation location, string GameCode)
        {
            string ID = OOTMMUtility.AddGameCodeToLogicID(location.location, GameCode, false);
            LogicDictionaryData.DictionaryHintEntries locationEntries = new LogicDictionaryData.DictionaryHintEntries()
            {
                ID = ID,
                Name = ID,
                SpoilerData = new MMRData.SpoilerlogReference()
                {
                    SpoilerLogNames = [ID],
                    Tags = [location.type]
                }
            };
            generator.dictionary.HintSpots.Add(locationEntries.ID, locationEntries);
        }
        private void AddPoolLocation(OOTMMPoolLocation location, string GameCode)
        {
            string ID = OOTMMUtility.AddGameCodeToLogicID(location.location, GameCode, false);
            LogicDictionaryData.DictionaryLocationEntries locationEntries = new LogicDictionaryData.DictionaryLocationEntries()
            {
                ID = ID,
                Name = ID,
                Area = location.scene,
                OriginalItem = OOTMMUtility.AddGameCodeToLogicID(location.item, GameCode),
                Repeatable = OOTMMUtility.IsLocationRenewable(location, GameCode, generator.extraData),
                ValidItemTypes = ["item"],
                SpoilerData = new MMRData.SpoilerlogReference()
                {
                    SpoilerLogNames = [ID],
                    Tags = [location.scene, location.type],
                    GossipHintNames = [location.hint]
                }
            };
            if (locationEntries.OriginalItem.In("OOT_FLEXIBLE", "OOT_RANDOM", "MM_???", "MM_RANDOM")) { 
                locationEntries.OriginalItem = OOTMMUtility.AddGameCodeToLogicID("RUPEE_GREEN", GameCode); }
            if (locationEntries.OriginalItem.In("MM_ARROWS_20")) { 
                locationEntries.OriginalItem = OOTMMUtility.AddGameCodeToLogicID("ARROWS_30", GameCode); }
            if (locationEntries.OriginalItem.In("OOT_NOTHING", "MM_NOTHING")) { 
                locationEntries.OriginalItem = "NOTHING"; }
            generator.dictionary.LocationList.Add(locationEntries.ID, locationEntries);
        }
    }
}
