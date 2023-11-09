using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PictureSorter
{
    public partial class frmProcessing : Form
    {
        public frmProcessing()
        {
            this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            InitializeComponent();
        }

        public void SetText(string text)
        {
            label1.Text = text;
        }

        public void SetCounter(int current, int max)
        {
            lblCounter.Text = $"{current}/{max}";
            int percentage = Math.Min(100, 100 * current / max);
            progressBar1.Value = percentage;
            Application.DoEvents();
        }
    }
}
