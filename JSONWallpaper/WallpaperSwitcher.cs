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
        private class WallpaperRecord
        {
            public Uri url { get; set; }
            public string author { get; set; }
        }

        WallpaperRecord[] records = null;
        private int nextWallpaperIndex = 0;
        private int prevWallpaperIndex = 0;
        private System.Timers.Timer timer = null;

        public bool IsRunning { get { return !(timer == null); } }
        public string JSONFilename { get; private set; }
        public uint IntervalInMinutes { get; private set; }
        public bool RunsAtStartUp { get; private set; }

        public WallpaperSwitcher()
        {
            LoadSettings();
            try
            {
                Start(IntervalInMinutes, JSONFilename, RunsAtStartUp);
            }
            catch(Exception ex)
            {
                Debug.Write("Could not start: " + ex.Message);
            }
        }

        public void Start(uint intervalInMinutes, string jsonFilename, bool runAtStartup)
        {
            if (IsRunning)
            {
                Stop();
            }

            records = JsonConvert.DeserializeObject<WallpaperRecord[]>(File.ReadAllText(jsonFilename));

            if (records.Count() == 0)
            {
                records = null;
                throw new Exception("No records loaded");
            }

            if (JSONFilename != jsonFilename) //don't reset index if same file loaded already.
            {
                nextWallpaperIndex = 0;
                prevWallpaperIndex = (records.Count() - 1);
            }
            else
            {
                prevWallpaperIndex = nextWallpaperIndex - 2;

                if(prevWallpaperIndex < 0)
                {
                    prevWallpaperIndex = records.Count() - prevWallpaperIndex;
                }
            }

            timer = new System.Timers.Timer();
            timer.Interval = (intervalInMinutes * 60 * 1000);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(ChangeWallpaperEvent);
            timer.Start();

            IntervalInMinutes = intervalInMinutes;
            JSONFilename = jsonFilename;
            RunsAtStartUp = runAtStartup;

            SaveSettings();
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
            nextWallpaperIndex = int.Parse((string)key.GetValue(@"NextRecordIndex", "0"));

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            RunsAtStartUp = ((string)startupKey.GetValue("JSONWallpaper", "notfound") != "notfound");
        }

        public void SaveSettings()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\JSONWallpaper", true);
            key.SetValue("IntervalInMinutes", IntervalInMinutes.ToString());
            key.SetValue("JSONFilename", JSONFilename);
            key.SetValue("NextRecordIndex", nextWallpaperIndex.ToString());

            RegistryKey startupKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);

            if(RunsAtStartUp)
            {   
                startupKey.SetValue("JSONWallpaper", Application.ExecutablePath);
            }
            else
            {
                if ((string)startupKey.GetValue("JSONWallpaper", "notfound") != "notfound")
                {
                    startupKey.DeleteValue("JSONWallpaper");
                }
            }
        }

        private void ChangeWallpaper(int index)
        {
            bool setWorked = false;
            if (records != null)
            {
                if (index >= records.Count())
                {
                    throw new Exception("Invalid Wallpaper Index Specified: " + index);
                }

                try
                {
                    setWorked = Wallpaper.Set(records[index].url, Wallpaper.Style.Stretched);
                }
                catch (Exception ex)
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

                    prevWallpaperIndex = index == 0 ? records.Count() - 1 : index - 1;
                }
            }
        }

        public void ChangeToNextWallpaper()
        {
            ChangeWallpaper(nextWallpaperIndex);
        }

        public void ChangeToPrevWallpaper()
        {
            ChangeWallpaper(prevWallpaperIndex);
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
