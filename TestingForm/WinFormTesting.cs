using MMR_Tracker_V3;
using MMR_Tracker_V3.TrackerObjectExtentions;
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
            Debug.WriteLine(JsonConvert.SerializeObject(LastSelectedObject, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
            if (LastSelectedObject is LocationData.LocationObject DebugLocObj)
            {
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(DebugLocObj.GetDictEntry(), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                RandomizedItem = DebugLocObj.Randomizeditem.Item;

            }
            if (LastSelectedObject is LocationData.LocationProxy DebugProxyObj)
            {
                var ProxyRef = MainInterface.InstanceContainer.Instance.GetLocationByID(DebugProxyObj.ReferenceID);
                Debug.WriteLine($"Proxied Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(ProxyRef?.GetDictEntry(), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                RandomizedItem = ProxyRef.Randomizeditem.Item;
            }
            if (LastSelectedObject is EntranceData.EntranceRandoExit DebugEntObj)
            {
                Debug.WriteLine($"Dictionary Entry");
                Debug.WriteLine(JsonConvert.SerializeObject(DebugEntObj.GetDictEntry(), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                RandomizedExit = DebugEntObj.DestinationExit;
            }
            if (RandomizedItem !=null)
            {
                var Item = MainInterface.InstanceContainer.Instance.GetItemByID(RandomizedItem);
                if (Item is not null)
                {
                    Debug.WriteLine($"Randomized Item");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                    Debug.WriteLine($"Randomized Item Dictionary Entry");
                    Debug.WriteLine(JsonConvert.SerializeObject(Item?.GetDictEntry(), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                }
            }
            if (RandomizedExit is not null)
            {
                var Destination = RandomizedExit.AsExit(MainInterface.InstanceContainer.Instance);
                if (Destination is not null)
                {
                    Debug.WriteLine($"Destination");
                    Debug.WriteLine(JsonConvert.SerializeObject(Destination, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
                    Debug.WriteLine($"Destination Dictionary Entry");
                    Debug.WriteLine(JsonConvert.SerializeObject(Destination?.GetDictEntry(), MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
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
    }
}
