﻿using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public abstract class CheckableLocation(InstanceData.TrackerInstance Parent)
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
        public virtual void GetPrice(out int outPrice, out char outCurrency)
        {
            outPrice = Price ?? -1;
            outCurrency = Currency ?? '$';
            return;
        }
        public virtual (int, char) GetPrice()
        {
            return (Price ?? -1, Currency ?? '$');
        }
        public virtual bool hasPrice()
        {
            var (Price, Cur) = GetPrice();
            return Price >= 0;
        }
        public virtual void SetPrice(int inPrice, char inCurrency = '\0')
        {
            if (inCurrency == '\0') { inCurrency = Currency ?? '$'; }
            Price = inPrice < 0 ? null : inPrice;
            Currency = inCurrency;
            return;
        }

        public abstract string GetName();
        public abstract LogicDictionaryData.DictionaryCheckableLocationEntry GetAbstractDictEntry();
        public abstract MiscData.CheckableLocationTypes LocationType();
    }
}
