using Newtonsoft.Json;
using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using TDMUtils;

namespace MMR_Tracker_V3
{
    public class UpdateManager
    {
        public static TrackerVersionStatus GetTrackerVersionStatus()
        {
            TrackerVersionStatus VersionStatus = new TrackerVersionStatus();

            VersionStatus.DoUpdateCheck = TrackerObjects.TrackerSettings.ReadDefaultOptionsFile().CheckForUpdate;

            if (!VersionStatus.DoUpdateCheck)
            {
                Debug.WriteLine("Checking for updates not enabled");
                return VersionStatus;
            }
            try
            {
                var client = new GitHubClient(new ProductHeaderValue("MMR-Tracker-V3"));
                VersionStatus.LatestVersion = client.Repository.Release.GetLatest("Thedrummonger", "MMR-Tracker-V3").Result;
                var VersionSatus = VersionStatus.LatestVersion.TagName.AsVersion().CompareTo(References.trackerVersion);

                Debug.WriteLine($"Latest Version: {VersionStatus.LatestVersion.TagName} Current Version {References.trackerVersion}");
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

        public static void DisableUpdateChecks()
        {
            var DefaultOptions = TrackerObjects.TrackerSettings.ReadDefaultOptionsFile();
            DefaultOptions.ToggleUpdateCheck(false);
            TrackerObjects.TrackerSettings.WriteOptionFile(DefaultOptions);
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
