using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSONWallpaper
{
    class TrayApplicationContext : ApplicationContext
    {
        private System.ComponentModel.IContainer components = null;
        private NotifyIcon theIcon;
        private WallpaperSwitcher ws;

        public TrayApplicationContext(string[] args)
        {
            InitializeComponent(args);
        }

        private void InitializeComponent(string[] args)
        {
            components = new System.ComponentModel.Container();
            theIcon = new NotifyIcon(components)
            {
                ContextMenuStrip = new ContextMenuStrip(),
                Icon = SystemIcons.WinLogo,
                Text = "JSON Wallpaper",
                Visible = true
            };

            theIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            theIcon.DoubleClick += notifyIcon_DoubleClick;

            ws = new WallpaperSwitcher(args);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(ws != null) { ws.Dispose(); }
                if(components != null) { components.Dispose(); }
            }
        }

        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            theIcon.ContextMenuStrip.Items.Clear();

            //build up the context menu
            theIcon.ContextMenuStrip.Items.Add(MakeMenuItem("&Configure", configure_clicked));
            theIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            theIcon.ContextMenuStrip.Items.Add(MakeMenuItem("&Exit", exit_clicked));
        }

        private void configure_clicked(object sender, EventArgs e)
        {
            ws.ShowConfigForm();
        }

        private void exit_clicked(object sender, EventArgs e)
        {
            ExitThread();
        }

        private ToolStripMenuItem MakeMenuItem(string displayText, EventHandler eventHandler)
        {
            ToolStripMenuItem theItem = new ToolStripMenuItem(displayText);
            theItem.Click += eventHandler;
            return theItem;
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ws.ShowConfigForm();
        }

        protected override void ExitThreadCore()
        {
            //This is called by ExitThread()
            if (ws.ConfigForm != null) { ws.ConfigForm.Close(); }
            theIcon.Visible = false;
            base.ExitThreadCore();
        }
    }
}
