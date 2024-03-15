using Microsoft.VisualBasic.Logging;
using MMR_Tracker_V3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static TestingForm.GameDataCreation.OOTMMV3.DataClasses;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class OOTMMUtility
    {
        public static string ReplaceVariableWithParam(string LogicLine, string Param, string Value)
        {
            return Regex.Replace(LogicLine, @$"\b{Param}\b", Value);
        }
        public static OOTMMLogicFunction? IsLogicFunction(string Key)
        {
            int Parlevel = 0;
            int MaxParlevel = 0;
            string Func = string.Empty;
            string ParamString = string.Empty;
            foreach (var i in Key)
            {
                if (i == '(') 
                {  
                    Parlevel++;
                    if (MaxParlevel < Parlevel) { MaxParlevel = Parlevel; }
                    continue;
                }
                if (i == ')') {  Parlevel--; continue; }
                if (Parlevel > 0) { ParamString += i; }
                else { Func += i; }
            }
            if (MaxParlevel != 1 || Parlevel != 0) { return null; }
            return new OOTMMLogicFunction(Func, ParamString);
        }

        public static string GetMQString(OOTMMLocationArea data, bool ISMQ)
        {
            if (data.boss || string.IsNullOrWhiteSpace(data.dungeon)) { return string.Empty; }
            return $" && setting(MasterQuest, {data.dungeon}, {ISMQ})";
        }

        public static string AddGameCodeToLogicID(string ID, string GameCode)
        {
            if (ID.StartsWith("MM ") || ID.StartsWith("OOT ")) { return ID; }
            return $"{GameCode} {ID}";
        }
    }
}
