using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MMR_Tracker_V3
{
    public class UpdateManager
    {
        public static TrackerVersionStatus GetTrackerVersionStatus()
        {
            TrackerVersionStatus VersionStatus = new TrackerVersionStatus();
            if (File.Exists(References.Globalpaths.OptionFile))
            {
                LogicObjects.OptionFile options;
                try { options = JsonConvert.DeserializeObject<LogicObjects.OptionFile>(File.ReadAllText(References.Globalpaths.OptionFile)); }
                catch { Debug.WriteLine("could not parse options.txt"); return VersionStatus; }
                if (!options.CheckForUpdate) {
                    Debug.WriteLine("Checking for updates not enabled");
                    return VersionStatus; 
                }
            }
            else { return VersionStatus; }
            VersionStatus.DoUpdateCheck = true;
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("MMR-Tracker-V3"));
                VersionStatus.LatestVersion = client.Repository.Release.GetLatest("Thedrummonger", "MMR-Tracker").Result;
                var VersionSatus = CompareVersions(VersionStatus.LatestVersion.TagName, References.trackerVersion);

                Debug.WriteLine($"Latest Version: { VersionStatus.LatestVersion.TagName } Current Version { References.trackerVersion }");
                if (VersionSatus < 0) { Debug.WriteLine($"Using Unreleased Dev Version"); VersionStatus.VersionStatus = versionStatus.dev; }
                else if (VersionSatus > 0) { Debug.WriteLine($"Using Outdated Version"); VersionStatus.VersionStatus = versionStatus.outdated; }
                else if (VersionSatus == 0) { Debug.WriteLine($"Using Current Version"); VersionStatus.VersionStatus = versionStatus.current; }
                return VersionStatus;
            }
            catch (Exception ex) 
            { 
                Debug.WriteLine("Could not get github release data");
                Debug.WriteLine(ex); 
                return VersionStatus;
            }
        }

        public class TrackerVersionStatus
        {
            public bool DoUpdateCheck = false;
            public versionStatus VersionStatus = versionStatus.current;
            public Release LatestVersion;
        }

        public enum versionStatus
        {
            dev,
            outdated,
            current
        }

        public static int CompareVersions(string V1, string V2)
        {
            if (!V1.Contains(".")) { V1 += ".0"; }
            if (!V2.Contains(".")) { V2 += ".0"; }
            var CleanedV1 = new Version(string.Join("", V1.Where(x => char.IsDigit(x) || x == '.')));
            var CleanedV2 = new Version(string.Join("", V2.Where(x => char.IsDigit(x) || x == '.')));
            return CleanedV1.CompareTo(CleanedV2);
        }
    }
}
