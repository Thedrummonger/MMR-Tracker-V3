using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class OOTMMPaths
    {
        public static string OOTMMTestingFolderPath { get { return Path.Combine(TestingReferences.GetDevTestingPath(), "OOTMM"); } }
        public static string DataSRCFolderPath { get { return Path.Combine(OOTMMTestingFolderPath, "OoTMM-master", "packages", "data", "src"); } }
        public static string MMWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "mm"); } }
        public static string OOTWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "oot"); } }
        public static string MQWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "mq"); } }
        public static string CommonMacroFile { get { return Path.Combine(DataSRCFolderPath, "macros", "macros_common.yml"); } }
        public static string MMMacroFile { get { return Path.Combine(DataSRCFolderPath, "macros", "macros_mm.yml"); } }
        public static string OOTMacroFile { get { return Path.Combine(DataSRCFolderPath, "macros", "macros_oot.yml"); } }
        public static string OOTPoolFile { get { return Path.Combine(DataSRCFolderPath, "pool", "pool_oot.csv"); } }
        public static string MMPoolFile { get { return Path.Combine(DataSRCFolderPath, "pool", "pool_mm.csv"); } }
        public static string OOTHintsFile { get { return Path.Combine(DataSRCFolderPath, "hints", "hints_oot.csv"); } }
        public static string MMHintsFile { get { return Path.Combine(DataSRCFolderPath, "hints", "hints_mm.csv"); } }
        public static string EntranceFile { get { return Path.Combine(DataSRCFolderPath, "defs", "entrances.yml"); } }
        public static string ExtraDataFile { get { return Path.Combine(TestingReferences.GetOtherGameDataPath("OOTMMV3"), "extradata.json"); } }
    }
}
