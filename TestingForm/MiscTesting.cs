using System.Diagnostics;
using TDMUtils;

namespace TestingForm
{
    internal class MiscTesting
    {

        public static void TestFuncParse()
        {
            string[] testStrings = ["Peanut", "Has(x)", "Hookshot", "Wallet(x, y, count)"];
            foreach(string testString in testStrings)
            {
                var Result = GameDataCreation.OOTMMV3.OOTMMUtility.IsLogicFunction(testString);
                if (Result is null) { Debug.WriteLine($"{testString} was not function"); }
                else { Debug.WriteLine($"{testString}\n{Result.ToFormattedJson()}"); }
            }
        }
    }
}
