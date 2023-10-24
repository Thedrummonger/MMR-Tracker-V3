using MMR_Tracker_V3;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
            File.WriteAllText(CreateTestingFile(Name, Extention), JsonConvert.SerializeObject(Data, Testing._NewtonsoftJsonSerializerOptions));
        }

        public static void ActivateWinFormInterface()
        {
            if (TestingForm.TestingInterface is null || MainInterface.CurrentProgram is null) { TestingForm.TestingInterface = new MainInterface(); }
            TestingForm.TestingInterface.Show();
        }
    }
}
