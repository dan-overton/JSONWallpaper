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
        
        WallpaperSwitcher ws;
        
        ConfigForm configForm = null;

        public TrayApplicationContext()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
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

            ws = new WallpaperSwitcher();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(components != null) { components.Dispose(); }
                if(configForm != null) { configForm.Dispose(); }
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
            ShowConfigForm();
        }

        void ShowConfigForm()
        {
            if (configForm == null)
            {
                configForm = new ConfigForm(ws);
                configForm.Closed += configForm_close;
                configForm.Show();
            }
            else
            {
                configForm.Activate();
            }
        }

        private void configForm_close(object sender, EventArgs e)
        {
            configForm = null;
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
            ShowConfigForm();
        }
    }
}
