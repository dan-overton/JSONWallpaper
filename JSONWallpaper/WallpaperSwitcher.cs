using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JSONWallpaper
{
    public class WallpaperSwitcher : IDisposable
    {
        public class WallpaperRecord
        {
            public Uri url { get; set; }
            public string author { get; set; }
        }

        WallpaperRecord[] records;
        private int recordIndex = 0;
        private System.Timers.Timer timer = null;
        private int _IntervalInMilliseconds;
        private string _JSONFilename;
        public bool IsRunning 
        { 
            get
            {
                return !(timer == null);

            }
        }

        public string JSONFilename { 
            get
            {
                return _JSONFilename;
            }
            set
            {
                if (_JSONFilename != value)
                {
                    recordIndex = 0;
                    _JSONFilename = value;
                    records = JsonConvert.DeserializeObject<WallpaperRecord[]>(File.ReadAllText(_JSONFilename));
                }
            }
        }

        public int IntervalInMinutes { 
            get
            {
                return ((int)_IntervalInMilliseconds / 1000) / 60;
            } 
            set
            {
                _IntervalInMilliseconds = value * 60 * 1000;

                if(timer!= null)
                {
                    Debug.WriteLine(String.Format("Resetting Timer Interval to {0} minutes at {1:t}",value, DateTime.Now));
                    Stop();
                    Start();
                }
            }
        }

        public WallpaperSwitcher()
        {
            LoadSettings();
        }

        public void Start()
        {
            Stop();

            timer = new System.Timers.Timer();
            timer.Interval = _IntervalInMilliseconds;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(Switch);
            timer.Start();
        }

        public void Stop()
        {
            if (timer != null)
            {
                timer.Stop();
                timer.Dispose();
                timer = null;
            }
        }

        public void LoadSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\JSONWallpaper", false);
            if (key == null)
            {
                key = Registry.CurrentUser.CreateSubKey(@"Software\JSONWallpaper");
            }
            
            IntervalInMinutes = int.Parse((string)key.GetValue(@"IntervalInMinutes", "1"));
            JSONFilename = (string)key.GetValue(@"JSONFilename", @"c:\test\backgrounds.json");
            recordIndex = int.Parse((string)key.GetValue(@"RecordIndex", "0"));
            if (bool.Parse((string)key.GetValue(@"Running", "false")))
            {
                Start();
            }
        }

        public void SaveSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\JSONWallpaper", true);
            key.SetValue(@"IntervalInMinutes", IntervalInMinutes.ToString());
            key.SetValue(@"JSONFilename", JSONFilename);
            key.SetValue(@"RecordIndex", recordIndex.ToString());
            key.SetValue(@"Running", IsRunning);
        }

        internal void Switch(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine(String.Format("Switch to index {0} occurred at {1:t}", recordIndex, DateTime.Now));
            Wallpaper.Set(records[recordIndex].url, Wallpaper.Style.Stretched);
            recordIndex++;

            if (recordIndex == records.Count())
            {
                recordIndex = 0;
            }
        }

        public void Dispose()
        {
            SaveSettings();
            Stop();
        }
    }
}
