using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSONWallpaper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //required to allow run on install with VS installer. Need to switch to a better installer!
            if (args.Length == 1 && args[0] == "INSTALLER") { 
                Process.Start(Application.ExecutablePath); 
                return; 
            }
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApplicationContext(args));
        }
    }
}
