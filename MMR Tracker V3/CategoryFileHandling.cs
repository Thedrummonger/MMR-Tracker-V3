using MathNet.Symbolics;
using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;

namespace MMR_Tracker_V3
{
    internal class CategoryFileHandling
    {
        public static Dictionary<string, int> GetCategoriesFromFile(InstanceData.TrackerInstance Instance)
        {
            Dictionary<string, int> Groups = [];
            if (Instance.LogicDictionary.AreaOrder is not null && Instance.LogicDictionary.AreaOrder.Length > 0)
            {
                foreach(var Line in Instance.LogicDictionary.AreaOrder)
                {
                    if (!Groups.ContainsKey(Line))
                    {
                        Groups.Add(Line.Trim(), Groups.Count);
                    }
                }
                return Groups;
            }
            else if (File.Exists(References.Globalpaths.CategoryTextFile))
            {
                bool AtGame = true;
                foreach (var i in File.ReadAllLines(References.Globalpaths.CategoryTextFile))
                {
                    var Line = i.ToLower().Trim();
                    if (string.IsNullOrWhiteSpace(Line) || Line.StartsWith("//")) { continue; }
                    if (Line.StartsWith("#gamecodestart:"))
                    {
                        AtGame = Line.Replace("#gamecodestart:", "").Trim().Split(',')
                            .Select(y => y.Trim()).Contains(Instance.LogicFile.GameCode.ToLower());
                        continue;
                    }
                    if (Line.StartsWith("#gamecodeend:")) { AtGame = true; continue; }

                    if (!Groups.ContainsKey(Line) && AtGame)
                    {
                        Groups.Add(Line.Trim(), Groups.Count);
                    }
                }
                return Groups;
            }
            else { return []; }
        }
    }
}
