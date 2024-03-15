using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingForm.GameDataCreation.OOTMMV3
{
    internal class Paths
    {
        public static string OOTMMTestingFolderPath { get { return Path.Combine(TestingReferences.GetDevTestingPath(), "OOTMM"); } }
        public static string DataSRCFolderPath { get { return Path.Combine(OOTMMTestingFolderPath, "OoTMM-master", "packages", "data", "src"); } }
        public static string MMWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "mm"); } }
        public static string OOTWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "oot"); } }
        public static string MQWorldFolderPath { get { return Path.Combine(DataSRCFolderPath, "world", "mq"); } }
    }
}
