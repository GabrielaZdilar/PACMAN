using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    abstract class Objekti:Sprite
    {
        public Objekti(string s, int kx, int ky, int width, int heigth)
           : base(s, kx, ky)
        {

            this.Width = width;
            this.Heigth = heigth;
        }
    }
    class Zid : Objekti
    {
        public Zid(string s, int kx, int ky, int width, int heigth)
           : base(s, kx, ky,width,heigth)
        {
            
        }
    }
    class Novcic : Objekti
    {
        public Novcic(string s, int kx, int ky, int width, int heigth)
           : base(s, kx, ky,width,heigth)
        {
            
        }
        private bool vidljiv;
        public bool Vidljiv
        {
            get { return vidljiv; }
            set { vidljiv = value; }
        }
    }
}
