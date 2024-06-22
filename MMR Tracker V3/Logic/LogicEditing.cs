using MMR_Tracker_V3.TrackerObjects;
using System.Collections.Generic;
using System.Linq;

namespace MMR_Tracker_V3.Logic
{
    public static class LogicEditing
    {
        public static void HandlePriceLogic(InstanceData.TrackerInstance instance, int Price, char Currency, List<string> Requirements, List<List<string>> Conditionals, out List<string> NewRequirements, out List<List<string>> NewConditionals)
        {
            if (!instance.PriceData.GetCapacityMap(Currency).Any())
            {
                NewRequirements = Requirements;
                NewConditionals = Conditionals;
                return;
            }
            var ValidWallets = instance.PriceData.GetCapacityMap(Currency).Keys.Where(item => item >= Price);
            var MinValue = ValidWallets.Any() ? ValidWallets.Min() : instance.PriceData.GetCapacityMap(Currency).Keys.Max();
            var NewWallet = instance.PriceData.GetCapacityMap(Currency)[MinValue];

            var FlattenedLogic = Requirements.Concat(Conditionals.SelectMany(x => x));
            if (FlattenedLogic.Any(x => instance.PriceData.WalletEntries.Contains(x)))
            {
                foreach (var Wallet in instance.PriceData.WalletEntries)
                {
                    Requirements = Requirements.Select(x => x == Wallet ? NewWallet : x).ToList();
                    Conditionals = Conditionals.Select(set => set.Select(x => x == Wallet ? NewWallet : x).ToList()).ToList();
                }
            }
            else { Requirements.Add(NewWallet); }
            NewRequirements = Requirements;
            NewConditionals = Conditionals;
        }

        public static void HandleOptionLogicEdits(IEnumerable<OptionData.Action> Actions, string ID, List<string> InRequirements, List<List<string>> InConditionals, out List<string> OutRequirements, out List<List<string>> OutConditionals)
        {
            List<string> Requirements = InRequirements;
            List<List<string>> Conditionals = InConditionals;
            foreach (var action in Actions)
            {
                foreach (var replacements in action.LogicReplacements.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Select(x => replacements.ReplacementList.ContainsKey(x) && ID != replacements.ReplacementList[x] ? replacements.ReplacementList[x] : x).ToList();
                    Conditionals = Conditionals.Select(set => set.Select(x => replacements.ReplacementList.ContainsKey(x) && ID != replacements.ReplacementList[x] ? replacements.ReplacementList[x] : x).ToList()).ToList();
                }
                foreach (var additionalSet in action.AdditionalLogic.Where(x => x.LocationValid(ID)))
                {
                    Requirements = Requirements.Concat(additionalSet.AdditionalRequirements).ToList();
                    Conditionals = Conditionals.Concat(additionalSet.AdditionalConditionals).ToList();
                }
            }
            OutRequirements = Requirements;
            OutConditionals = Conditionals;
        }
    }
}
