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
    public partial class Form3 : Form
    {
        public string Password { get; private set; }
        private  char passChar ;
        //private int maxattempt = 3;
        //private int attempt = 1;

        public Form3()
        {
            InitializeComponent();
            this.DialogResult = DialogResult.None;
            Password = string.Empty;
            passChar = textBox1.PasswordChar;
            textBox1.PasswordChar = '\0';
            checkBox1.Checked = true;
            //label2.Text = string.Empty;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
            else
                if (e.KeyCode == Keys.Escape)
                textBox1.Clear(); 
        }

        private void Form3_Paint(object sender, PaintEventArgs e)
        {
            //GraphicsPath wantedshape = new GraphicsPath();
            //wantedshape.AddEllipse(0, 0, this.Width*1.2f, this.Height*1.7f);
            //this.Region = new Region(wantedshape);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Password = textBox1.Text;
            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            textBox1.PasswordChar = checkBox1.Checked ? '\0' : passChar;
        }
    }
}
