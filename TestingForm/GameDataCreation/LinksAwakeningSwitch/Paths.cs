﻿namespace TestingForm.GameDataCreation.LinksAwakeningSwitch
{
    internal class Paths
    {
        public static string RandoTestFolderPath()
        {
            return Path.Combine(TestingReferences.GetDevTestingPath(), "LASRando");
        }
        public static string RandoSourcePath() 
        {
            return Path.Combine(RandoTestFolderPath(), "LAS-Randomizer-master");
        }
        public static string RandoDataPath()
        {
            return Path.Combine(RandoSourcePath(), "Data");
        }
        public static string RandoItemsFile()
        {
            return Path.Combine(RandoDataPath(), "items.yml");
        }
        public static string RandoLocationsFile()
        {
            return Path.Combine(RandoDataPath(), "locations.yml");
        }
        public static string RandoLogicFile()
        {
            return Path.Combine(RandoDataPath(), "logic.yml");
        }
    }
}
