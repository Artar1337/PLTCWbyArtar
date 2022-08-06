using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PLTCWbyArtar
{
    public partial class Form3 : Form
    {
        public Form3(Image img = null)
        {
            InitializeComponent();
            pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            panel1.AutoScroll = true;
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | 
                AnchorStyles.Left | AnchorStyles.Right;
            if (img != null)
                pictureBox.Image = img;
            this.WindowState = FormWindowState.Maximized;
        }

        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
