using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Igrac
    {
        private string ime;
        public string Ime
        {
            get { return ime; }
            set { ime = value; }
        }
        private int rez;
        public int Rez
        {
            get { return rez; }
            set { rez = value; }
        }
        public Igrac(string im,int rez)
        {
            this.Ime = im;
            this.Rez = rez;
        }
    }
}
