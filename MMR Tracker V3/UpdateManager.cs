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
                InstanceData.OptionFile options;
                try { options = JsonConvert.DeserializeObject<InstanceData.OptionFile>(File.ReadAllText(References.Globalpaths.OptionFile)); }
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
                VersionStatus.LatestVersion = client.Repository.Release.GetLatest("Thedrummonger", "MMR-Tracker-V3").Result;
                var VersionSatus = VersionStatus.LatestVersion.TagName.AsVersion().CompareTo(References.trackerVersion);

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

    }
}
