using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using Console = Colorful.Console;

namespace UpdateDecoyFiles
{
    class Program
    {
        static readonly string[] decoyFiles = { "PhoneNumbers.txt", "Addresses.txt" };
        static readonly string targetDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static FileSystemWatcher fsw;

        static void Main(string[] args)
        {
            fsw = new FileSystemWatcher(targetDir);

            fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            fsw.Changed += new FileSystemEventHandler(OnChanged);
            fsw.Created += new FileSystemEventHandler(OnChanged);
            fsw.Deleted += new FileSystemEventHandler(OnChanged);
            fsw.Renamed += new RenamedEventHandler(OnRenamed);
            fsw.Error += new ErrorEventHandler(OnError);

            fsw.EnableRaisingEvents = true;

            Console.WriteLine("Decoy Updater v1.0\n");
            Console.Write("Status: ");
            Console.WriteLine("ACTIVE", Color.LightGreen);
            Console.WriteLine("Target Directory: {0}\n", targetDir);

            Console.WriteLine("Press \'Enter\' to stop protection and exit.\n");
            Console.ReadLine();
        }

        //  This method is called when a file is created, changed, or deleted.
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            bool isDecoy = false;

            foreach(var fileName in decoyFiles)
            {
                if(fileName == e.Name)
                {
                    isDecoy = true;
                    break;
                }
            }

            WatcherChangeTypes wct = e.ChangeType;
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.Write("{0} ", time);

            if (isDecoy)
            {
                Console.Write("DECOY", Color.Red);
                Console.WriteLine(" {0}", e.FullPath);
            }
            else
            {
                Console.Write(wct.ToString().ToUpper(), Color.Cyan);

                int index;

                using (var rng = new RNGCryptoServiceProvider())
                {
                    byte[] data = new byte[4];
                    UInt32 value;

                    do
                    {
                        rng.GetBytes(data);
                        value = BitConverter.ToUInt32(data, 0);
                    } while (value == 0);

                    index = (int)(value % decoyFiles.Length);
                }

                Console.WriteLine(" {0}", e.FullPath);

                fsw.EnableRaisingEvents = false;
                UpdateDecoy(index);
                time = DateTime.Now.ToString("HH:mm:ss.fff");
                Console.Write("{0} ", time);
                Console.Write("UPDATED ", Color.LightGreen);
                Console.WriteLine("Decoy file: {0}.", decoyFiles[index]);
                fsw.EnableRaisingEvents = true;
            }
        }

        private static void UpdateDecoy(int index)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[4];
                UInt32 value;

                do
                {
                    rng.GetBytes(data);
                    value = BitConverter.ToUInt32(data, 0);
                } while (value == 0);

                int timeToSleep = (int)(value % 5);
                System.Threading.Thread.Sleep(timeToSleep * 1000);
            }
            
            string targetFile = Path.Combine(targetDir, decoyFiles[index]);

            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] data = new byte[8];
                rng.GetBytes(data);

                using (Stream stream = File.Open(targetFile, FileMode.Open))
                {
                    stream.Position = 0;
                    stream.Write(data, 0, data.Length);
                }
            }
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            bool isDecoy = false;

            foreach (var fileName in decoyFiles)
            {
                if (fileName == e.OldName)
                {
                    isDecoy = true;
                    break;
                }
            }

            WatcherChangeTypes wct = e.ChangeType;
            string time = DateTime.Now.ToString("HH:mm:ss.fff");
            Console.Write("{0} ", time);

            if (isDecoy)
            {
                Console.Write("DECOY ", Color.Red);
            }
            
            Console.Write(wct.ToString().ToUpper(), Color.Cyan);
            Console.Write(" FROM ", Color.Yellow);
            Console.Write("{0}", e.OldFullPath);
            Console.Write(" TO ", e.FullPath, Color.Yellow);
            Console.WriteLine("{0}", e.FullPath);

        }

        //  This method is called when the FileSystemWatcher detects an error.
        private static void OnError(object source, ErrorEventArgs e)
        {
            //  Show that an error has been detected.
            Console.WriteLine("The FileSystemWatcher has detected an error");
            //  Give more information if the error is due to an internal buffer overflow.
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if Windows is reporting many file system events quickly 
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                Console.WriteLine(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
            }
        }
    }
}
