using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MMR_Tracker_V3.TrackerObjects.MiscData;

namespace MMR_Tracker_V3.TrackerObjects
{
    public class MMRData
    {

        [Serializable]
        public class JsonFormatLogicItem
        {
            public string Id { get; set; }
            public List<string> RequiredItems { get; set; } = new List<string>();
            public List<List<string>> ConditionalItems { get; set; } = new List<List<string>>();
            public TimeOfDay TimeNeeded { get; set; }
            public TimeOfDay TimeAvailable { get; set; }
            public TimeOfDay TimeSetup { get; set; }
            public bool IsTrick { get; set; }

            private string _trickTooltip;
            private string _trickCategory;
            public string TrickTooltip
            {
                get { return IsTrick ? _trickTooltip : null; }
                set { _trickTooltip = value; }
            }
            public string TrickCategory
            {
                get { return IsTrick ? _trickCategory : null; }
                set { _trickCategory = value; }
            }
        }

        [Serializable]
        public class SpoilerLogData
        {
            public GameplaySettings GameplaySettings { get; set; }
            public Dictionary<string, string> LocationLog { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, string> GossipLog { get; set; } = new Dictionary<string, string>();
            public Dictionary<string, int> PriceLog { get; set; } = new Dictionary<string, int>();
        }

        [Serializable]
        public class GameplaySettings
        {
            public string StrayFairyMode { get; set; } = "Default";
            public bool DoubleArcheryRewards { get; set; } = false;
            public bool UpdateShopAppearance { get; set; } = true;
            public bool AddSongs { get; set; } = false;
            public string BossKeyMode { get; set; } = "Default";
            public string SmallKeyMode { get; set; } = "DoorsOpen";
            public bool SpeedupLabFish { get; set; } = true;
            public string BossRemainsMode { get; set; } = "Default";
            public string PriceMode { get; set; } = "None";
            public string UserLogicFileName { get; set; } = "";
            public bool CloseCows { get; set; } = true;
            public bool ArrowCycling { get; set; } = true;
            public bool CritWiggleDisable { get; set; } = false;
            public bool DrawHash { get; set; } = true;
            public bool ElegySpeedup { get; set; } = true;
            public bool FastPush { get; set; } = true;
            public bool IceTrapQuirks { get; set; } = false;
            public bool UpdateWorldModels { get; set; } = true;
            public bool OcarinaUnderwater { get; set; } = true;
            public bool QuestItemStorage { get; set; } = true;
            public bool ContinuousDekuHopping { get; set; } = false;
            public bool ProgressiveUpgrades { get; set; } = true;
            public bool TargetHealthBar { get; set; } = false;
            public bool ClimbMostSurfaces { get; set; } = false;
            public bool FreeScarecrow { get; set; } = false;
            public bool FillWallet { get; set; } = false;
            public string AutoInvert { get; set; } = "Never";
            public bool HiddenRupeesSparkle { get; set; } = false;
            public string LogicMode { get; set; } = "Casual";
            public string ItemPlacement { get; set; } = "Bespoke";
            public List<string> EnabledTricks { get; set; } = new List<string>();
            public bool RandomizeDungeonEntrances { get; set; } = false;
            public bool RandomizeEnemies { get; set; } = false;
            public string StartingItemMode { get; set; } = "None";
            public string CustomItemListString { get; set; } = "";
            public string CustomStartingItemListString { get; set; } = "";
            public string CustomJunkLocationsString { get; set; } = "";
            public string IceTraps { get; set; } = "None";
            public string IceTrapAppearance { get; set; } = "MajorItems";
            public string DamageMode { get; set; } = "Default";
            public string DamageEffect { get; set; } = "Default";
            public string MovementMode { get; set; } = "Default";
            public string FloorType { get; set; } = "Default";
            public string NutandStickDrops { get; set; } = "Default";
            public string ClockSpeed { get; set; } = "Default";
            public bool HideClock { get; set; } = false;
            public string BlastMaskCooldown { get; set; } = "Default";
            public bool EnableSunsSong { get; set; } = false;
            public bool AllowFierceDeityAnywhere { get; set; } = false;
            public bool ByoAmmo { get; set; } = false;
            public bool DeathMoonCrash { get; set; } = false;
            public bool HookshotAnySurface { get; set; } = false;
            public Dictionary<string,string> ShortenCutsceneSettings { get; set; } = new Dictionary<string, string> { { "General", "" }, { "BossIntros", "" } };
            public bool QuickTextEnabled { get; set; } = true;
            public string Character { get; set; } = "LinkMM";
            public string GossipHintStyle { get; set; } = "Competitive";
            public string GaroHintStyle { get; set; } = "Default";
            public bool MixGossipAndGaroHints { get; set; } = false;
            public bool FreeHints { get; set; } = true;
            public bool FreeGaroHints { get; set; } = false;
            public bool ClearHints { get; set; } = true;
            public bool ClearGaroHints { get; set; } = false;
            public bool HintsIndicateImportance { get; set; } = false;
            public bool PreventDowngrades { get; set; } = true;
            public bool UpdateChests { get; set; } = false;
            public bool FixEponaSword { get; set; } = true;
            public bool EnablePictoboxSubject { get; set; } = true;
            public bool LenientGoronSpikes { get; set; } = false;
            public bool SpeedupBeavers { get; set; } = true;
            public bool SpeedupDampe { get; set; } = true;
            public bool SpeedupDogRace { get; set; } = true;
            public bool SpeedupBank { get; set; } = true;
        }

        [Serializable]
        public class LogicFile
        {
            public int Version { get; set; } = -1;

            public string GameCode 
            { 
                get { return _GameCode ?? "MMR"; } 
                set { _GameCode = value == "MMR" ? null : value; }
            }
            private string _GameCode = null;

            public List<JsonFormatLogicItem> Logic { get; set; }

            public override string ToString()
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(this, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Serialize(this, _jsonSerializerOptions);
            }

            public static LogicFile FromJson(string json)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<LogicFile>(json, _NewtonsoftJsonSerializerOptions);
                //return JsonSerializer.Deserialize<LogicFile>(json, _jsonSerializerOptions);
            }

            private readonly static Newtonsoft.Json.JsonSerializerSettings _NewtonsoftJsonSerializerOptions = new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
            };

            public IEnumerable<string> GetAllItemsUsedInLogic()
            {
                var AllReq = Logic.Select(x => x.RequiredItems).SelectMany(x => x).Distinct();
                var allCond = Logic.Select(x => x.ConditionalItems.SelectMany(x => x).Distinct()).SelectMany(x => x).Distinct();
                return AllReq.Concat(allCond).Distinct();
            }
        }

        public class SpoilerlogReference
        {
            public string[] SpoilerLogNames { get; set; } = Array.Empty<string>();
            public string[] GossipHintNames { get; set; } = Array.Empty<string>();
            public string[] PriceDataNames { get; set; } = Array.Empty<string>();
        }
    }
}
