using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace WindowsFormsGRID
{
    public partial class Form2 : Form
    {
        public string password;
        private int maxattempt = 3;
        private int attempt = 1;



        public Form2()
        {
            InitializeComponent();
            label2.Text = string.Empty;
        }


            public Form2(string password)
        {
            this.password = password;
            InitializeComponent();
            label2.Text = "attempt "+attempt.ToString();
            this.DialogResult = DialogResult.None;
            //this.CancelButton = button2;
            //this.HelpButton = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == password)
            {
                //  password is ok.
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                ++attempt;
                label2.Text = "attempt " + attempt.ToString();
                if (attempt > maxattempt)
                    this.Close();
                textBox1.Clear();
                textBox1.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
            //this.Dispose();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
            else
                if (e.KeyCode == Keys.Escape)
                textBox1.Clear(); ;
        }

        private void Form2_Paint(object sender, PaintEventArgs e)
        {
            //GraphicsPath wantedshape = new GraphicsPath();
            //wantedshape.AddEllipse(0, 0, this.Width*1.2f, this.Height*1.7f);
            //this.Region = new Region(wantedshape);
        }
    }
}
