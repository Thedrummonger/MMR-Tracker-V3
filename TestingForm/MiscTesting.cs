using MMR_Tracker_V3.DataStructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm
{
    internal class MiscTesting
    {

        public static void TestFuncParse()
        {
            MMRData.JsonFormatLogicItem logicItem = new MMRData.JsonFormatLogicItem() { Id = "Test" };
            string Test = "setting(hookshotAnywhereOot) && !setting(ageChange, none), small_keys_forest(5), small_keys_forest(2)";

            string result = MMR_Tracker_V3.GameDataCreation.OOTMMV2.FunctionParsing.ParseCondFunc(Test, logicItem);

            Debug.WriteLine(result);
        }
    }
}
