using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace csFTPStarter
{
    public partial class DummyWindowBO : Form
    {
        public int rvX=0;
        public int rvY=0;
        private bool bVerify = true;
        public int winNum = 0;
        private MainForm mainForm;
        //public event FormClosingEventHandler DummyWindow_FormClosing;
        
        public DummyWindowBO(MainForm frm1)
        {
            InitializeComponent();
            mainForm = frm1;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DummyWindowBO_FormClosing);
        }

        private void SaveWindowPosition()
        {
            rvX = this.Bounds.X;
            rvY = this.Bounds.Y;
            
            if(winNum==1)
            {
                mainForm.txtPosX0.Text = rvX.ToString();
                mainForm.txtPosY0.Text = rvY.ToString();
            }
            else if(winNum==2)
            {
                mainForm.txtPosX1.Text = rvX.ToString();
                mainForm.txtPosY1.Text = rvY.ToString();
            }
            else if(winNum==3)
            {
                mainForm.txtPosX2.Text = rvX.ToString();
                mainForm.txtPosY2.Text = rvY.ToString();
            }
            else if(winNum==4)
            {
                mainForm.txtPosX3.Text = rvX.ToString();
                mainForm.txtPosY3.Text = rvY.ToString();
            }
            else if(winNum==5)
            {
                mainForm.txtPosX4.Text = rvX.ToString();
                mainForm.txtPosY4.Text = rvY.ToString();
            }
            else
            {
                mainForm.txtPosX5.Text = rvX.ToString();
                mainForm.txtPosY5.Text = rvY.ToString();
            }
        }

        private void DummyWindowBO_Load(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(rvX, rvY);
            lblTableNumber.Text = winNum.ToString();
            this.Text = "Table " + lblTableNumber.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(0, 0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(this.Bounds.X ,0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width-this.Bounds.Width, 0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(0, this.Bounds.Y);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width - this.Bounds.Width, this.Bounds.Y);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(0, Screen.PrimaryScreen.Bounds.Height - this.Bounds.Height);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(this.Bounds.X, Screen.PrimaryScreen.Bounds.Height - this.Bounds.Height);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.Location = new System.Drawing.Point(Screen.PrimaryScreen.Bounds.Width - this.Bounds.Width, Screen.PrimaryScreen.Bounds.Height - this.Bounds.Height);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bVerify = false;
            SaveWindowPosition();
            this.Close();
        }

        private void DummyWindowBO_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            bVerify = false;
            this.Close();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            bVerify = false;
            SaveWindowPosition();
            this.Close();
        }

    }
}
