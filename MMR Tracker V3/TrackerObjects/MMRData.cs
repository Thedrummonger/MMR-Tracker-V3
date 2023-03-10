using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public enum StrayFairyMode
        {
            Default,
            ChestsOnly,
            KeepWithinDungeon,
        }
        public enum BossKeyMode
        {
            Default,
            DoorsOpen = 1,
            KeepWithinDungeon = 1 << 1,
            KeepThroughTime = 1 << 2,
        }
        public enum SmallKeyMode
        {
            Default,
            DoorsOpen = 1,
            KeepWithinDungeon = 1 << 1,
            KeepThroughTime = 1 << 2,
        }
        public enum BossRemainsMode
        {
            Default,
            GreatFairyRewards,
            KeepWithinDungeon,
        }
        public enum PriceMode
        {
            None,
            Purchases = 1,
            Minigames = 2,
            Misc = 4,
            AccountForRoyalWallet = 8,
        }
        public enum AutoInvertState : byte
        {
            Never,
            FirstCycle,
            Always,
        }
        public enum LogicMode
        {
            Casual,
            Glitched,
            Vanilla,
            UserLogic,
            NoLogic,
        }
        public enum ItemPlacement
        {
            Random,
            Bespoke,
            Classic
        }
        public enum StartingItemMode
        {
            None,
            Random,
            AllowTemporaryItems,
        }
        public enum IceTraps
        {
            None,
            Normal,
            Extra,
            Mayhem,
            Onslaught,
        }
        public enum IceTrapAppearance
        {
            MajorItems,
            JunkItems,
            Anything,
        }
        public enum DamageMode
        {
            Default,
            Double,
            Quadruple,
            OHKO,
            Doom
        }
        public enum DamageEffect
        {
            Default,
            Fire,
            Ice,
            Shock,
            Knockdown,
            Random
        }
        public enum MovementMode
        {
            Default,
            HighSpeed,
            SuperLowGravity,
            LowGravity,
            HighGravity
        }
        public enum FloorType
        {
            Default,
            Sand,
            Ice,
            Snow,
            Random
        }
        public enum NutAndStickDrops
        {
            Default = 0,
            Light = 1,
            Medium = 2,
            Extra = 3,
            Mayhem = 4
        }
        public enum ClockSpeed
        {
            Default,
            VerySlow,
            Slow,
            Fast,
            VeryFast,
            SuperFast
        }
        public enum BlastMaskCooldown
        {
            Default,
            Instant,
            VeryShort,
            Short,
            Long,
            VeryLong
        }
        public enum Character
        {
            LinkMM,
            LinkOOT,
            AdultLink,
            Kafei
        }
        public enum GossipHintStyle
        {
            Default,
            Random,
            Relevant,
            Competitive,
        }

        [Serializable]
        public class GameplaySettings
        {
            public StrayFairyMode StrayFairyMode { get; set; } = StrayFairyMode.Default;
            public bool DoubleArcheryRewards { get; set; } = false;
            public bool UpdateShopAppearance { get; set; } = true;
            public bool AddSongs { get; set; } = false;
            public BossKeyMode BossKeyMode { get; set; } = BossKeyMode.Default;
            public SmallKeyMode SmallKeyMode { get; set; } = SmallKeyMode.DoorsOpen;
            public bool SpeedupLabFish { get; set; } = true;
            public BossRemainsMode BossRemainsMode { get; set; } = BossRemainsMode.Default;
            public PriceMode PriceMode { get; set; } = PriceMode.None;
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
            public AutoInvertState AutoInvert { get; set; } = AutoInvertState.Never;
            public bool HiddenRupeesSparkle { get; set; } = false;
            public LogicMode LogicMode { get; set; } = LogicMode.Casual;
            public ItemPlacement ItemPlacement { get; set; } = ItemPlacement.Bespoke;
            public List<string> EnabledTricks { get; set; } = new List<string>();
            public bool RandomizeDungeonEntrances { get; set; } = false;
            public bool RandomizeEnemies { get; set; } = false;
            public StartingItemMode StartingItemMode { get; set; } = StartingItemMode.None;
            public string CustomItemListString { get; set; } = "";
            public string CustomStartingItemListString { get; set; } = "";
            public string CustomJunkLocationsString { get; set; } = "";
            public IceTraps IceTraps { get; set; } = IceTraps.None;
            public IceTrapAppearance IceTrapAppearance { get; set; } = IceTrapAppearance.MajorItems;
            public DamageMode DamageMode { get; set; } = DamageMode.Default;
            public DamageEffect DamageEffect { get; set; } = DamageEffect.Default;
            public MovementMode MovementMode { get; set; } = MovementMode.Default;
            public FloorType FloorType { get; set; } = FloorType.Default;
            public NutAndStickDrops NutandStickDrops { get; set; } = NutAndStickDrops.Default;
            public ClockSpeed ClockSpeed { get; set; } = ClockSpeed.Default;
            public bool HideClock { get; set; } = false;
            public BlastMaskCooldown BlastMaskCooldown { get; set; } = BlastMaskCooldown.Default;
            public bool EnableSunsSong { get; set; } = false;
            public bool AllowFierceDeityAnywhere { get; set; } = false;
            public bool ByoAmmo { get; set; } = false;
            public bool DeathMoonCrash { get; set; } = false;
            public bool HookshotAnySurface { get; set; } = false;
            public Dictionary<string,string> ShortenCutsceneSettings { get; set; } = new Dictionary<string, string> { { "General", "" }, { "BossIntros", "" } };
            public bool QuickTextEnabled { get; set; } = true;
            public Character Character { get; set; } = Character.LinkMM;
            public GossipHintStyle GossipHintStyle { get; set; } = GossipHintStyle.Competitive;
            public GossipHintStyle GaroHintStyle { get; set; } = GossipHintStyle.Default;
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
