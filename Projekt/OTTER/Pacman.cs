using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    class Pacman:Likovi
    {
        public Pacman(string s,int kx,int ky,int width,int heigth)
            :base(s,kx,ky,width,heigth)
        {
            this.Zivot = 3;

        }
        
        private int zivot;
        public int Zivot
        {
            get { return zivot; }
            set { if (zivot < 0)
                    zivot = 0;
                else
                    zivot = value;
                        }
        }
       
    }

    class Duh:Likovi
    {
        public Duh(string s, int kx, int ky, int width, int heigth)
            : base(s, kx, ky,width,heigth)
        {
           
        }
        
       
    }
   

}
