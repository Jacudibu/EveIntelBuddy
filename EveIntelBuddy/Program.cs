using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace EveIntelBuddy
{
    class Program
    {
        private const string FilePath = "/ssdapps/eve-online/drive_c/users/jacudibu/My Documents/EVE/logs/Chatlogs/";
        private static string ChannelName = "Bean-Intel";
        private static List<string> _watchedSystems = new List<string>() {"6V-D0E", "LS3-HP", "QX-4HO", "BVRQ-O"};
        private const int BeepDurationInSeconds = 1;
        
        private static FileStream? _intelFileStream;
        private static StreamReader? _intelStreamReader;
        
        static void Main(string[] args)
        {
            var channel = ConsolePrompt("Which In-Game Chat Channel should I watch? ");
            ChannelName = channel.Equals("default") ? "Bean-Intel" : channel;
            
            var systems = ConsolePrompt("Which systems (or words) should raise an alert? ");
            if (systems.Equals("default"))
            {
                _watchedSystems = new List<string>() {"6V-D0E", "LS3-HP", "QX-4HO", "BVRQ-O"}
                    .Select(x => x.ToLower())
                    .ToList();
            }
            else
            {
                _watchedSystems = systems
                    .Replace(',', ' ')
                    .Split(' ')
                    .Where(x => x.Length > 0)
                    .Select(x => x.ToLower())
                    .ToList();
            }
            
            Console.WriteLine($"Okay, I'll keep an eye on {ChannelName} for the words {string.Join(", ", _watchedSystems)}. Fly save!");
            
            var watcher = new FileSystemWatcher
            {
                Path = FilePath, 
                NotifyFilter =   NotifyFilters.LastWrite | NotifyFilters.LastAccess,
                Filter = ChannelName + "_*.txt"
            };
            
            try
            {
                Console.Out.WriteLine("Listening on " + FilePath);
                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;
                WaitForExit();
            }
            finally
            {
                _intelStreamReader?.Close();
                _intelFileStream?.Close();
            }
        }

        static void OnCreated(object source, FileSystemEventArgs e)
        {
            Console.Out.WriteLine("OnCreated triggered " + e.Name);

            _intelStreamReader?.Close();
            _intelFileStream?.Close();
            
            _intelFileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            _intelStreamReader = new StreamReader(_intelFileStream);
        }
        
        
        static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (_intelFileStream == null)
            {
                _intelFileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_intelStreamReader == null)
            {
                _intelStreamReader = new StreamReader(_intelFileStream);
            }

            if (!e.FullPath.Equals(_intelFileStream.Name))
            {
                return;
            }
            
            while (!_intelStreamReader.EndOfStream)
            {
                var line = _intelStreamReader.ReadLine();
                ProcessLine(line);
            }
        }

        static void ProcessLine(string line)
        {
            foreach (var systemName in _watchedSystems)
            {
                if (line.ToLower().Contains(systemName.ToLower()))
                {
                    Console.Out.WriteLine(line);
                    for (int i = 0; i < BeepDurationInSeconds * 5; i++)
                    {
                        Console.Beep();
                        Thread.Sleep(200);                        
                    }
                }
            }
        }

        static string ConsolePrompt(string prompt)
        {
            var result = "";
            while (string.IsNullOrEmpty(result))
            {
                Console.Out.WriteLine(prompt);
                result = Console.ReadLine();
            }

            Console.Out.WriteLine("");
            return result;
        }

        static void WaitForExit()
        {
            var exit = "";
            while (!exit.Equals("exit") && !exit.Equals("stop"))
            {
                exit = ConsolePrompt("\nEnter 'exit' if you want me to stop").ToLower();
            }
        }
    }
}