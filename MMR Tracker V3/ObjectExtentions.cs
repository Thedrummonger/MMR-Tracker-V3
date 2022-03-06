using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.LogicObjects;
using static MMR_Tracker_V3.TrackerObjects.ItemData;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3
{
    public static class ObjectExtentions
    {
        //ItemData
        public static ItemObject GetItemByID(this LogicObjects.TrackerInstance instance, string item)
        {
            var Item = instance.ItemPool.FirstOrDefault(x => item == x.Id);
            return Item;
        }

        public static LocationData.LocationObject GetLocationByID(this LogicObjects.TrackerInstance instance, string item)
        {
            var Item = instance.LocationPool.FirstOrDefault(x => item == x.ID);
            return Item;
        }

        public static MacroObject GetMacroByID(this TrackerInstance instance, string item)
        {
            var Item = instance.MacroPool.FirstOrDefault(x => item == x.ID);
            return Item;
        }

        public static OptionData.TrackerOption GetTrackerOptionByID(this LogicObjects.TrackerInstance instance, string item)
        {
            var Item = instance.TrackerOptions.FirstOrDefault(x => item == x.ID);
            return Item;
        }

        public static bool IsLiteralID(this string ID, out string CleanedID)
        {
            bool Literal = false;
            CleanedID = ID;
            if (ID.StartsWith("'") && ID.EndsWith("'"))
            {
                Literal = true;
                CleanedID = ID.Replace("'", "");
            }
            return Literal;
        }

        public static LogicEntryType GetLocationEntryType(this LogicObjects.TrackerInstance instance, string ID, bool literal = false)
        {
            if (literal && instance.InstanceReference.LocationDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.location; }
            if (literal && instance.InstanceReference.HintDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.Hint; }
            if (instance.GetMacroByID(ID) != null) { return LogicEntryType.macro; }
            if (instance.InstanceReference.LocationDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.location; }
            if (instance.InstanceReference.HintDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.Hint; }
            return LogicEntryType.error;
        }

        public static LogicEntryType GetItemEntryType(this LogicObjects.TrackerInstance instance, string OriginalID, bool literal = false)
        {
            LogicCalculation.MultipleItemEntry(OriginalID, out string ID, out _);
            if (literal && instance.InstanceReference.ItemDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.item; }
            if (instance.GetMacroByID(ID) != null) { return LogicEntryType.macro; }
            if (instance.InstanceReference.ItemDictionaryMapping.ContainsKey(ID)) { return LogicEntryType.item; }
            if (bool.TryParse(ID, out _)) { return LogicEntryType.Bool; }
            if (LogicCalculation.LogicOptionEntry(instance, ID, out _)) { return LogicEntryType.Option; }
            return LogicEntryType.error;
        }

        public static MMRData.JsonFormatLogicItem GetLogic(this LogicObjects.TrackerInstance instance, string OriginalID)
        {
            bool Literal = OriginalID.IsLiteralID(out string ID);
            LogicEntryType entryType = instance.GetLocationEntryType(ID, Literal);

            if (entryType == LogicEntryType.macro)
            {
                ID = instance.HandleDynamicLogic(ID);
                if (ID == null) 
                { 
                    return Utility.CreateInaccessableLogic(ID); 
                }
            }

            List<string> Requirements = new();
            List<List<string>> Conditionals = new();
            bool Istrick = false;
            if (instance.InstanceReference.LogicFileMapping.ContainsKey(ID))
            {
                int Index = instance.InstanceReference.LogicFileMapping[ID];
                Istrick = instance.LogicFile.Logic[Index].IsTrick;
                var LogicEntry = instance.LogicFile.Logic[Index];
                Utility.DeepCloneLogic(LogicEntry.RequiredItems, LogicEntry.ConditionalItems, out Requirements, out Conditionals);
            }
            if (instance.LogicOverride.ContainsKey(ID))
            {
                List<string> OverrideRequirements = Requirements;
                List<List<string>> OverrideConditionals = Conditionals;
                if (instance.LogicOverride[ID].RequiredItems != null) { OverrideRequirements = instance.LogicOverride[ID].RequiredItems; }
                if (instance.LogicOverride[ID].ConditionalItems != null) { OverrideConditionals = instance.LogicOverride[ID].ConditionalItems; }
                Utility.DeepCloneLogic(OverrideRequirements, OverrideConditionals, out Requirements, out Conditionals);
            }

            var ValidOptions = instance.TrackerOptions.Where(x => x.GetActions().LocationValid(ID));
            if (ValidOptions.Any())
            {
                HandleOptionLogicEdits(ValidOptions, ID, Requirements, Conditionals, out Requirements, out Conditionals);
            }

            var Logic = new MMRData.JsonFormatLogicItem()
            {
                Id = ID,
                RequiredItems = Requirements,
                ConditionalItems = Conditionals,
                IsTrick = Istrick
            };

            return Logic;
        }

        public static MMRData.JsonFormatLogicItem GetOriginalLogic(this LogicObjects.TrackerInstance instance, string ID, bool copy = false)
        {
            bool Literal = ID.IsLiteralID(out ID);

            List<string> Requirements = new();
            List<List<string>> Conditionals = new();
            if (instance.InstanceReference.LogicFileMapping.ContainsKey(ID))
            {
                int Index = instance.InstanceReference.LogicFileMapping[ID];
                var LogicEntry = instance.LogicFile.Logic[Index];
                if (!copy) { return LogicEntry; }
                Utility.DeepCloneLogic(LogicEntry.RequiredItems, LogicEntry.ConditionalItems, out Requirements, out Conditionals);
            }

            var Logic = new MMRData.JsonFormatLogicItem()
            {
                Id = ID,
                RequiredItems = Requirements,
                ConditionalItems = Conditionals,
                IsTrick = false
            };

            return Logic;
        }

        public static void HandleOptionLogicEdits(IEnumerable<OptionData.TrackerOption> Options, string ID, List<string> InRequirements, List<List<string>> InConditionals, out List<string> OutRequirements, out List<List<string>> OutConditionals)
        {
            List<string> Requirements = InRequirements;
            List<List<string>> Conditionals = InConditionals;
            foreach (var option in Options)
            {
                foreach (var replacements in option.GetActions().LogicReplacements)
                {
                    if (!replacements.LocationValid(ID)) { continue; }
                    foreach (var i in replacements.ReplacementList)
                    {
                        Requirements = Requirements
                            .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                        for (var p = 0; p < Conditionals.Count; p++)
                        {
                            Conditionals[p] = Conditionals[p]
                                .Select(x => x == i.Target ? x.Replace(i.Target, i.Replacement) : x.Replace(" ", " ")).ToList();
                        }
                    }
                }
                foreach (var additionalSet in option.GetActions().AdditionalLogic)
                {
                    if (!additionalSet.LocationValid(ID)) { continue; }
                    foreach (var i in additionalSet.AdditionalRequirements) { Requirements.Add(i); }
                    foreach (var i in additionalSet.AdditionalConditionals) { Conditionals.Add(i); }
                }
            }
            OutRequirements = Requirements;
            OutConditionals = Conditionals;
        }

        public static string HandleDynamicLogic(this LogicObjects.TrackerInstance instance, string ID)
        {
            var MacroObject = instance.GetMacroByID(ID);
            var MacroDictObject = MacroObject.GetDictEntry(instance);
            if (MacroDictObject.DynamicLogicData != null)
            {
                var LocationToCompareIndex = instance.InstanceReference.LocationDictionaryMapping[MacroDictObject.DynamicLogicData.LocationToCompare];
                var LocationToCompare = instance.LocationPool[LocationToCompareIndex];
                foreach (var arg in MacroDictObject.DynamicLogicData.Arguments)
                {
                    if (LocationToCompare.Randomizeditem.Item == arg.ItemAtLocation)
                    {
                        return arg.LogicToUse;
                    }
                }
                return null;
            }
            else
            {
                return ID;
            }
        }

        public static int GetAmountPlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            int AmountAquired = itemObject.GetTotalUsable();
            int AmountSetAtLocation = 0;
            foreach(var x in Instance.LocationPool.Where(x => x.CheckState != CheckState.Checked))
            {
                var SetItem = x.Randomizeditem.Item;
                var SpoilerItem = x.Randomizeditem.SpoilerLogGivenItem;
                if ((SetItem != null && SetItem == itemObject.Id) || (SpoilerItem != null && SpoilerItem == itemObject.Id)) { AmountSetAtLocation++; }
            }

            return AmountAquired + AmountSetAtLocation;
        }

        public static bool CanBePlaced(this ItemObject itemObject, LogicObjects.TrackerInstance Instance)
        {
            if (itemObject.GetDictEntry(Instance).MaxAmountInWorld < 0) { return true; }
            return itemObject.GetAmountPlaced(Instance) < itemObject.GetDictEntry(Instance).MaxAmountInWorld;
        }

        public static int GetTotalUsable(this ItemObject itemObject)
        {
            var AmountAquiredOnline = 0;
            foreach(var i in itemObject.AmountAquiredOnline)
            {
                AmountAquiredOnline += i.Value;
            }

            return itemObject.AmountAquiredLocally + AmountAquiredOnline + itemObject.AmountInStartingpool;
        }

        public static bool Useable(this ItemObject itemObject, int Amount = 1)
        {
            return itemObject.GetTotalUsable() >= Amount;
        }

        public static void ChangeLocalItemAmounts(this ItemObject itemObject, LocationData.LocationObject location, int Amount)
        {
            if (Amount != 0)
            {
                if (location.Randomizeditem.OwningPlayer != -1)
                {
                    if (!itemObject.AmountSentToPlayer.ContainsKey(location.Randomizeditem.OwningPlayer)) 
                    { 
                        itemObject.AmountSentToPlayer.Add(location.Randomizeditem.OwningPlayer, 0); 
                    }
                    itemObject.AmountSentToPlayer[location.Randomizeditem.OwningPlayer] += Amount;
                }
                else
                {
                    itemObject.AmountAquiredLocally += Amount;
                }
            }
        }

        public static void ToggleChecked(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            CheckState CurrentState = data.CheckState;
            if (CurrentState == NewState) 
            {
                return; 
            }
            else if (CurrentState == CheckState.Checked)
            {
                UncheckItem(data, NewState, Instance);
            }
            else if (NewState == CheckState.Checked)
            {
                if (!CheckItem(data, NewState, Instance)) { return; }
            }
            else
            {
                if (!ToggleMarked(data, NewState, Instance)) { return; }
            }
            data.CheckState = NewState;
        }

        public static bool UncheckItem(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            var ItemObjectToAltar = Instance.GetItemByID(data.Randomizeditem.Item);

            if (ItemObjectToAltar != null)
            {
                ItemObjectToAltar.ChangeLocalItemAmounts(data, -1);
            }
            if (NewState == CheckState.Unchecked)
            {
                data.Randomizeditem.Item = null;
            }


            return true;

        }

        public static bool CheckItem(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (string.IsNullOrWhiteSpace(data.Randomizeditem.Item)) { return false; }

            var ItemObjectToAltar = Instance.GetItemByID(data.Randomizeditem.Item);
            if (ItemObjectToAltar != null) { ItemObjectToAltar.ChangeLocalItemAmounts(data, 1); }

            return true;
        }

        public static bool ToggleMarked(this LocationData.LocationObject data, CheckState NewState, LogicObjects.TrackerInstance Instance)
        {
            if (NewState == CheckState.Marked && string.IsNullOrWhiteSpace(data.Randomizeditem.Item))
            {
                return false;
            }
            else if (NewState == CheckState.Unchecked)
            {
                data.Randomizeditem.Item = null;
            }
            return true;
        }

        public static string GetItemAtCheck(this LocationData.LocationObject data, LogicObjects.TrackerInstance Instance)
        {
            var ItemAtCheck = data.Randomizeditem.Item;
            if (data.Randomizeditem.SpoilerLogGivenItem != null)
            {
                ItemAtCheck = data.Randomizeditem.SpoilerLogGivenItem;
            }
            if ((data.IsUnrandomized()) && data.GetDictEntry(Instance).OriginalItem != null)
            {
                ItemAtCheck = data.GetDictEntry(Instance).OriginalItem;
            }
            return ItemAtCheck;
        }
    }
}
