﻿using MMR_Tracker_V3.TrackerObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CLIFrontEnd
{
    internal class CLIUtility
    {
        public static MiscData.Divider CreateDivider(string key = "=")
        {
            string Divider = "";
            for (var i = 0; i < Console.WindowWidth; i++) { Divider += key; }
            return new MiscData.Divider(Divider);
        }
    }
}