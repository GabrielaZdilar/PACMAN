using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace OTTER
{
    public partial class POREDAK : Form
    {
        public Form frmPoredak;
        private SortedList<int, string> pomocna;
        public SortedList<int, string> Pomocna
        {
            get { return pomocna; }
            set { pomocna = value; }
        }
        public POREDAK()
        {
            InitializeComponent();
        }
       
        delegate void DelegatTipaVoidiu(Form frm);
        private void PostaviTekstNaLabeluiu(Form frm)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.frmPoredak.InvokeRequired)
            {
                DelegatTipaVoidiu d = new DelegatTipaVoidiu(PostaviTekstNaLabeluiu);
                this.Invoke(d, new object[] { frm });
            }
            else
            {
                frm.Show();
            }
        }
        private void POREDAK_Load(object sender, EventArgs e)
        {
            string imeD = "poredak.txt";
            richTextBox1.Text="Korisničko ime" + '\t' + "Rezultat"+"\n";
            using (StreamReader sr = File.OpenText(imeD))
            {
                string linija;
                while ((linija = sr.ReadLine()) != null)
                {
                    string[] s = linija.Split('\t');
                    richTextBox1.Text += s[0] + '\t' + '\t' + s[1].ToString()+'\n';
                }
            }
        }
    }
}
