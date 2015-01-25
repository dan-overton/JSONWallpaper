using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JSONWallpaper
{
    public partial class ConfigForm : Form
    {
        WallpaperSwitcher switcher;
        public ConfigForm(WallpaperSwitcher ws) : this()
        {
            switcher = ws;
            numInterval.Value = switcher.IntervalInMinutes;
            txtJSONFile.Text = switcher.JSONFilename;
        }

        public ConfigForm()
        {
            InitializeComponent();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            switcher.IntervalInMinutes = (uint)numInterval.Value;
            switcher.JSONFilename = txtJSONFile.Text;
            switcher.SaveSettings();
            this.Close();
        }

        private void cmdSelectJSONFile_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "JSON files (*.json)|*.json";
            openFileDialog1.Title = "Choose JSON File";
            openFileDialog1.CheckFileExists = true;

            openFileDialog1.ShowDialog(this);
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            txtJSONFile.Text = openFileDialog1.FileName;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            switcher.IntervalInMinutes = (uint)numInterval.Value;
            switcher.JSONFilename = txtJSONFile.Text;
            switcher.SaveSettings();
        }

        private void btnPrev_Click(object sender, EventArgs e)
        {
            btnPrev.Enabled = false;
            btnNext.Enabled = false;
            switcher.ChangeToPrevWallpaper();
            btnPrev.Enabled = true;
            btnNext.Enabled = true;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            btnPrev.Enabled = false;
            btnNext.Enabled = false;
            switcher.ChangeToNextWallpaper();
            btnPrev.Enabled = true;
            btnNext.Enabled = true;
        }
    }
}
