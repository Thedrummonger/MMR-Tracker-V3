using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class CheckableLocation(InstanceData.TrackerInstance Parent)
    {
        private InstanceData.TrackerInstance _parent = Parent;
        public InstanceData.TrackerInstance GetParent() { return _parent; }
        public void SetParent(InstanceData.TrackerInstance parent) { _parent = parent; }

        public string ID { get; set; }
        public bool Available { get; set; } = false;
        public CheckState CheckState { get; set; } = CheckState.Unchecked;
        public bool Starred { get; set; }
        public bool Hidden { get; set; } = false;
        public int? Price { get; set; } = null;
        public char? Currency { get; set; } = null;
        public string DisplayName { get; set; }
        public RandomizedState RandomizedState { get; set; } = RandomizedState.Randomized;
        public InstanceData.ReferenceData referenceData { get; set; } = new InstanceData.ReferenceData();
        public void GetPrice(out int outPrice, out char outCurrency)
        {
            outPrice = Price ?? -1;
            outCurrency = Currency ?? '$';
            return;
        }
        public (int, char) GetPrice()
        {
            return (Price ?? -1, Currency ?? '$');
        }
        public bool hasPrice()
        {
            var (Price, Cur) = GetPrice();
            return Price >= 0;
        }
        public void SetPrice(int inPrice, char inCurrency = '\0')
        {
            if (inCurrency == '\0') { inCurrency = Currency ?? '$'; }
            Price = inPrice < 0 ? null : inPrice;
            Currency = inCurrency;
            return;
        }

        public string GetName()
        {
            string name = CheckForDictName(this);
            if (name is not null) { return name; }
            name = CheckForObjectName(this);
            if (name is not null) { return name; }
            return ID;
        }

        private static string CheckForDictName(dynamic Object)
        {
            if (!Utility.DynamicMethodExists(Object, "GetDictEntry")) { return null; }
            var DictEntry = Object.GetDictEntry();
            if (DictEntry is null) { return null; }
            if (Utility.DynamicMethodExists(DictEntry, "GetName")) { return DictEntry.GetName(); }
            if (Utility.DynamicPropertyExist(DictEntry, "Name")) { return DictEntry.Name; }
            return null;
        }

        private static string CheckForObjectName(dynamic Object)
        {
            if (Utility.DynamicPropertyExist(Object, "Name")) { return Object.Name; }
            return null;
        }
    }
}
