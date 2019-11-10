using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace EveIntelBuddy
{
    class Program
    {
        private static Preset _currentPreset;
        private const int BeepDurationInSeconds = 1;
        
        private static FileStream? _intelFileStream;
        private static StreamReader? _intelStreamReader;
        
        static void Main(string[] args)
        {
            Settings.LoadOrInitialize();
            InitializePreset();
            
            var watcher = new FileSystemWatcher
            {
                Path = Settings.ChatLogFolder, 
                NotifyFilter =   NotifyFilters.LastWrite | NotifyFilters.LastAccess,
                Filter = _currentPreset.Channel + "_*.txt"
            };
            
            try
            {
                watcher.Changed += OnChanged;
                watcher.Created += OnCreated;
                watcher.EnableRaisingEvents = true;
                WaitForUserInput();
            }
            finally
            {
                _intelStreamReader?.Close();
                _intelFileStream?.Close();
            }
        }
        
        static void InitializePreset()
        {
            _currentPreset = Preset.LoadExistingPreset() ?? Preset.CreateNewPreset();
            Console.WriteLine($"Okay, I'll keep an eye on {_currentPreset.Channel} for the words {string.Join(", ", _currentPreset.Words)}. Fly save!");
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
            foreach (var word in _currentPreset.Words)
            {
                if (line.ToLower().Contains(word.ToLower()))
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
        
        private static readonly string[] ExitStrings = {"exit", "stop"};
        private static readonly string[] SetupStrings = {"setup", "reconfigure"};
        
        static void WaitForUserInput()
        {
            var input = "";
            while (!input.Equals("exit") && !input.Equals("stop"))
            {
                input = Utility.ConsolePrompt("\nEnter 'exit' if you want me to stop, " +
                                      "'setup' to reconfigure, " +
                                      "'test' to play a test sound and " +
                                      "'save' in order to save your current settings as a preset.").ToLower();

                if (ExitStrings.Any(x => x.Equals(input)))
                {
                    return;
                }
                
                if (SetupStrings.Any(x => x.Equals(input)))
                {
                    InitializePreset();
                }

                if (input.Equals("save"))
                {
                    var name = Utility.ConsolePrompt("Please enter a name for the preset.");
                    _currentPreset.Save(name);
                }

                if (input.Equals("test"))
                {
                    Beep();
                }
            }
        }
    }
}