using System;
using System.IO;
using System.Threading;

namespace EveIntelBuddy
{
    class Program
    {
        private const string FilePath = "/ssdapps/eve-online/drive_c/users/jacudibu/My Documents/EVE/logs/Chatlogs/";
        private const string ChannelName = "Test123Yay";
        
        private static FileStream? _intelFileStream;
        private static StreamReader? _intelStreamReader;
        
        static void Main(string[] args)
        {
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
                while (true)
                {
                    Thread.Sleep(1000);
                }                
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
            Console.Out.WriteLine("OnChanged triggered " + e.Name);
            if (_intelFileStream == null)
            {
                _intelFileStream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            }

            if (_intelStreamReader == null)
            {
                _intelStreamReader = new StreamReader(_intelFileStream);
            }
            
            while (!_intelStreamReader.EndOfStream)
            {
                var data = _intelStreamReader.ReadLine();
                Console.Out.WriteLine(data);
            }
        }
    }
}