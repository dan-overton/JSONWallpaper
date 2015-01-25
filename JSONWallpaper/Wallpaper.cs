using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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
        static extern UInt32 SystemParametersInfo(UInt32 uiAction, UInt32 uiParam, String pvParam, UInt32 fWinIni);

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        public static void Set(Uri uri, Style style)
        {
            Stream f = null;

            try
            {
                f = new System.Net.WebClient().OpenRead(uri.ToString());
                Set(f, style);
            }
            catch
            {
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        public static void Set(String FileName, Style style)
        {
            FileStream f = null;

            try
            {
                f = new FileStream(FileName, FileMode.Open, FileAccess.Read);
                Set(f, style);
            }
            catch
            {
            }
            finally
            {
                if (f != null)
                {
                    f.Close();
                }
            }
        }

        public static void Set(System.IO.Stream IOStream, Style style)
        {
            System.Drawing.Image img = System.Drawing.Image.FromStream(IOStream);
            string tempPath = Path.Combine(Path.GetTempPath(), "wallpaper.bmp");
            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Bmp);

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

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                tempPath,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }
    }
}
