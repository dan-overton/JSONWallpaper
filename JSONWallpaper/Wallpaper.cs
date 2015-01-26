using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JSONWallpaper
{
    //http://stackoverflow.com/questions/1061678/change-desktop-wallpaper-using-code-in-net

    public sealed class Wallpaper
    {
        Wallpaper() { }

        const UInt32 SPI_SETDESKWALLPAPER = 20;
        const UInt32 SPIF_UPDATEINIFILE = 0x01;
        const UInt32 SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static bool Set(Uri uri, Style style)
        {
            using(Stream f = new System.Net.WebClient().OpenRead(uri.ToString()))
            {
                return Set(f, style);
            }
        }

        public static bool Set(String FileName, Style style)
        {
             using (FileStream f = new FileStream(FileName, FileMode.Open, FileAccess.Read))
             {
                 return Set(f, style);
             }
        }

        public static bool Set(System.IO.Stream IOStream, Style style)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            int attempts = 10;
            bool successful = false;


            using(System.Drawing.Image img = System.Drawing.Image.FromStream(IOStream))
            {
                while (successful == false && attempts > 0)
                {
                    try
                    {
                        img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);
                        successful = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Img save failed " + ex.Message);
                        attempts = attempts - 1;
                        Thread.Sleep(1000);
                    }
                }
            }

            if(successful == false)
            {
                return false;
            }

            successful = false;
            attempts = 10;

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            //SystemParametersInfo fails for no apparent reason several times.
            while(successful == false && attempts > 0)
            {
                try
                {
                    successful = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, tempPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Set failed " + ex.Message);
                    attempts = attempts - 1;
                    Thread.Sleep(1000);
                }
            }

            return successful;
        }
    }
}
