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

            //ROW 1=============================================================================================================

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

            //PowderKeg

            //PictoBox

            //Lens

            //Hookshot

            //Fairy Sword

            //Song Epona

            //BossaNova

            //Swamp Deed

            //ROW 4=============================================================================================================

            //Bottle

            //Milk

            //GoldDust

            //SeaHourse

            //Chateu

            //Soaring

            //Elegey
            instance.AddDisplayBox("DBSongElegy", 3, 24);
            instance.AddTextToDefaultImage("DBSongElegy", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.HasSeen, "SongElegy");
            instance.AddDisplayItem("DBSongElegy", "SongElegy", 3, 24, "SongElegy", false, 100, 80, 0);
            instance.AddStaticTextBox("DBSongElegy", WinFormImageUtils.TextPosition.bottomLeft, WinFormImageUtils.TextType.text, "Elegy");

            //Mountain Deed

            //ROW 5=============================================================================================================

            //Bottle Fairy

            //Bottle Deku

            //Bottle Fish

            //Bottle Bug

            //Bottle Small poe

            //Song Storms

            //Song Oath

            //OceanDeed

            //ROW 6=============================================================================================================

            //Bottle Big Poe

            //Bottle Water

            //Bottle HS

            //Bottle Zora

            //Bottle Mushroom

            //Heart Piece
            instance.AddDisplayBox("DBHeartPieces", 4, 7);
            instance.AddDisplayItem("DBHeartPieces", "HeartPiece", 4, 7, "Piece of Heart");
            instance.AddTextToDisplayItem("DBHeartPieces", "HeartPiece", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Piece of Heart");

            //Heart containers
            instance.AddDisplayBox("DBHeartContainer", 5, 7);
            instance.AddDisplayItem("DBHeartContainer", "HeartContainer", 5, 7, "Heart Container");
            instance.AddTextToDisplayItem("DBHeartContainer", "HeartContainer", WinFormImageUtils.TextPosition.topRight, WinFormImageUtils.TextType.ItemCount, "Heart Container");

            //Room Key

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

        }
    }
}
