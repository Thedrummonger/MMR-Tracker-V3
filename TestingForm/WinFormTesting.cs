using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtensions;
using MMR_Tracker_V3.TrackerObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows_Form_Frontend;
using static MMR_Tracker_V3.TrackerObjects.MiscData;
using TDMUtils;

namespace TestingForm
{
    internal class WinFormTesting
    {
        public static object LastSelectedObject;
        public static void ActivateWinFormInterface()
        {
            Debug.WriteLine($"Loading Main Interface");
            if (!WinformLoaded())
            {
                Debug.WriteLine($"Initializing Main Interface");
                _ = new MainInterface(true);
                EventListeners.BuildWinFormEventListeners();
            }
            MainInterface.CurrentProgram.Show();
        }
        public static bool WinformLoaded()
        {
            return MainInterface.CurrentProgram is not null;
        }
        public static bool WinformInstanceLoaded()
        {
            return WinformLoaded() && MainInterface.InstanceContainer.Instance is not null;
        }
        public static bool CanSaveWinformTrackerState()
        {
            return WinformLoaded() && MainInterface.InstanceContainer?.Instance is not null;
        }
        public static void SaveWinformTrackerState()
        {
            if (!CanSaveWinformTrackerState()) { return; }
            LogicRecreation.SaveTrackerState(MainInterface.InstanceContainer);
            Debug.WriteLine($"Tracker Instance Saved;");
        }
        public static bool CanLoadWinformTrackerState()
        {
            return WinformLoaded() && MainInterface.InstanceContainer?.Instance is not null && LogicRecreation.CurrentSaveState is not null;
        }
        public static void LoadWinformTrackerState()
        {
            if (!CanLoadWinformTrackerState()) { return; }
            LogicRecreation.LoadTrackerState(MainInterface.InstanceContainer);
            MainInterface.InstanceContainer.logicCalculation.CalculateLogic();
            MainInterface.CurrentProgram.UpdateUI();
        }

        public static void PrintWinformSelectedObject()
        {
            if (LastSelectedObject is null) { return; }

            string RandomizedItem = null;
            EntranceData.EntranceRandoDestination RandomizedExit = null;
            Debug.WriteLine($"Data for {LastSelectedObject}=========================================================");
            Debug.WriteLine(JsonConvert.SerializeObject(LastSelectedObject, NewtonsoftExtensions.DefaultSerializerSettings));
            if (LastSelectedObject is CheckableLocation DebugCLOObj)
            {
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(DebugCLOObj.GetAbstractDictEntry().ToFormattedJson());
            }

            if (LastSelectedObject is LocationData.LocationObject DebugLocObj) { RandomizedItem = DebugLocObj.Randomizeditem.Item; }
            else if (LastSelectedObject is EntranceData.EntranceRandoExit DebugEntObj) { RandomizedExit = DebugEntObj.DestinationExit; }
            else if (LastSelectedObject is LocationData.LocationProxy DebugProxyObj)
            {
                Debug.WriteLine($"Proxied Location");
                Debug.WriteLine(DebugProxyObj.GetReferenceLocation().ToFormattedJson());
                Debug.WriteLine($"Proxied Location Dictionary Entry");
                Debug.WriteLine(DebugProxyObj.GetReferenceLocation().GetDictEntry().ToFormattedJson());
                Debug.WriteLine($"Logic Inheritance");
                Debug.WriteLine(DebugProxyObj.GetLogicInheritance().ToFormattedJson());
                Debug.WriteLine($"Logic Inheritance Dictionary Entry");
                Debug.WriteLine(DebugProxyObj.GetLogicInheritance().GetAbstractDictEntry().ToFormattedJson());
                RandomizedItem = DebugProxyObj.GetReferenceLocation().Randomizeditem.Item;
            }

            if (RandomizedItem !=null)
            {
                var Item = MainInterface.InstanceContainer.Instance.GetItemByID(RandomizedItem);
                if (Item is not null)
                {
                    Debug.WriteLine($"Randomized Item");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item, NewtonsoftExtensions.DefaultSerializerSettings));
                    Debug.WriteLine($"Randomized Item Dictionary Entry");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item?.GetDictEntry(), NewtonsoftExtensions.DefaultSerializerSettings));
                }
            }
            if (RandomizedExit is not null)
            {
                var Destination = RandomizedExit.AsExit(MainInterface.InstanceContainer.Instance);
                if (Destination is not null)
                {
                    Debug.WriteLine($"Destination");
                    Debug.WriteLine(JsonConvert.SerializeObject(Destination, NewtonsoftExtensions.DefaultSerializerSettings));
                    Debug.WriteLine($"Destination Dictionary Entry");
                    Debug.WriteLine(JsonConvert.SerializeObject(Destination?.GetDictEntry(), NewtonsoftExtensions.DefaultSerializerSettings));
                }
            }
            Debug.WriteLine(MainInterface.CurrentProgram.ActiveControl.Name);
        }

        internal static void CleanUpWinForm()
        {
            MainInterface.CurrentProgram = null;
            MainInterface.InstanceContainer = new MMR_Tracker_V3.TrackerObjects.InstanceData.InstanceContainer();
            LastSelectedObject = null;
        }

        internal static void GiveItem()
        {
            CheckItemForm checkItemForm = new([null], MainInterface.InstanceContainer);
            checkItemForm.ShowDialog();
            if (checkItemForm._Result.Count != 1) { return; }
            var SelectedItem = checkItemForm._Result[0];
            var Item = SelectedItem.GetItemLocation().ItemData.ItemID;
            var ItemObject = MainInterface.InstanceContainer.Instance.GetItemByID(Item);
            if (ItemObject is null) { return; }
            ItemObject.AmountAquiredOnline.SetIfEmpty(MainInterface.InstanceContainer.netConnection.PlayerID, 0);
            ItemObject.AmountAquiredOnline[MainInterface.InstanceContainer.netConnection.PlayerID]++;
        }
    }
}
