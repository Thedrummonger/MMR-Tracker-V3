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

namespace TestingForm
{
    internal class Utility
    {
        public static string CreateTestingFile(string Name, string Extention = "txt")
        {
            return Path.Combine(TestingReferences.GetDevTestingPath(), $"{Name}.{Extention}");
        }

        public static void CreateTestingFile(string Name, object Data, string Extention = "txt")
        {
            File.WriteAllText(CreateTestingFile(Name, Extention), JsonConvert.SerializeObject(Data, MMR_Tracker_V3.Utility._NewtonsoftJsonSerializerOptions));
        }

        public static void ActivateWinFormInterface()
        {
            if (!TestingForm.WinformLoaded())
            {
                TestingForm.TestingInterface = new MainInterface(true);
                TestingForm.TestingInterface.WinFormClosing += TestingForm.MainInterface_FormClosing;
            }
            TestingForm.TestingInterface.Show();
        }
        public static void TestLogicForInvalidItems(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LogicFile.Logic)
            {
                var logicitems = i.ConditionalItems.SelectMany(x => x).Concat(i.RequiredItems).ToArray();
                foreach (var l in logicitems)
                {
                    Container.logicCalculation.LogicEntryAquired(l, new List<string>());
                }
            }
        }
        public static void TestLocationsForInvalidVanillaItem(MiscData.InstanceContainer Container)
        {
            foreach (var i in Container.Instance.LocationPool)
            {
                string OriginalItem = i.Value.GetDictEntry(Container.Instance).OriginalItem;
                if (Container.Instance.GetItemByID(OriginalItem) is null)
                {
                    Debug.WriteLine($"{OriginalItem} at loc {i.Key} is not a valid item");
                }
            }
        }
    }
}
