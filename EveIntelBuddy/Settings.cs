using System;
using System.IO;

namespace EveIntelBuddy
{
    public static class Settings
    {
        public const string PresetFolderName = "presets";
        public const string SettingsFileName = "settings.txt";
        
        public static string ChatLogFolder = null;

        public static void LoadOrInitialize()
        {
            if (File.Exists(Settings.SettingsFileName))
            {
                Load();
            }
            else
            {
                Initialize();
            }

            if (!Directory.Exists(PresetFolderName))
            {
                Directory.CreateDirectory(PresetFolderName);
            }
        }

        private static void Load()
        {
            ChatLogFolder = File.ReadAllText(SettingsFileName);
        }

        private static void Initialize()
        {
            Console.WriteLine("Hello there! This seems to be your very first time using this app.");
            Console.WriteLine("In order for it to work, you'll need to enable Chat Logs in your EVE Settings.");
            Console.WriteLine("Please paste your Chatlogs directory here: ");
            var path = Console.ReadLine();
            while (!Directory.Exists(path))
            {
                Console.WriteLine("The Folder you've linked doesn't seem to exists. Try again.");
                path = Console.ReadLine();
            }

            ChatLogFolder = path;
            File.WriteAllText(SettingsFileName, ChatLogFolder);
        }
    }
}