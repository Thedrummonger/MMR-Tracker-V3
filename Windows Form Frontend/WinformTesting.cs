using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Windows_Form_Frontend
{
    internal class WinformTesting
    {
        public static void DoTests()
        {
            //WinformTesting.TPRCreateData();
            OOTMMCreateData();
            //WinformTesting.WWRCreateData();
            //TestFuncParse();
            //PMRCreateData();
            //MMR_Tracker_V3.OtherGames.TPRV2.ReadAndParse.ReadLines();
            //TPRCreateData();
        }

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
            instance.AddDisplayItem("DBbows", "UpgradeBiggestQuiver", 1, 2, "ProgressiveItems, disabled && UpgradeBiggestQuiver || ProgressiveItems, enabled && MMRTProgressiveQuiverX3");
            instance.AddDisplayItem("DBbows", "UpgradeBigQuiver", 1, 2, "ProgressiveItems, disabled && UpgradeBigQuiver || ProgressiveItems, enabled && MMRTProgressiveQuiverX2");
            instance.AddDisplayItem("DBbows", "ItemBow", 1, 2, "ProgressiveItems, disabled && ItemBow || ProgressiveItems, enabled && MMRTProgressiveQuiver");
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
            instance.AddDisplayItem("DBbombs", "UpgradeBiggestBombBag", 0, 3, "ProgressiveItems, disabled && UpgradeBiggestBombBag || ProgressiveItems, enabled && MMRTProgressiveBombBagX3");
            instance.AddDisplayItem("DBbombs", "UpgradeBigBombBag", 0, 3, "ProgressiveItems, disabled && UpgradeBigBombBag || ProgressiveItems, enabled && MMRTProgressiveBombBagX2");
            instance.AddDisplayItem("DBbombs", "ItemBombBag", 0, 3, "ProgressiveItems, disabled && ItemBombBag || ProgressiveItems, enabled && MMRTProgressiveBombBag");
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
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemBottleMadameAroma||ShopItemMilkBarChateau");

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
            instance.AddStaticTextBox("DBHeartPieces", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Piece of Heart");

            //Heart containers
            instance.AddDisplayBox("DBHeartContainer", 5, 7);
            instance.AddDisplayItem("DBHeartContainer", "HeartContainer", 5, 7, "Heart Container");
            instance.AddStaticTextBox("DBHeartContainer", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Heart Container");

            //Room Key
            iconx = 5;
            icony = 6;
            ID = "TradeItemRoomKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemRoomKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemRoomKey");

            //ROW 7=============================================================================================================

            //Postman
            iconx = 0;
            icony = 14;
            ID = "MaskPostmanHat";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskPostmanHat");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskPostmanHat");

            //Allnight
            iconx = 1;
            icony = 14;
            ID = "MaskAllNight";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskAllNight");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskAllNight");

            //Blast
            iconx = 2;
            icony = 14;
            ID = "MaskBlast";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskBlast");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskBlast");

            //Stone
            iconx = 3;
            icony = 14;
            ID = "MaskStone";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskStone");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskStone");

            //GreatFairy
            iconx = 4;
            icony = 14;
            ID = "MaskGreatFairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskGreatFairy");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskGreatFairy");

            //Deku
            iconx = 5;
            icony = 14;
            ID = "MaskDeku";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskDeku");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskDeku");

            //Redpotion
            iconx = 1;
            icony = 9;
            ID = "RedPotion";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemBottleWitch+ShopItemTradingPostRedPotion+ShopItemWitchRedPotion+ShopItemGoronRedPotion+ShopItemZoraRedPotion");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Any Red Potion");

            //Mama
            iconx = 0;
            icony = 7;
            ID = "TradeItemMamaLetter";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemMamaLetter");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemMamaLetter");

            //ROW 8=============================================================================================================

            //Keaton
            iconx = 0;
            icony = 15;
            ID = "MaskKeaton";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskKeaton");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskKeaton");

            //Bremon
            iconx = 1;
            icony = 15;
            ID = "MaskBremen";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskBremen");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskBremen");

            //Bunny
            iconx = 2;
            icony = 15;
            ID = "MaskBunnyHood";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskBunnyHood");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskBunnyHood");

            //DonGero
            iconx = 3;
            icony = 15;
            ID = "MaskDonGero";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskDonGero");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskDonGero");

            //Scents
            iconx = 4;
            icony = 15;
            ID = "MaskScents";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskScents");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskScents");

            //Goron
            iconx = 5;
            icony = 15;
            ID = "MaskGoron";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskGoron");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskGoron");

            //Greenpotion???
            iconx = 2;
            icony = 9;
            ID = "GreenPotion";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ShopItemTradingPostGreenPotion+ShopItemWitchGreenPotion+ShopItemBusinessScrubGreenPotion");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Any Green Potion");

            //Kafei Letter
            iconx = 1;
            icony = 7;
            ID = "TradeItemKafeiLetter";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemKafeiLetter");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemKafeiLetter");

            //ROW 9=============================================================================================================

            //romani
            iconx = 0;
            icony = 16;
            ID = "MaskRomani";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskRomani");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskRomani");

            //circus
            iconx = 1;
            icony = 16;
            ID = "MaskCircusLeader";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskCircusLeader");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskCircusLeader");

            //kafei
            iconx = 2;
            icony = 16;
            ID = "MaskKafei";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskKafei");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskKafei");

            //couples
            iconx = 3;
            icony = 16;
            ID = "MaskCouple";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskCouple");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskCouple");

            //masktruth
            iconx = 4;
            icony = 16;
            ID = "MaskTruth";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskTruth");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskTruth");

            //zora
            iconx = 5;
            icony = 16;
            ID = "MaskZora";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskZora");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskZora");

            //bluepotion
            iconx = 3;
            icony = 9;
            ID = "BluePotion";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ShopItemWitchBluePotion+ShopItemBusinessScrubBluePotion");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Any Blue Potion");

            //pendant
            iconx = 2;
            icony = 7;
            ID = "TradeItemPendant";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "TradeItemPendant");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "TradeItemPendant");

            //ROW 10=============================================================================================================

            //Kamaro
            iconx = 0;
            icony = 17;
            ID = "MaskKamaro";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskKamaro");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskKamaro");

            //Gibdo
            iconx = 1;
            icony = 17;
            ID = "MaskGibdo";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskGibdo");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskGibdo");

            //Garo
            iconx = 2;
            icony = 17;
            ID = "MaskGaro";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskGaro");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskGaro");

            //Captians
            iconx = 3;
            icony = 17;
            ID = "MaskCaptainHat";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskCaptainHat");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskCaptainHat");

            //Giants
            iconx = 4;
            icony = 17;
            ID = "MaskGiant";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskGiant");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskGiant");

            //Deity
            iconx = 5;
            icony = 17;
            ID = "MaskFierceDeity";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "MaskFierceDeity");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "MaskFierceDeity");

            //ClockFairy
            iconx = 5;
            icony = 22;
            ID = "CollectibleStrayFairyClockTown";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "CollectibleStrayFairyClockTown");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "CollectibleStrayFairyClockTown");

            //Notebook ???
            iconx = 5;
            icony = 21;
            ID = "ItemNotebook";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemNotebook");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemNotebook");

            //ROW 10=============================================================================================================

            //Swords
            instance.AddDisplayBox("DBsword", 0, 20);
            instance.AddTextToDefaultImage("DBsword", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "UpgradeBiggestBombBag+UpgradeBigBombBag+ItemBombBag");
            instance.AddDisplayItem("DBsword", "UpgradeGildedSword", 2, 20, "ProgressiveItems, disabled && UpgradeGildedSword || ProgressiveItems, enabled && Progressive Sword, 3");
            instance.AddDisplayItem("DBsword", "UpgradeRazorSword", 1, 20, "ProgressiveItems, disabled && UpgradeRazorSword || ProgressiveItems, enabled && Progressive Sword, 2");
            instance.AddDisplayItem("DBsword", "StartingSword", 0, 20, "ProgressiveItems, disabled && StartingSword || ProgressiveItems, enabled && Progressive Sword");

            //Odolawa
            iconx = 0;
            icony = 21;
            ID = "RemainsOdolwa";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "RemainsOdolwa");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "RemainsOdolwa");

            //WoodfallMap
            iconx = 0;
            icony = 22;
            ID = "ItemWoodfallMap";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemWoodfallMap");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemWoodfallMap");

            //WoodfallCompass
            iconx = 1;
            icony = 22;
            ID = "ItemWoodfallCompass";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemWoodfallCompass");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemWoodfallCompass");

            //WoodfallBK
            iconx = 2;
            icony = 22;
            ID = "ItemWoodfallBossKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemWoodfallBossKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemWoodfallBossKey");

            //WoodfallKey
            iconx = 3;
            icony = 22;
            ID = "ItemWoodfallKey1";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemWoodfallKey1");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemWoodfallKey1");

            //Woodfall Fairy
            iconx = 5;
            icony = 12;
            ID = "WoodfallStrayFairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Woodfall Stray Fairy");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Woodfall Stray Fairy");

            //Swamp Skull
            iconx = 4;
            icony = 22;
            ID = "SwampSkullToken";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Swamp Skulltula Spirit", false, -100, 0, -40);
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Swamp Skulltula Spirit");

            //ROW 11=============================================================================================================

            //Sheild
            instance.AddDisplayBox("DBShield", 4, 20);
            instance.AddTextToDefaultImage("DBShield", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "Hero's Shield+UpgradeMirrorShield");
            instance.AddDisplayItem("DBShield", "UpgradeMirrorShield", 5, 20, "UpgradeMirrorShield");
            instance.AddDisplayItem("DBShield", "UpgradeHeroShield", 4, 20, "Hero's Shield");

            //Goht
            iconx = 1;
            icony = 21;
            ID = "RemainsGoht";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "RemainsGoht");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "RemainsGoht");

            //Snowhead Map
            iconx = 0;
            icony = 22;
            ID = "ItemSnowheadMap";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemSnowheadMap");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemSnowheadMap");

            //Snowhead Compass
            iconx = 1;
            icony = 22;
            ID = "ItemSnowheadCompass";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemSnowheadCompass");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemSnowheadCompass");

            //Snowhead BK
            iconx = 2;
            icony = 22;
            ID = "ItemSnowheadBossKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemSnowheadBossKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemSnowheadBossKey");

            //Snowhead Key
            iconx = 3;
            icony = 22;
            ID = "SnowheadSmallKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Snowhead Small Key");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Snowhead Small Key");

            //Snowhead Fairy
            iconx = 4;
            icony = 12;
            ID = "SnowheadStrayFairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Snowhead Stray Fairy");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Snowhead Stray Fairy");

            //Magic
            iconx = 3;
            icony = 7;
            ID = "Magic";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Magic Power || Extended Magic Power");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Magic Power+Extended Magic Power");


            //ROW 12=============================================================================================================

            //Wallet
            instance.AddDisplayBox("DBWallet", 5, 3);
            instance.AddTextToDefaultImage("DBWallet", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "UpgradeRoyalWallet+UpgradeGiantWallet+UpgradeAdultWallet");
            instance.AddDisplayItem("DBWallet", "UpgradeRoyalWallet", 5, 1, "ProgressiveItems, disabled && UpgradeRoyalWallet || ProgressiveItems, enabled && MMRTProgressiveWalletX3");
            instance.AddDisplayItem("DBWallet", "UpgradeGiantWallet", 5, 4, "ProgressiveItems, disabled && UpgradeGiantWallet || ProgressiveItems, enabled && MMRTProgressiveWalletX2");
            instance.AddDisplayItem("DBWallet", "UpgradeAdultWallet", 5, 3, "ProgressiveItems, disabled && UpgradeAdultWallet || ProgressiveItems, enabled && MMRTProgressiveWallet");

            //Gyorg
            iconx = 2;
            icony = 21;
            ID = "RemainsGyorg";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "RemainsGyorg");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "RemainsGyorg");

            //Greatbay Map
            iconx = 0;
            icony = 22;
            ID = "ItemGreatBayMap";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemGreatBayMap");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemGreatBayMap");

            //Greatbay Compass
            iconx = 1;
            icony = 22;
            ID = "ItemGreatBayCompass";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemGreatBayCompass");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemGreatBayCompass");

            //Greatbay Boss Key
            iconx = 2;
            icony = 22;
            ID = "ItemGreatBayBossKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemGreatBayBossKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemGreatBayBossKey");

            //Greatbay Kkey
            iconx = 3;
            icony = 22;
            ID = "ItemGreatBayKey1";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemGreatBayKey1");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemGreatBayKey1");

            //Greatbay Fairy
            iconx = 3;
            icony = 12;
            ID = "GreatBayStrayFairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Great Bay Stray Fairy");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Great Bay Stray Fairy");

            //Ocean Skull
            iconx = 4;
            icony = 22;
            ID = "OceanSkullToken";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Ocean Skulltula Spirit", false, -200, -40, 80);
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Ocean Skulltula Spirit");

            //ROW 13=============================================================================================================

            //GO MODE
            iconx = 5;
            icony = 13;
            ID = "AreaMoonAccess";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "AreaMoonAccess");

            //Twinmold
            iconx = 3;
            icony = 21;
            ID = "RemainsTwinmold";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "RemainsTwinmold");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "RemainsTwinmold");

            //StoneTower Map
            iconx = 0;
            icony = 22;
            ID = "ItemStoneTowerMap";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemStoneTowerMap");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemStoneTowerMap");

            //StoneTower Compass
            iconx = 1;
            icony = 22;
            ID = "ItemStoneTowerCompass";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemStoneTowerCompass");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemStoneTowerCompass");

            //StoneTower Boss Key
            iconx = 2;
            icony = 22;
            ID = "ItemStoneTowerBossKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddTextToDefaultImage($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "ItemStoneTowerBossKey");
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "ItemStoneTowerBossKey");

            //StoneTower Key
            iconx = 3;
            icony = 22;
            ID = "StoneTowerKey";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Stone Tower Small Key");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Stone Tower Small Key");

            //StoneTower Fairy
            iconx = 2;
            icony = 12;
            ID = "StoneTowerStrayFairy";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "Stone Tower Stray Fairy");
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Stone Tower Stray Fairy");

            //Double Defence
            iconx = 5;
            icony = 7;
            ID = "FairyDoubleDefense";
            instance.AddDisplayBox($"DB{ID}", iconx, icony);
            instance.AddDisplayItem($"DB{ID}", ID, iconx, icony, "FairyDoubleDefense", true);
            instance.AddStaticTextBox($"DB{ID}", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "FairyDoubleDefense");


            File.WriteAllText(Path.Combine(References.TestingPaths.GetDevCodePath(), "Windows Form Frontend", "ItemTrackerData", "MMRItemTracker.json"), Newtonsoft.Json.JsonConvert.SerializeObject(instance, MMR_Tracker_V3.Testing._NewtonsoftJsonSerializerOptions));
            File.WriteAllText(Path.Combine("ItemTrackerData", "MMRItemTracker.json"), Newtonsoft.Json.JsonConvert.SerializeObject(instance, MMR_Tracker_V3.Testing._NewtonsoftJsonSerializerOptions));

        }

        public static void PMRCreateData()
        {
            MMR_Tracker_V3.OtherGames.PaperMarioRando.ReadData.ReadEadges(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
        }

        public static void TPRCreateData()
        {
            MMR_Tracker_V3.OtherGames.TPRando.ReadAndParseData.CreateFiles(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Testing.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);

            //Testing.PrintObjectToConsole(MMR_Tracker_V3.OtherGames.TPRando.ParseMacrosFromCode.ReadMacrosFromCode());

            List<string> Areas = MainInterface.InstanceContainer.Instance.LocationPool.Values.Select(x => x.GetDictEntry(MainInterface.InstanceContainer.Instance).Area).Distinct().ToList();
            Testing.PrintObjectToConsole(Areas);

            List<string> Bugs = MainInterface.InstanceContainer.Instance.ItemPool.Values.Where(x => x.Id.StartsWith("Female_") || x.Id.StartsWith("Male_")).Select(x => x.Id).ToList();
            string AnyBug = string.Join(" or ", Bugs);
            Debug.WriteLine(AnyBug);

            string Root = MainInterface.InstanceContainer.Instance.EntrancePool.RootArea;
            Debug.WriteLine($"Root Area {Root} Valid {MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.ContainsKey(Root)} Aquired {MainInterface.InstanceContainer.logicCalculation.LogicEntryAquired(Root, new List<string>())}");
        }

        public static void OOTMMCreateData()
        {
            MMR_Tracker_V3.OtherGames.OOTMMV2.GenData.ReadData(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            foreach(var i in MainInterface.InstanceContainer.Instance.EntrancePool.AreaList.Values)
            {
                foreach(var j in i.RandomizableExits(MainInterface.InstanceContainer.Instance)) { j.Value.RandomizedState = MiscData.RandomizedState.Unrandomized; }
            }
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Testing.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }

        internal static void WWRCreateData()
        {
            MMR_Tracker_V3.OtherGames.WindWakerRando.ReadAndParseData.CreateFiles(out MMRData.LogicFile Logic, out LogicDictionaryData.LogicDictionary dictionary);
            WinFormInstanceCreation.CreateWinFormInstance(JsonConvert.SerializeObject(Logic), JsonConvert.SerializeObject(dictionary));
            Testing.TestLogicForInvalidItems(MainInterface.InstanceContainer);
            Testing.TestLocationsForInvalidVanillaItem(MainInterface.InstanceContainer);
        }

        public static void TestFuncParse()
        {
            MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem() { Id = "Test" };
            string Test = "setting(hookshotAnywhereOot) && !setting(ageChange, none), small_keys_forest(5), small_keys_forest(2)";

            string result =  MMR_Tracker_V3.OtherGames.OOTMMV2.FunctionParsing.ParseCondFunc(Test, logicItem);

            Debug.WriteLine(result);
        }
    }
}
