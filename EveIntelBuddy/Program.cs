using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace EveIntelBuddy
{
    class Program
    {
        private static string ChannelName = "Bean-Intel";
        private static List<string> _watchedSystems = new List<string>() {"6V-D0E", "LS3-HP", "QX-4HO", "BVRQ-O"};
        private const int BeepDurationInSeconds = 1;
        
        private static FileStream? _intelFileStream;
        private static StreamReader? _intelStreamReader;
        
        static void Main(string[] args)
        {
            Settings.LoadOrInitialize();
            Reconfigure();
            
            var watcher = new FileSystemWatcher
            {
                Path = Settings.ChatLogFolder, 
                NotifyFilter =   NotifyFilters.LastWrite | NotifyFilters.LastAccess,
                Filter = ChannelName + "_*.txt"
            };
            
            try
            {
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

        static void Reconfigure()
        {
            var channel = ConsolePrompt("Which In-Game Chat Channel should I watch?");
            ChannelName = channel.Equals("default") ? "Bean-Intel" : channel;
            
            var systems = ConsolePrompt("Which systems (or words) should raise an alert?");
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
                    Beep();
                }
            }
        }

        static void Beep()
        {
            for (int i = 0; i < BeepDurationInSeconds * 5; i++)
            {
                Console.Beep();
                Thread.Sleep(200);                        
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

        private static readonly string[] ExitStrings = new[] {"exit", "stop"};
        private static readonly string[] SetupStrings = new[] {"setup", "reconfigure"};
        
        static void WaitForExit()
        {
            var input = "";
            while (!input.Equals("exit") && !input.Equals("stop"))
            {
                input = ConsolePrompt("\nEnter 'exit' if you want me to stop, 'setup' to reconfigure and 'test' to play a test sound.").ToLower();

                if (ExitStrings.Any(x => x.Equals(input)))
                {
                    return;
                }
                
                if (SetupStrings.Any(x => x.Equals(input)))
                {
                    Reconfigure();
                }

                if (input.Equals("test"))
                {
                    Beep();
                }
            }
        }
    }
}