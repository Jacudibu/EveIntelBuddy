using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EveIntelBuddy
{
    public class Preset
    {
        private static readonly int PresetFolderNameLength = Settings.PresetFolderName.Length + 1;
        private static readonly int TotalExtraLength = Settings.PresetFolderName.Length - ".txt".Length - 1;

        public readonly string Channel;
        public readonly List<string> Words;

        public Preset(string channel, List<string> words)
        {
            Channel = channel;
            Words = words;
        }
        
        public void Save(string name)
        {
            File.WriteAllText($"{Settings.PresetFolderName}/{name}.txt", Channel + "\n" + string.Join(",", Words));
        }
        
        public static void PresetDialogue()
        {
            var files = Directory.GetFiles(Settings.PresetFolderName);
            if (files.Length > 0)
            {
                Console.WriteLine("Use one of the following presets?");
                for (var i = 0; i < files.Length; i++)
                {
                    Console.WriteLine($"({i + 1}) {files[i].Substring(PresetFolderNameLength, files[i].Length - TotalExtraLength)}");
                }

                var input = Console.ReadLine() ?? "";
                if (Regex.IsMatch(input, "[0-9]"))
                {
                    var index = int.Parse(input);
                    if (index == 0)
                    {
                        
                    }
                    
                    
                    
                }
                
            }
        }
    }
}