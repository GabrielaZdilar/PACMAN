using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace OTTER
{
    public partial class IzbornikForma : Form
    {
        public IzbornikForma()
        {
            InitializeComponent();
        }
        
        private void btnUpis_Click(object sender, EventArgs e)
        {
            BGL igra = new BGL();
            igra.frmIzbornik = this;
            igra.Player = textBox1.Text;
            igra.ShowDialog();
        }
    }
}
