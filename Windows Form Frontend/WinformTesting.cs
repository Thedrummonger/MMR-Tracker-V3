using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Form_Frontend
{
    internal class WinformTesting
    {
        public static void CreateMMRItemTrackerObject()
        {
            WinFormImageUtils.ItemTrackerInstance instance = new();
            instance.LogicVersion = 12;
            instance.GameCode = "MMR";
            instance.imageSheet = new();
            instance.imageSheet.ImageSheetPath = Path.Combine("ItemTrackerData", "MMR.png");
            instance.imageSheet.ImageDimentions = 32;
            instance.ImagesPerLimiterDirection = 8;
            instance.LimiterDirection = WinFormImageUtils.StaticDirecton.Horizontal;

            //ROW 1=============================================================================================================

            string ID;
            int iconx;
            int icony;

            //Ocarina
            instance.AddDisplayBox("DBOcarina", 0, 2);
            instance.AddTextToDefaultImage("DBOcarina", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemOcarina");
            instance.AddDisplayItem("DBOcarina", "ItemOcarina", 0, 2, "ItemOcarina");

            //Bow
            instance.AddDisplayBox("DBbows", 1, 2);
            instance.AddTextToDefaultImage("DBbows", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "UpgradeBiggestQuiver+UpgradeBigQuiver+ItemBow");
            instance.AddDisplayItem("DBbows", "UpgradeBiggestQuiver", 1, 2, "UpgradeBiggestQuiver");
            instance.AddDisplayItem("DBbows", "UpgradeBigQuiver", 1, 2, "UpgradeBigQuiver");
            instance.AddDisplayItem("DBbows", "ItemBow", 1, 2, "ItemBow");
            instance.AddTextToDisplayItem("DBbows", "UpgradeBiggestQuiver", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "50");
            instance.AddTextToDisplayItem("DBbows", "UpgradeBigQuiver", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "40");
            instance.AddTextToDisplayItem("DBbows", "ItemBow", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "30");

            //FireArrow
            instance.AddDisplayBox("DBFireArrow", 2, 2);
            instance.AddTextToDefaultImage("DBFireArrow", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemFireArrow");
            instance.AddDisplayItem("DBFireArrow", "ItemFireArrow", 2, 2, "ItemFireArrow");

            //IceArrow
            instance.AddDisplayBox("DBIceArrow", 3, 2);
            instance.AddTextToDefaultImage("DBIceArrow", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemIceArrow");
            instance.AddDisplayItem("DBIceArrow", "ItemIceArrow", 3, 2, "ItemIceArrow");

            //LightArrow
            instance.AddDisplayBox("DBLightArrow", 4, 2);
            instance.AddTextToDefaultImage("DBLightArrow", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemLightArrow");
            instance.AddDisplayItem("DBLightArrow", "ItemLightArrow", 4, 2, "ItemLightArrow");

            //Song of Time
            instance.AddDisplayBox("DBSongTime", 3, 24);
            instance.AddTextToDefaultImage("DBSongTime", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongTime");
            instance.AddDisplayItem("DBSongTime", "SongTime", 3, 24, "SongTime", false, -200, -20, 100);
            instance.AddStaticTextBox("DBSongTime", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "SoT");

            //Sonata
            instance.AddDisplayBox("DBSongSonata", 3, 24);
            instance.AddTextToDefaultImage("DBSongSonata", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongSonata");
            instance.AddDisplayItem("DBSongSonata", "SongSonata", 3, 24, "SongSonata", false, -50, 32, -50);
            instance.AddStaticTextBox("DBSongSonata", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Sonata");

            //MoonTear
            instance.AddDisplayBox("DBMoonTear", 0, 6);
            instance.AddTextToDefaultImage("DBMoonTear", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemMoonTear");
            instance.AddDisplayItem("DBMoonTear", "TradeItemMoonTear", 0, 6, "TradeItemMoonTear");

            //ROW 2=============================================================================================================

            //Bombs
            instance.AddDisplayBox("DBbombs", 0, 3);
            instance.AddTextToDefaultImage("DBbombs", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "UpgradeBiggestBombBag+UpgradeBigBombBag+ItemBombBag");
            instance.AddDisplayItem("DBbombs", "UpgradeBiggestBombBag", 0, 3, "UpgradeBiggestBombBag");
            instance.AddDisplayItem("DBbombs", "UpgradeBigBombBag", 0, 3, "UpgradeBigBombBag");
            instance.AddDisplayItem("DBbombs", "ItemBombBag", 0, 3, "ItemBombBag");
            instance.AddTextToDisplayItem("DBbombs", "UpgradeBiggestBombBag", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "40");
            instance.AddTextToDisplayItem("DBbombs", "UpgradeBigBombBag", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "30");
            instance.AddTextToDisplayItem("DBbombs", "ItemBombBag", WinFormImageUtils.TextPosition.topLeft, WinFormImageUtils.TextType.text, "20");

            //Bombchu ???
            instance.AddDisplayBox("DBBombchu", 1, 3);
            instance.AddTextToDefaultImage("DBBombchu", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ShopItemBombsBombchu10+ChestInvertedStoneTowerBombchu10+ChestLinkTrialBombchu10");
            instance.AddDisplayItem("DBBombchu", "BombchuPack", 1, 3, "Any Bombchu Pack");
            instance.AddStaticTextBox("DBBombchu", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "10 Pack");

            //DekuSticks ???
            instance.AddDisplayBox("DBSticks", 2, 3);

            //DekuNuts ???
            instance.AddDisplayBox("DBNuts", 3, 3);

            //MagicBean
            instance.AddDisplayBox("DBMagicBean", 4, 3);
            instance.AddTextToDefaultImage("DBMagicBean", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemMagicBean+ChestInvertedStoneTowerBean+ShopItemBusinessScrubMagicBean");
            instance.AddDisplayItem("DBMagicBean", "ItemMagicBean", 4, 3, "OtherMagicBean");

            //Song of Healing
            instance.AddDisplayBox("DBHealing", 3, 24);
            instance.AddTextToDefaultImage("DBHealing", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongHealing");
            instance.AddDisplayItem("DBHealing", "SongHealing", 3, 24, "SongHealing", false, 100, 0, 50);
            instance.AddStaticTextBox("DBHealing", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Healing");

            //Lullaby
            instance.AddDisplayBox("DBLullaby", 3, 24);
            instance.AddTextToDefaultImage("DBLullaby", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongLullaby");
            instance.AddDisplayItem("DBLullaby", "SongLullaby", 3, 24, "SongLullaby", false, 64, -64, -32);
            instance.AddStaticTextBox("DBLullaby", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Lullaby");

            //Land Title Deed
            instance.AddDisplayBox("DBlandDeed", 1, 6);
            instance.AddTextToDefaultImage("DBlandDeed", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemLandDeed");
            instance.AddDisplayItem("DBlandDeed", "TradeItemLandDeed", 1, 6, "TradeItemLandDeed");

            //ROW 3=============================================================================================================

            //PowderKeg 0 4
            iconx = 0;
            icony = 4;
            ID = "Powderkeg";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "Powder Keg");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Powder Keg");

            //PictoBox
            iconx = 1;
            icony = 4;
            ID = "ItemPictobox";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemPictobox");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemPictobox");

            //Lens
            iconx = 2;
            icony = 4;
            ID = "ItemLens";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemLens");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemLens");

            //Hookshot
            iconx = 3;
            icony = 4;
            ID = "ItemHookshot";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemHookshot");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemHookshot");

            //Fairy Sword
            iconx = 4;
            icony = 4;
            ID = "ItemFairySword";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemFairySword");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemFairySword");

            //Song Epona
            iconx = 3;
            icony = 24;
            ID = "SongEpona";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongEpona");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "SongEpona", false, 100,20,-60);
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Epona");

            //BossaNova
            iconx = 3;
            icony = 24;
            ID = "SongNewWaveBossaNova";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongNewWaveBossaNova");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "SongNewWaveBossaNova", false, -200, 50, 100);
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "NWBN");

            //Swamp Deed
            iconx = 2;
            icony = 6;
            ID = "TradeItemSwampDeed";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemSwampDeed");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemSwampDeed");

            //ROW 4=============================================================================================================

            //Bottle
            iconx = 0;
            icony = 9;
            ID = "Bottle";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "Bottle with Red Potion+Bottle with Milk+Bottle with Gold Dust+Empty Bottle+Bottle with Chateau Romani");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Any Bottle");
            instance.AddTextToDisplayItem($"DB{ID}", ID, WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Bottle with Red Potion+Bottle with Milk+Bottle with Gold Dust+Empty Bottle+Bottle with Chateau Romani");

            //Milk
            iconx = 0;
            icony = 10;
            ID = "BottleMilk";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "Milk+Bottle with Milk");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Any Milk");

            //GoldDust
            iconx = 0;
            icony = 11;
            ID = "GoldDust";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemBottleGoronRace");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemBottleGoronRace");

            //SeaHourse
            iconx = 0;
            icony = 12;
            ID = "MundaneItemSeahorse";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MundaneItemSeahorse");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MundaneItemSeahorse");

            //Chateu
            iconx = 1;
            icony = 12;
            ID = "Chateau";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemBottleMadameAroma+ShopItemMilkBarChateau");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MMRTItemDisplayHasChateau");

            //Soaring
            instance.AddDisplayBox("DBSoaring", 3, 24);
            instance.AddTextToDefaultImage("DBSoaring", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongSoaring");
            instance.AddDisplayItem("DBSoaring", "SongSoaring", 3, 24, "SongSoaring", false, 32, 0, 0);
            instance.AddStaticTextBox("DBSoaring", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Soaring");

            //Elegey
            instance.AddDisplayBox("DBSongElegy", 3, 24);
            instance.AddTextToDefaultImage("DBSongElegy", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongElegy");
            instance.AddDisplayItem("DBSongElegy", "SongElegy", 3, 24, "SongElegy", false, 100, 80, 0);
            instance.AddStaticTextBox("DBSongElegy", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Elegy");

            //Mountain Deed
            iconx = 3;
            icony = 6;
            ID = "TradeItemMountainDeed";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemMountainDeed");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemMountainDeed");

            //ROW 5=============================================================================================================

            //Bottle Fairy
            iconx = 1;
            icony = 12;
            ID = "Fairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchFairy");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchFairy");

            //Bottle Deku
            iconx = 5;
            icony = 9;
            ID = "DekuPrincess";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchPrincess");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchPrincess");

            //Bottle Fish
            iconx = 2;
            icony = 10;
            ID = "Fish";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchFish");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchFish");

            //Bottle Bug
            iconx = 3;
            icony = 10;
            ID = "Bug";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchBug");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchBug");

            //Bottle Small poe
            iconx = 5;
            icony = 10;
            ID = "SmallPoe";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchPoe");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchPoe");

            //Song Storms
            instance.AddDisplayBox("DBStorms", 3, 24);
            instance.AddTextToDefaultImage("DBStorms", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongStorms");
            instance.AddDisplayItem("DBStorms", "SongSoaring", 3, 24, "SongStorms", false, 64, 64, 128);
            instance.AddStaticTextBox("DBStorms", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Storms");

            //Song Oath
            instance.AddDisplayBox("DBOath", 3, 24);
            instance.AddTextToDefaultImage("DBOath", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongOath");
            instance.AddDisplayItem("DBOath", "SongOath", 3, 24, "SongOath", false, 20, -50, 100);
            instance.AddStaticTextBox("DBOath", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Oath");

            //OceanDeed
            iconx = 4;
            icony = 6;
            ID = "TradeItemOceanDeed";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemOceanDeed");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemOceanDeed");

            //ROW 6=============================================================================================================

            //Bottle Big Poe
            iconx = 0;
            icony = 11;
            ID = "BottleCatchBigPoe";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchBigPoe");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchBigPoe");

            //Bottle Water
            iconx = 1;
            icony = 11;
            ID = "BottleCatchSpringWater";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchSpringWater");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchSpringWater");

            //Bottle HS
            iconx = 2;
            icony = 11;
            ID = "BottleCatchHotSpringWater";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchHotSpringWater");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchHotSpringWater");

            //Bottle Zora
            iconx = 3;
            icony = 11;
            ID = "BottleCatchEgg";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchEgg");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchEgg");

            //Bottle Mushroom
            iconx = 5;
            icony = 11;
            ID = "BottleCatchMushroom";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "BottleCatchMushroom");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "BottleCatchMushroom");

            //Heart Piece
            instance.AddDisplayBox("DBHeartPieces", 4, 7);
            instance.AddDisplayItem("DBHeartPieces", "HeartPiece", 4, 7, "Piece of Heart");
            instance.AddTextToDisplayItem("DBHeartPieces", "HeartPiece", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Piece of Heart");

            //Heart containers
            instance.AddDisplayBox("DBHeartContainer", 5, 7);
            instance.AddDisplayItem("DBHeartContainer", "HeartContainer", 5, 7, "Heart Container");
            instance.AddTextToDisplayItem("DBHeartContainer", "HeartContainer", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Heart Container");

            //Room Key
            iconx = 5;
            icony = 6;
            ID = "TradeItemRoomKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemRoomKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemRoomKey");

            //ROW 7=============================================================================================================

            //Postman

            //Allnight

            //Blast

            //Stone

            //GreatFairy

            //Deku

            //Redpotion

            //Mama

            //ROW 8=============================================================================================================

            //Keaton

            //Bremon

            //Bunny

            //DonGero

            //Scents

            //Goron

            //Greenpotion???

            //Kafei Letter

            //ROW 9=============================================================================================================

            //romani

            //circus

            //kafei

            //couples

            //masktruth

            //zora

            //bluepotion

            //pendant

            //ROW 10=============================================================================================================

            //Kamaro

            //Gibdo

            //Garo

            //Captians

            //Giants

            //Deity

            //ClockFairy

            //Notebook ???

            //ROW 10=============================================================================================================

            //Swords

            //Odolawa

            //WoodfallMap

            //WoodfallCompass

            //WoodfallBK

            //Woodfall Fairy

            //Swamp Skull

            //ROW 11=============================================================================================================

            //Sheild

            //Goht

            //Snowhead Map

            //Snowhead Compass

            //Snowhead Key

            //Snopwhead BK

            //Snowhead Fairy

            //Magic


            //ROW 12=============================================================================================================

            //Wallet

            //Gyorg

            //Greatbay Map

            //Greatbay Compass

            //Greatbay Key

            //Greatbay BK

            //Greatbay Fairy

            //Ocean Skull

            //ROW 13=============================================================================================================

            //GO MODE

            //Twinmold

            //StoneTower Map

            //StoneTower Compass

            //StoneTower Key

            //StoneTower BK

            //StoneTower Fairy

            //Double Defence


            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevCodePath(), "Windows Form Frontend", "ItemTrackerData", "MMRItemTracker.json"), Newtonsoft.Json.JsonConvert.SerializeObject(instance, MMR_Tracker_V3.Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine("ItemTrackerData", "MMRItemTracker.json"), Newtonsoft.Json.JsonConvert.SerializeObject(instance, MMR_Tracker_V3.Testing._NewtonsoftJsonSerializerOptions));

        }
    }
}
