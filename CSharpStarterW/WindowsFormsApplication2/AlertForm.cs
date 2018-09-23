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
    public partial class AlertForm : Form
    {
        #region PROPERTIES
        public string Message
        {
            set { labelMessage.Text = value; }
        }

        public int ProgressValue
        {
            set { progressBar1.Value = value; }
        }
        #endregion

        #region METHODS
        public AlertForm()
        {
            InitializeComponent();
        }
        #endregion
    }
}
