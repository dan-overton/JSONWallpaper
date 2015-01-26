using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSONWallpaper
{
    public class WallpaperSwitcher : IDisposable
    {
        public class WallpaperRecord
        {
            public Uri url { get; set; }
            public string author { get; set; }
        }

        WallpaperRecord[] records = null;
        private uint nextWallpaperIndex = 0;
        private uint prevWallpaperIndex = 0;
        private System.Timers.Timer timer = null;
        private uint _IntervalInMilliseconds;
        private string _JSONFilename;

        public bool IsRunning 
        { 
            get
            {
                return !(timer == null);
            }
        }

        public string JSONFilename 
        { 
            get
            {
                return _JSONFilename;
            }
            set
            {
                if(_JSONFilename != value) //setting to the same value does nothing.
                {
                    nextWallpaperIndex = 0;
                    _JSONFilename = value;

                    if (_JSONFilename != "")
                    {
                        records = JsonConvert.DeserializeObject<WallpaperRecord[]>(File.ReadAllText(_JSONFilename));
                    }
                }
            }
        }

        public uint IntervalInMinutes { 
            get
            {
                return ((uint)_IntervalInMilliseconds / 1000) / 60;
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
            ChangeToNextWallpaper();
        }

        public void Start()
        {
            if(records == null)
            {
                throw new Exception("Cannot start timer. No JSON records loaded.");
            }

            Stop();

            timer = new System.Timers.Timer();
            timer.Interval = _IntervalInMilliseconds;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(ChangeWallpaperEvent);
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
            
            IntervalInMinutes = uint.Parse((string)key.GetValue(@"IntervalInMinutes", "1"));
            JSONFilename = (string)key.GetValue(@"JSONFilename", Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),"backgrounds.json"));
            nextWallpaperIndex = uint.Parse((string)key.GetValue(@"RecordIndex", "0"));
        }

        public void SaveSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\JSONWallpaper", true);
            key.SetValue(@"IntervalInMinutes", IntervalInMinutes.ToString());
            key.SetValue(@"JSONFilename", JSONFilename);
            key.SetValue(@"RecordIndex", nextWallpaperIndex.ToString());
        }

        private void ChangeWallpaper(uint index)
        {
            bool setWorked = false;
            if(index >= records.Count())
            {
                throw new Exception("Invalid Wallpaper Index Specified: " + index);
            }

            try
            {
                setWorked = Wallpaper.Set(records[index].url, Wallpaper.Style.Stretched);
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Set exceptioned: " + ex.Message);
                setWorked = true; //skip to next one for next attempt.
            }

            if (setWorked == false)
            {
                Debug.WriteLine("Set failed");
            }
            else
            {
                nextWallpaperIndex = index + 1;

                if (nextWallpaperIndex == records.Count())
                {
                    nextWallpaperIndex = 0;
                }

                prevWallpaperIndex = index == 0 ? (uint)records.Count() - 1 : index - 1;
            }
        }

        public void ChangeToNextWallpaper()
        {
            ChangeWallpaper(nextWallpaperIndex);
            Start();
        }

        public void ChangeToPrevWallpaper()
        {
            ChangeWallpaper(prevWallpaperIndex);
            Start();
        }

        internal void ChangeWallpaperEvent(object sender, System.Timers.ElapsedEventArgs e)
        {
            ChangeToNextWallpaper();
        }

        public void Dispose()
        {
            SaveSettings();
            Stop();
        }
    }
}
