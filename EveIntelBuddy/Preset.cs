using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace EveIntelBuddy
{
    public class Preset
    {
        private static readonly int PresetFolderNameLength = Settings.PresetFolderName.Length + 1;
        private static readonly int TotalExtraLength = Settings.PresetFolderName.Length + ".txt".Length + 1;

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

        private static Preset Load(string fileName)
        {
            var content = File.ReadAllLines(fileName);
            return new Preset(content[0], new List<string>(content[1].Split(',')));
        }
        
        public static Preset? LoadExistingPreset()
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
                if (Regex.IsMatch(input, "^[0-9]+$"))
                {
                    var index = int.Parse(input);
                    if (index == 0)
                    {
                        return null;
                    }

                    if (index > files.Length)
                    {
                        return null;
                    }

                    return Load(files[index - 1]);
                }

                var matchingFileNames = files.Where(x => x.Equals($"{Settings.PresetFolderName}/{input.ToLower()}.txt")).ToArray();
                if (matchingFileNames.Count() == 1)
                {
                    return Load(matchingFileNames.First());
                }
            }
            
            return null;
        }
        
        public static Preset CreateNewPreset()
        {
            var channel = Utility.ConsolePrompt("Which In-Game Chat Channel should I watch?");
            
            var wordString = Utility.ConsolePrompt("Which systems (or words) should raise an alert?");
            var words = wordString
                    .Replace(',', ' ')
                    .Split(' ')
                    .Where(x => x.Length > 0)
                    .Select(x => x.ToLower())
                    .ToList();
            
            return new Preset(channel, words);
        }
    }
}