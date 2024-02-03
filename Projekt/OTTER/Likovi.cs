using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OTTER
{
    abstract class Likovi:Sprite
    {
        public Likovi(string s, int kx, int ky, int width, int heigth)
           : base(s, kx, ky)
        {
            this.Width = width;
            this.Heigth = heigth;
        }
        public override int X
        {
            get { return x; }
            set
            {
                if (value <= GameOptions.LeftEdge)
                    this.x = GameOptions.RightEdge;
                else if (value >= GameOptions.RightEdge)
                    this.x = GameOptions.LeftEdge;
                else
                    this.x = value;
            }
        }
        public override int Y
        {
            get { return y; }
            set
            {
                if (value <= GameOptions.UpEdge)
                    this.y = GameOptions.DownEdge;
                else if (value >= GameOptions.DownEdge)
                    this.y = GameOptions.UpEdge;
                else
                    this.y = value;
            }
        }
        private bool kretanje;
        public bool Kretanje
        {
            get { return kretanje; }
            set { kretanje = value; }
        }
    }
}
