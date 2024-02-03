using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OTTER
{
    /// <summary>
    /// -
    /// </summary>
    public partial class BGL : Form
    {
        public Form frmIzbornik;
        private string player;
        public string Player
        {
            get { return player; }
            set
            {
                if (value == "")
                    player = "Nepoznat";
                else
                    player = value;
            }
        }
        /* ------------------- */
        #region Environment Variables

        List<Func<int>> GreenFlagScripts = new List<Func<int>>();

        /// <summary>
        /// Uvjet izvršavanja igre. Ako je <c>START == true</c> igra će se izvršavati.
        /// </summary>
        /// <example><c>START</c> se često koristi za beskonačnu petlju. Primjer metode/skripte:
        /// <code>
        /// private int MojaMetoda()
        /// {
        ///     while(START)
        ///     {
        ///       //ovdje ide kod
        ///     }
        ///     return 0;
        /// }</code>
        /// </example>
        public static bool START = true;

        //sprites
        /// <summary>
        /// Broj likova.
        /// </summary>
        public static int spriteCount = 0, soundCount = 0;

        /// <summary>
        /// Lista svih likova.
        /// </summary>
        //public static List<Sprite> allSprites = new List<Sprite>();
        public static SpriteList<Sprite> allSprites = new SpriteList<Sprite>();

        //sensing
        int mouseX, mouseY;
        Sensing sensing = new Sensing();

        //background
        List<string> backgroundImages = new List<string>();
        int backgroundImageIndex = 0;
        string ISPIS = "";

        SoundPlayer[] sounds = new SoundPlayer[1000];
        TextReader[] readFiles = new StreamReader[1000];
        TextWriter[] writeFiles = new StreamWriter[1000];
        bool showSync = false;
        int loopcount;
        DateTime dt = new DateTime();
        String time;
        double lastTime, thisTime, diff;

        #endregion
        /* ------------------- */
        #region Events
        
        private void Draw(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            try
            {                
                foreach (Sprite sprite in allSprites)
                {                    
                    if (sprite != null)
                        if (sprite.Show == true)
                        {
                            g.DrawImage(sprite.CurrentCostume, new Rectangle(sprite.X, sprite.Y, sprite.Width, sprite.Heigth));
                        }
                    if (allSprites.Change)
                        break;
                }
                if (allSprites.Change)
                    allSprites.Change = false;
            }
            catch
            {
                //ako se doda sprite dok crta onda se mijenja allSprites
                MessageBox.Show("Greška!");
            }
        }

        private void startTimer(object sender, EventArgs e)
        {
            this.frmIzbornik.Hide();
            timer1.Start();
            timer2.Start();
            Init();
        }

        private void updateFrameRate(object sender, EventArgs e)
        {
            updateSyncRate();
        }

        /// <summary>
        /// Crta tekst po pozornici.
        /// </summary>
        /// <param name="sender">-</param>
        /// <param name="e">-</param>
        public void DrawTextOnScreen(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var brush = new SolidBrush(Color.WhiteSmoke);
            string text = ISPIS;

            SizeF stringSize = new SizeF();
            Font stringFont = new Font("Arial", 14);
            stringSize = e.Graphics.MeasureString(text, stringFont);

            using (Font font1 = stringFont)
            {
                RectangleF rectF1 = new RectangleF(0, 0, stringSize.Width, stringSize.Height);
                e.Graphics.FillRectangle(brush, Rectangle.Round(rectF1));
                e.Graphics.DrawString(text, font1, Brushes.Black, rectF1);
            }
        }

        private void mouseClicked(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;
        }

        private void mouseDown(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = true;
            sensing.MouseDown = true;            
        }

        private void mouseUp(object sender, MouseEventArgs e)
        {
            //sensing.MouseDown = false;
            sensing.MouseDown = false;
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            mouseX = e.X;
            mouseY = e.Y;

            //sensing.MouseX = e.X;
            //sensing.MouseY = e.Y;
            //Sensing.Mouse.x = e.X;
            //Sensing.Mouse.y = e.Y;
            sensing.Mouse.X = e.X;
            sensing.Mouse.Y = e.Y;

        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            sensing.Key = e.KeyCode.ToString();
            sensing.KeyPressedTest = true;
        }

        private void keyUp(object sender, KeyEventArgs e)
        {
            sensing.Key = "";
            sensing.KeyPressedTest = false;
        }

        private void Update(object sender, EventArgs e)
        {
            if (sensing.KeyPressed(Keys.Escape))
            {
                START = false;
            }

            if (START)
            {
                this.Refresh();
            }
        }

        #endregion
        /* ------------------- */
        #region Start of Game Methods

        //my
        #region my

        //private void StartScriptAndWait(Func<int> scriptName)
        //{
        //    Task t = Task.Factory.StartNew(scriptName);
        //    t.Wait();
        //}

        //private void StartScript(Func<int> scriptName)
        //{
        //    Task t;
        //    t = Task.Factory.StartNew(scriptName);
        //}

        private int AnimateBackground(int intervalMS)
        {
            while (START)
            {
                setBackgroundPicture(backgroundImages[backgroundImageIndex]);
                Game.WaitMS(intervalMS);
                backgroundImageIndex++;
                if (backgroundImageIndex == 3)
                    backgroundImageIndex = 0;
            }
            return 0;
        }

        private void KlikNaZastavicu()
        {
            foreach (Func<int> f in GreenFlagScripts)
            {
                Task.Factory.StartNew(f);
            }
        }

        #endregion

        /// <summary>
        /// BGL
        /// </summary>
        public BGL()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Pričekaj (pauza) u sekundama.
        /// </summary>
        /// <example>Pričekaj pola sekunde: <code>Wait(0.5);</code></example>
        /// <param name="sekunde">Realan broj.</param>
        public void Wait(double sekunde)
        {
            int ms = (int)(sekunde * 1000);
            Thread.Sleep(ms);
        }

        //private int SlucajanBroj(int min, int max)
        //{
        //    Random r = new Random();
        //    int br = r.Next(min, max + 1);
        //    return br;
        //}

        /// <summary>
        /// -
        /// </summary>
        public void Init()
        {
            if (dt == null) time = dt.TimeOfDay.ToString();
            loopcount++;
            //Load resources and level here
            this.Paint += new PaintEventHandler(DrawTextOnScreen);
            SetupGame();
        }

        /// <summary>
        /// -
        /// </summary>
        /// <param name="val">-</param>
        public void showSyncRate(bool val)
        {
            showSync = val;
            if (val == true) syncRate.Show();
            if (val == false) syncRate.Hide();
        }

        /// <summary>
        /// -
        /// </summary>
        public void updateSyncRate()
        {
            if (showSync == true)
            {
                thisTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                diff = thisTime - lastTime;
                lastTime = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;

                double fr = (1000 / diff) / 1000;

                int fr2 = Convert.ToInt32(fr);

                syncRate.Text = fr2.ToString();
            }

        }

        //stage
        #region Stage

        /// <summary>
        /// Postavi naslov pozornice.
        /// </summary>
        /// <param name="title">tekst koji će se ispisati na vrhu (naslovnoj traci).</param>
        public void SetStageTitle(string title)
        {
            this.Text = title;
        }

        /// <summary>
        /// Postavi boju pozadine.
        /// </summary>
        /// <param name="r">r</param>
        /// <param name="g">g</param>
        /// <param name="b">b</param>
        public void setBackgroundColor(int r, int g, int b)
        {
            this.BackColor = Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// Postavi boju pozornice. <c>Color</c> je ugrađeni tip.
        /// </summary>
        /// <param name="color"></param>
        public void setBackgroundColor(Color color)
        {
            this.BackColor = color;
        }

        /// <summary>
        /// Postavi sliku pozornice.
        /// </summary>
        /// <param name="backgroundImage">Naziv (putanja) slike.</param>
        public void setBackgroundPicture(string backgroundImage)
        {
            this.BackgroundImage = new Bitmap(backgroundImage);
        }

        /// <summary>
        /// Izgled slike.
        /// </summary>
        /// <param name="layout">none, tile, stretch, center, zoom</param>
        public void setPictureLayout(string layout)
        {
            if (layout.ToLower() == "none") this.BackgroundImageLayout = ImageLayout.None;
            if (layout.ToLower() == "tile") this.BackgroundImageLayout = ImageLayout.Tile;
            if (layout.ToLower() == "stretch") this.BackgroundImageLayout = ImageLayout.Stretch;
            if (layout.ToLower() == "center") this.BackgroundImageLayout = ImageLayout.Center;
            if (layout.ToLower() == "zoom") this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        #endregion

        //sound
        #region sound methods

        /// <summary>
        /// Učitaj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        /// <param name="file">-</param>
        public void loadSound(int soundNum, string file)
        {
            soundCount++;
            sounds[soundNum] = new SoundPlayer(file);
        }

        /// <summary>
        /// Sviraj zvuk.
        /// </summary>
        /// <param name="soundNum">-</param>
        public void playSound(int soundNum)
        {
            sounds[soundNum].Play();
        }

        /// <summary>
        /// loopSound
        /// </summary>
        /// <param name="soundNum">-</param>
        public void loopSound(int soundNum)
        {
            sounds[soundNum].PlayLooping();
        }

        /// <summary>
        /// Zaustavi zvuk.
        /// </summary>
        /// <param name="soundNum">broj</param>
        public void stopSound(int soundNum)
        {
            sounds[soundNum].Stop();
        }

        #endregion

        //file
        #region file methods

        /// <summary>
        /// Otvori datoteku za čitanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToRead(string fileName, int fileNum)
        {
            readFiles[fileNum] = new StreamReader(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToRead(int fileNum)
        {
            readFiles[fileNum].Close();
        }

        /// <summary>
        /// Otvori datoteku za pisanje.
        /// </summary>
        /// <param name="fileName">naziv datoteke</param>
        /// <param name="fileNum">broj</param>
        public void openFileToWrite(string fileName, int fileNum)
        {
            writeFiles[fileNum] = new StreamWriter(fileName);
        }

        /// <summary>
        /// Zatvori datoteku.
        /// </summary>
        /// <param name="fileNum">broj</param>
        public void closeFileToWrite(int fileNum)
        {
            writeFiles[fileNum].Close();
        }

        /// <summary>
        /// Zapiši liniju u datoteku.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <param name="line">linija</param>
        public void writeLine(int fileNum, string line)
        {
            writeFiles[fileNum].WriteLine(line);
        }

        /// <summary>
        /// Pročitaj liniju iz datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća pročitanu liniju</returns>
        public string readLine(int fileNum)
        {
            return readFiles[fileNum].ReadLine();
        }

        /// <summary>
        /// Čita sadržaj datoteke.
        /// </summary>
        /// <param name="fileNum">broj datoteke</param>
        /// <returns>vraća sadržaj</returns>
        public string readFile(int fileNum)
        {
            return readFiles[fileNum].ReadToEnd();
        }

        #endregion

        //mouse & keys
        #region mouse methods

        /// <summary>
        /// Sakrij strelicu miša.
        /// </summary>
        public void hideMouse()
        {
            Cursor.Hide();
        }

        /// <summary>
        /// Pokaži strelicu miša.
        /// </summary>
        public void showMouse()
        {
            Cursor.Show();
        }

        /// <summary>
        /// Provjerava je li miš pritisnut.
        /// </summary>
        /// <returns>true/false</returns>
        public bool isMousePressed()
        {
            //return sensing.MouseDown;
            return sensing.MouseDown;
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">naziv tipke</param>
        /// <returns></returns>
        public bool isKeyPressed(string key)
        {
            if (sensing.Key == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Provjerava je li tipka pritisnuta.
        /// </summary>
        /// <param name="key">tipka</param>
        /// <returns>true/false</returns>
        public bool isKeyPressed(Keys key)
        {
            if (sensing.Key == key.ToString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #endregion
        /* ------------------- */

        /* ------------ GAME CODE START ------------ */

        /* Game variables */
        

        /* Initialization */
        Pacman pacman;
        Duh prvi, drugi, treci, cetvrti;
        Zid z1, z2, z3, z4;
        public bool kretanje;
        List<Novcic> ln;
        

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        delegate void DelegatTipaVoidi(Form frm);
        private void PostaviTekstNaLabelui(Form frm)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.frmIzbornik.InvokeRequired)
            {
                DelegatTipaVoidi d = new DelegatTipaVoidi(PostaviTekstNaLabelui);
                this.Invoke(d, new object[] { frm });
            }
            else
            {
                frm.Show();
            }
        }

        private void BGL_FormClosed(object sender, FormClosedEventArgs e)
        {
            PostaviTekstNaLabelui(frmIzbornik);
        }

        int rezultat;
        
        private void SetupGame()
        {
            //1. setup stage
            SetStageTitle(this.Player);
            setBackgroundColor(Color.Black);            
            //setBackgroundPicture("backgrounds\\back.jpg");
            //none, tile, stretch, center, zoom
            setPictureLayout("stretch");

            //2. add sprites
            z1 = new Zid("sprites\\plavo.png", 170, 0, 75, 300);
            z1.SetTransparentColor(Color.White);
            z1.SetSize(50);
            Game.AddSprite(z1);

            z2 = new Zid("sprites\\plavo.png", 400, 0, 75, 300);
            z2.SetTransparentColor(Color.White);
            z2.SetSize(50);
            Game.AddSprite(z2);

            z3 = new Zid("sprites\\plavo.png", 250, 350, 75, 300);
            z3.SetTransparentColor(Color.White);
            z3.SetSize(50);
            Game.AddSprite(z3);

            z4 = new Zid("sprites\\plavo.png", 480, 350, 75, 300);
            z4.SetTransparentColor(Color.White);
            z4.SetSize(50);
            Game.AddSprite(z4);

           

            ln = new List<Novcic>();
            Novcic k1 = new Novcic("sprites\\nov.png", 0, 0, 35, 35);
            k1.SetTransparentColor(Color.White);
            k1.SetSize(50);
            Game.AddSprite(k1);
            ln.Add(k1);
            Novcic k2 = new Novcic("sprites\\nov.png", 30, 0, 35, 35);
            k2.SetTransparentColor(Color.White);
            k2.SetSize(50);
            Game.AddSprite(k2);
            ln.Add(k2);
            Novcic k3 = new Novcic("sprites\\nov.png", 60, 0, 35, 35);
            k3.SetTransparentColor(Color.White);
            k3.SetSize(50);
            Game.AddSprite(k3);
            ln.Add(k3);
            Novcic k4 = new Novcic("sprites\\nov.png", 90, 0, 35, 35);
            k4.SetTransparentColor(Color.White);
            k4.SetSize(50);
            Game.AddSprite(k4);
            ln.Add(k4);
            Novcic k5 = new Novcic("sprites\\nov.png", 120, 0, 35, 35);
            k5.SetTransparentColor(Color.White);
            k5.SetSize(50);
            Game.AddSprite(k5);
            ln.Add(k5);
            Novcic k6 = new Novcic("sprites\\nov.png", 150, 0, 35, 35);
            k6.SetTransparentColor(Color.White);
            k6.SetSize(50);
            Game.AddSprite(k6);
            ln.Add(k6);
            Novcic k7 = new Novcic("sprites\\nov.png", 0, 30, 35, 35);
            k7.SetTransparentColor(Color.White);
            k7.SetSize(50);
            Game.AddSprite(k7);
            ln.Add(k7);
            Novcic k8 = new Novcic("sprites\\nov.png", 30, 30, 35, 35);
            k8.SetTransparentColor(Color.White);
            k8.SetSize(50);
            Game.AddSprite(k8);
            ln.Add(k8);
            Novcic k9 = new Novcic("sprites\\nov.png", 60, 30, 35, 35);
            k9.SetTransparentColor(Color.White);
            k9.SetSize(50);
            Game.AddSprite(k9);
            ln.Add(k9);
            Novcic k10 = new Novcic("sprites\\nov.png", 90, 30, 35, 35);
            k10.SetTransparentColor(Color.White);
            k10.SetSize(50);
            Game.AddSprite(k10);
            ln.Add(k10);
            Novcic k11 = new Novcic("sprites\\nov.png", 120, 30, 35, 35);
            k11.SetTransparentColor(Color.White);
            k11.SetSize(50);
            Game.AddSprite(k11);
            ln.Add(k11);
            Novcic k12 = new Novcic("sprites\\nov.png", 150, 30, 35, 35);
            k12.SetTransparentColor(Color.White);
            k12.SetSize(50);
            Game.AddSprite(k12);
            ln.Add(k12);

            Novcic k13 = new Novcic("sprites\\nov.png", 0, 60, 35, 35);
            k13.SetTransparentColor(Color.White);
            k13.SetSize(50);
            Game.AddSprite(k13);
            ln.Add(k13);
            Novcic k14 = new Novcic("sprites\\nov.png", 30, 60, 35, 35);
            k14.SetTransparentColor(Color.White);
            k14.SetSize(50);
            Game.AddSprite(k14);
            ln.Add(k14);
            Novcic k15 = new Novcic("sprites\\nov.png", 60, 60, 35, 35);
            k15.SetTransparentColor(Color.White);
            k15.SetSize(50);
            Game.AddSprite(k15);
            ln.Add(k15);
            Novcic k16 = new Novcic("sprites\\nov.png", 90, 60, 35, 35);
            k16.SetTransparentColor(Color.White);
            k16.SetSize(50);
            Game.AddSprite(k16);
            ln.Add(k16);
            Novcic k17 = new Novcic("sprites\\nov.png", 120, 60, 35, 35);
            k17.SetTransparentColor(Color.White);
            k17.SetSize(50);
            Game.AddSprite(k17);
            ln.Add(k17);
            Novcic k18 = new Novcic("sprites\\nov.png", 150, 60, 35, 35);
            k18.SetTransparentColor(Color.White);
            k18.SetSize(50);
            Game.AddSprite(k18);
            ln.Add(k18);

            Novcic k19 = new Novcic("sprites\\nov.png", 0, 90, 35, 35);
            k19.SetTransparentColor(Color.White);
            k19.SetSize(50);
            Game.AddSprite(k19);
            ln.Add(k19);
            Novcic k20 = new Novcic("sprites\\nov.png", 30, 90, 35, 35);
            k20.SetTransparentColor(Color.White);
            k20.SetSize(50);
            Game.AddSprite(k20);
            ln.Add(k20);
            Novcic k21 = new Novcic("sprites\\nov.png", 60, 90, 35, 35);
            k21.SetTransparentColor(Color.White);
            k21.SetSize(50);
            Game.AddSprite(k21);
            ln.Add(k21);
            Novcic k22 = new Novcic("sprites\\nov.png", 90, 90, 35, 35);
            k22.SetTransparentColor(Color.White);
            k22.SetSize(50);
            Game.AddSprite(k22);
            ln.Add(k22);
            Novcic k23 = new Novcic("sprites\\nov.png", 120, 90, 35, 35);
            k23.SetTransparentColor(Color.White);
            k23.SetSize(50);
            Game.AddSprite(k23);
            ln.Add(k23);
            Novcic k24 = new Novcic("sprites\\nov.png", 150, 90, 35, 35);
            k24.SetTransparentColor(Color.White);
            k24.SetSize(50);
            Game.AddSprite(k24);
            ln.Add(k24);

            Novcic k25 = new Novcic("sprites\\nov.png", 0, 120, 35, 35);
            k25.SetTransparentColor(Color.White);
            k25.SetSize(50);
            Game.AddSprite(k25);
            ln.Add(k25);
            Novcic k26 = new Novcic("sprites\\nov.png", 30, 120, 35, 35);
            k26.SetTransparentColor(Color.White);
            k26.SetSize(50);
            Game.AddSprite(k26);
            ln.Add(k26);
            Novcic k27 = new Novcic("sprites\\nov.png", 60, 120, 35, 35);
            k27.SetTransparentColor(Color.White);
            k27.SetSize(50);
            Game.AddSprite(k27);
            ln.Add(k27);
            Novcic k28 = new Novcic("sprites\\nov.png", 90, 120, 35, 35);
            k28.SetTransparentColor(Color.White);
            k28.SetSize(50);
            Game.AddSprite(k28);
            ln.Add(k28);
            Novcic k29 = new Novcic("sprites\\nov.png", 120, 120, 35, 35);
            k29.SetTransparentColor(Color.White);
            k29.SetSize(50);
            Game.AddSprite(k29);
            ln.Add(k29);
            Novcic k30 = new Novcic("sprites\\nov.png", 150, 120, 35, 35);
            k30.SetTransparentColor(Color.White);
            k30.SetSize(50);
            Game.AddSprite(k30);
            ln.Add(k30);

            Novcic k31 = new Novcic("sprites\\nov.png", 205, 0, 35, 35);
            k31.SetTransparentColor(Color.White);
            k31.SetSize(50);
            Game.AddSprite(k31);
            ln.Add(k31);
            Novcic k32 = new Novcic("sprites\\nov.png", 235, 0, 35, 35);
            k32.SetTransparentColor(Color.White);
            k32.SetSize(50);
            Game.AddSprite(k32);
            ln.Add(k32);
            Novcic k33 = new Novcic("sprites\\nov.png", 265, 0, 35, 35);
            k33.SetTransparentColor(Color.White);
            k33.SetSize(50);
            Game.AddSprite(k33);
            ln.Add(k33);
            Novcic k34 = new Novcic("sprites\\nov.png", 295, 0, 35, 35);
            k34.SetTransparentColor(Color.White);
            k34.SetSize(50);
            Game.AddSprite(k34);
            ln.Add(k34);
            Novcic k35 = new Novcic("sprites\\nov.png", 325, 0, 35, 35);
            k35.SetTransparentColor(Color.White);
            k35.SetSize(50);
            Game.AddSprite(k35);
            ln.Add(k35);
            Novcic k36 = new Novcic("sprites\\nov.png", 355, 0, 35, 35);
            k36.SetTransparentColor(Color.White);
            k36.SetSize(50);
            Game.AddSprite(k36);
            ln.Add(k36);
            Novcic k37 = new Novcic("sprites\\nov.png", 385, 0, 35, 35);
            k37.SetTransparentColor(Color.White);
            k37.SetSize(50);
            Game.AddSprite(k37);
            ln.Add(k37);

            Novcic k38 = new Novcic("sprites\\nov.png", 205, 30, 35, 35);
            k38.SetTransparentColor(Color.White);
            k38.SetSize(50);
            Game.AddSprite(k38);
            ln.Add(k38);
            Novcic k39 = new Novcic("sprites\\nov.png", 235, 30, 35, 35);
            k39.SetTransparentColor(Color.White);
            k39.SetSize(50);
            Game.AddSprite(k39);
            ln.Add(k39);
            Novcic k40 = new Novcic("sprites\\nov.png", 265, 30, 35, 35);
            k40.SetTransparentColor(Color.White);
            k40.SetSize(50);
            Game.AddSprite(k40);
            ln.Add(k40);
            Novcic k41 = new Novcic("sprites\\nov.png", 295, 30, 35, 35);
            k41.SetTransparentColor(Color.White);
            k41.SetSize(50);
            Game.AddSprite(k41);
            ln.Add(k41);
            Novcic k42 = new Novcic("sprites\\nov.png", 325, 30, 35, 35);
            k42.SetTransparentColor(Color.White);
            k42.SetSize(50);
            Game.AddSprite(k42);
            ln.Add(k42);
            Novcic k43 = new Novcic("sprites\\nov.png", 355, 30, 35, 35);
            k43.SetTransparentColor(Color.White);
            k43.SetSize(50);
            Game.AddSprite(k43);
            ln.Add(k43);
            Novcic k44 = new Novcic("sprites\\nov.png", 385, 30, 35, 35);
            k44.SetTransparentColor(Color.White);
            k44.SetSize(50);
            Game.AddSprite(k44);
            ln.Add(k44);

            Novcic k45 = new Novcic("sprites\\nov.png", 205, 60, 35, 35);
            k45.SetTransparentColor(Color.White);
            k45.SetSize(50);
            Game.AddSprite(k45);
            ln.Add(k45);
            Novcic k46 = new Novcic("sprites\\nov.png", 235, 60, 35, 35);
            k46.SetTransparentColor(Color.White);
            k46.SetSize(50);
            Game.AddSprite(k46);
            ln.Add(k46);
            Novcic k47 = new Novcic("sprites\\nov.png", 265, 60, 35, 35);
            k47.SetTransparentColor(Color.White);
            k47.SetSize(50);
            Game.AddSprite(k47);
            ln.Add(k47);
            Novcic k48 = new Novcic("sprites\\nov.png", 295, 60, 35, 35);
            k48.SetTransparentColor(Color.White);
            k48.SetSize(50);
            Game.AddSprite(k48);
            ln.Add(k48);
            Novcic k49 = new Novcic("sprites\\nov.png", 325, 60, 35, 35);
            k49.SetTransparentColor(Color.White);
            k49.SetSize(50);
            Game.AddSprite(k49);
            ln.Add(k49);
            Novcic k50 = new Novcic("sprites\\nov.png", 355, 60, 35, 35);
            k50.SetTransparentColor(Color.White);
            k50.SetSize(50);
            Game.AddSprite(k50);
            ln.Add(k50);
            Novcic k51 = new Novcic("sprites\\nov.png", 385, 60, 35, 35);
            k51.SetTransparentColor(Color.White);
            k51.SetSize(50);
            Game.AddSprite(k51);
            ln.Add(k51);

            Novcic k52 = new Novcic("sprites\\nov.png", 205, 90, 35, 35);
            k52.SetTransparentColor(Color.White);
            k52.SetSize(50);
            Game.AddSprite(k52);
            ln.Add(k52);
            Novcic k53 = new Novcic("sprites\\nov.png", 235, 90, 35, 35);
            k53.SetTransparentColor(Color.White);
            k53.SetSize(53);
            Game.AddSprite(k53);
            ln.Add(k53);
            Novcic k54 = new Novcic("sprites\\nov.png", 265, 90, 35, 35);
            k54.SetTransparentColor(Color.White);
            k54.SetSize(54);
            Game.AddSprite(k54);
            ln.Add(k54);
            Novcic k55 = new Novcic("sprites\\nov.png", 295, 90, 35, 35);
            k55.SetTransparentColor(Color.White);
            k55.SetSize(50);
            Game.AddSprite(k55);
            ln.Add(k55);
            Novcic k56 = new Novcic("sprites\\nov.png", 325, 90, 35, 35);
            k56.SetTransparentColor(Color.White);
            k56.SetSize(50);
            Game.AddSprite(k56);
            ln.Add(k56);
            Novcic k57 = new Novcic("sprites\\nov.png", 355, 90, 35, 35);
            k57.SetTransparentColor(Color.White);
            k57.SetSize(50);
            Game.AddSprite(k57);
            ln.Add(k57);
            Novcic k58 = new Novcic("sprites\\nov.png", 385, 90, 35, 35);
            k58.SetTransparentColor(Color.White);
            k58.SetSize(50);
            Game.AddSprite(k58);
            ln.Add(k58);

            Novcic k59 = new Novcic("sprites\\nov.png", 205, 120, 35, 35);
            k59.SetTransparentColor(Color.White);
            k59.SetSize(50);
            Game.AddSprite(k59);
            ln.Add(k59);
            Novcic k60 = new Novcic("sprites\\nov.png", 235, 120, 35, 35);
            k60.SetTransparentColor(Color.White);
            k60.SetSize(50);
            Game.AddSprite(k60);
            ln.Add(k60);
            Novcic k61 = new Novcic("sprites\\nov.png", 265, 120, 35, 35);
            k61.SetTransparentColor(Color.White);
            k61.SetSize(50);
            Game.AddSprite(k61);
            ln.Add(k61);
            Novcic k62 = new Novcic("sprites\\nov.png", 295, 120, 35, 35);
            k62.SetTransparentColor(Color.White);
            k62.SetSize(50);
            Game.AddSprite(k62);
            ln.Add(k62);
            Novcic k63 = new Novcic("sprites\\nov.png", 325, 120, 35, 35);
            k63.SetTransparentColor(Color.White);
            k63.SetSize(50);
            Game.AddSprite(k63);
            ln.Add(k63);
            Novcic k64 = new Novcic("sprites\\nov.png", 355, 120, 35, 35);
            k64.SetTransparentColor(Color.White);
            k64.SetSize(50);
            Game.AddSprite(k64);
            ln.Add(k64);
            Novcic k65 = new Novcic("sprites\\nov.png", 385, 120, 35, 35);
            k65.SetTransparentColor(Color.White);
            k65.SetSize(50);
            Game.AddSprite(k65);
            ln.Add(k65);

            Novcic k66 = new Novcic("sprites\\nov.png", 440, 0, 35, 35);
            k66.SetTransparentColor(Color.White);
            k66.SetSize(50);
            Game.AddSprite(k66);
            ln.Add(k66);
            Novcic k67 = new Novcic("sprites\\nov.png", 470, 0, 35, 35);
            k67.SetTransparentColor(Color.White);
            k67.SetSize(50);
            Game.AddSprite(k67);
            ln.Add(k67);
            Novcic k68 = new Novcic("sprites\\nov.png", 500, 0, 35, 35);
            k68.SetTransparentColor(Color.White);
            k68.SetSize(50);
            Game.AddSprite(k68);
            ln.Add(k68);
            Novcic k69 = new Novcic("sprites\\nov.png", 530, 0, 35, 35);
            k69.SetTransparentColor(Color.White);
            k69.SetSize(50);
            Game.AddSprite(k69);
            ln.Add(k69);

            Novcic k70 = new Novcic("sprites\\nov.png", 440, 30, 35, 35);
            k70.SetTransparentColor(Color.White);
            k70.SetSize(50);
            Game.AddSprite(k70);
            ln.Add(k70);
            Novcic k71 = new Novcic("sprites\\nov.png", 470, 30, 35, 35);
            k71.SetTransparentColor(Color.White);
            k71.SetSize(50);
            Game.AddSprite(k71);
            ln.Add(k71);
            Novcic k72 = new Novcic("sprites\\nov.png", 500, 30, 35, 35);
            k72.SetTransparentColor(Color.White);
            k72.SetSize(50);
            Game.AddSprite(k72);
            ln.Add(k72);
            Novcic k73 = new Novcic("sprites\\nov.png", 530, 30, 35, 35);
            k73.SetTransparentColor(Color.White);
            k73.SetSize(50);
            Game.AddSprite(k73);
            ln.Add(k73);

            Novcic k74 = new Novcic("sprites\\nov.png", 440, 60, 35, 35);
            k74.SetTransparentColor(Color.White);
            k74.SetSize(50);
            Game.AddSprite(k74);
            ln.Add(k74);
            Novcic k75 = new Novcic("sprites\\nov.png", 470, 60, 35, 35);
            k75.SetTransparentColor(Color.White);
            k75.SetSize(50);
            Game.AddSprite(k75);
            ln.Add(k75);
            Novcic k76 = new Novcic("sprites\\nov.png", 500, 60, 35, 35);
            k76.SetTransparentColor(Color.White);
            k76.SetSize(50);
            Game.AddSprite(k76);
            ln.Add(k76);
            Novcic k77 = new Novcic("sprites\\nov.png", 530, 60, 35, 35);
            k77.SetTransparentColor(Color.White);
            k77.SetSize(50);
            Game.AddSprite(k77);
            ln.Add(k77);
            Novcic k78 = new Novcic("sprites\\nov.png", 560, 60, 35, 35);
            k78.SetTransparentColor(Color.White);
            k78.SetSize(50);
            Game.AddSprite(k78);
            ln.Add(k78);
            Novcic k79 = new Novcic("sprites\\nov.png", 590, 60, 35, 35);
            k79.SetTransparentColor(Color.White);
            k79.SetSize(50);
            Game.AddSprite(k79);
            ln.Add(k79);
            Novcic k80 = new Novcic("sprites\\nov.png", 620, 60, 35, 35);
            k80.SetTransparentColor(Color.White);
            k80.SetSize(50);
            Game.AddSprite(k80);
            ln.Add(k80);
            Novcic k81 = new Novcic("sprites\\nov.png", 650, 60, 35, 35);
            k81.SetTransparentColor(Color.White);
            k81.SetSize(50);
            Game.AddSprite(k81);
            ln.Add(k81);
            Novcic k82 = new Novcic("sprites\\nov.png", 680, 60, 35, 35);
            k82.SetTransparentColor(Color.White);
            k82.SetSize(50);
            Game.AddSprite(k82);
            ln.Add(k82);

            Novcic k83 = new Novcic("sprites\\nov.png", 440, 90, 35, 35);
            k83.SetTransparentColor(Color.White);
            k83.SetSize(50);
            Game.AddSprite(k83);
            ln.Add(k83);
            Novcic k84 = new Novcic("sprites\\nov.png", 470, 90, 35, 35);
            k84.SetTransparentColor(Color.White);
            k84.SetSize(50);
            Game.AddSprite(k84);
            ln.Add(k84);
            Novcic k85 = new Novcic("sprites\\nov.png", 500, 90, 35, 35);
            k85.SetTransparentColor(Color.White);
            k85.SetSize(50);
            Game.AddSprite(k85);
            ln.Add(k85);
            Novcic k86 = new Novcic("sprites\\nov.png", 530, 90, 35, 35);
            k86.SetTransparentColor(Color.White);
            k86.SetSize(50);
            Game.AddSprite(k86);
            ln.Add(k86);
            Novcic k87 = new Novcic("sprites\\nov.png", 560, 90, 35, 35);
            k87.SetTransparentColor(Color.White);
            k87.SetSize(50);
            Game.AddSprite(k87);
            ln.Add(k87);
            Novcic k88 = new Novcic("sprites\\nov.png", 590, 90, 35, 35);
            k88.SetTransparentColor(Color.White);
            k88.SetSize(50);
            Game.AddSprite(k88);
            ln.Add(k88);
            Novcic k89 = new Novcic("sprites\\nov.png", 620, 90, 35, 35);
            k89.SetTransparentColor(Color.White);
            k89.SetSize(50);
            Game.AddSprite(k89);
            ln.Add(k89);
            Novcic k90 = new Novcic("sprites\\nov.png", 650, 90, 35, 35);
            k90.SetTransparentColor(Color.White);
            k90.SetSize(50);
            Game.AddSprite(k90);
            ln.Add(k90);
            Novcic k91 = new Novcic("sprites\\nov.png", 680, 90, 35, 35);
            k91.SetTransparentColor(Color.White);
            k91.SetSize(50);
            Game.AddSprite(k91);
            ln.Add(k91);

            Novcic k92 = new Novcic("sprites\\nov.png", 440, 120, 35, 35);
            k92.SetTransparentColor(Color.White);
            k92.SetSize(50);
            Game.AddSprite(k92);
            ln.Add(k92);
            Novcic k93 = new Novcic("sprites\\nov.png", 470, 120, 35, 35);
            k93.SetTransparentColor(Color.White);
            k93.SetSize(50);
            Game.AddSprite(k93);
            ln.Add(k93);
            Novcic k94 = new Novcic("sprites\\nov.png", 500, 120, 35, 35);
            k94.SetTransparentColor(Color.White);
            k94.SetSize(50);
            Game.AddSprite(k94);
            ln.Add(k94);
            Novcic k95 = new Novcic("sprites\\nov.png", 530, 120, 35, 35);
            k65.SetTransparentColor(Color.White);
            k95.SetSize(50);
            Game.AddSprite(k95);
            ln.Add(k95);
            Novcic k96 = new Novcic("sprites\\nov.png", 560, 120, 35, 35);
            k96.SetTransparentColor(Color.White);
            k96.SetSize(50);
            Game.AddSprite(k96);
            ln.Add(k96);
            Novcic k97 = new Novcic("sprites\\nov.png", 590, 120, 35, 35);
            k97.SetTransparentColor(Color.White);
            k97.SetSize(50);
            Game.AddSprite(k97);
            ln.Add(k97);
            Novcic k98 = new Novcic("sprites\\nov.png", 620, 120, 35, 35);
            k98.SetTransparentColor(Color.White);
            k98.SetSize(50);
            Game.AddSprite(k98);
            ln.Add(k98);
            Novcic k99 = new Novcic("sprites\\nov.png", 650, 120, 35, 35);
            k99.SetTransparentColor(Color.White);
            k99.SetSize(50);
            Game.AddSprite(k99);
            ln.Add(k99);
            Novcic k100 = new Novcic("sprites\\nov.png", 680, 120, 35, 35);
            k100.SetTransparentColor(Color.White);
            k100.SetSize(50);
            Game.AddSprite(k100);
            ln.Add(k100);

            Novcic k101 = new Novcic("sprites\\nov.png", 0, 350, 35, 35);
            k101.SetTransparentColor(Color.White);
            k101.SetSize(50);
            Game.AddSprite(k101);
            ln.Add(k101);
            Novcic k102 = new Novcic("sprites\\nov.png", 30, 350, 35, 35);
            k102.SetTransparentColor(Color.White);
            k102.SetSize(50);
            Game.AddSprite(k102);
            ln.Add(k102);
            Novcic k103 = new Novcic("sprites\\nov.png", 60, 350, 35, 35);
            k103.SetTransparentColor(Color.White);
            k103.SetSize(50);
            Game.AddSprite(k103);
            ln.Add(k103);
            Novcic k104 = new Novcic("sprites\\nov.png", 90, 350, 35, 35);
            k104.SetTransparentColor(Color.White);
            k104.SetSize(50);
            Game.AddSprite(k104);
            ln.Add(k104);
            Novcic k105 = new Novcic("sprites\\nov.png", 120, 350, 35, 35);
            k105.SetTransparentColor(Color.White);
            k105.SetSize(50);
            Game.AddSprite(k105);
            ln.Add(k105);
            Novcic k106 = new Novcic("sprites\\nov.png", 150, 350, 35, 35);
            k106.SetTransparentColor(Color.White);
            k106.SetSize(50);
            Game.AddSprite(k106);
            ln.Add(k106);
            Novcic k107 = new Novcic("sprites\\nov.png", 180, 350, 35, 35);
            k107.SetTransparentColor(Color.White);
            k107.SetSize(50);
            Game.AddSprite(k107);
            ln.Add(k107);
            Novcic k108 = new Novcic("sprites\\nov.png", 210, 350, 35, 35);
            k108.SetTransparentColor(Color.White);
            k108.SetSize(50);
            Game.AddSprite(k108);
            ln.Add(k108);
            Novcic k109 = new Novcic("sprites\\nov.png", 240, 350, 35, 35);
            k109.SetTransparentColor(Color.White);
            k109.SetSize(50);
            Game.AddSprite(k109);
            ln.Add(k109);

            Novcic k110 = new Novcic("sprites\\nov.png", 0, 380, 35, 35);
            k110.SetTransparentColor(Color.White);
            k110.SetSize(50);
            Game.AddSprite(k110);
            ln.Add(k110);
            Novcic k111 = new Novcic("sprites\\nov.png", 30, 380, 35, 35);
            k111.SetTransparentColor(Color.White);
            k111.SetSize(50);
            Game.AddSprite(k111);
            ln.Add(k111);
            Novcic k112 = new Novcic("sprites\\nov.png", 60, 380, 35, 35);
            k112.SetTransparentColor(Color.White);
            k112.SetSize(50);
            Game.AddSprite(k112);
            ln.Add(k112);
            Novcic k113 = new Novcic("sprites\\nov.png", 90, 380, 35, 35);
            k113.SetTransparentColor(Color.White);
            k113.SetSize(50);
            Game.AddSprite(k113);
            ln.Add(k113);
            Novcic k114 = new Novcic("sprites\\nov.png", 120, 380, 35, 35);
            k114.SetTransparentColor(Color.White);
            k114.SetSize(50);
            Game.AddSprite(k114);
            ln.Add(k114);
            Novcic k115 = new Novcic("sprites\\nov.png", 150, 380, 35, 35);
            k115.SetTransparentColor(Color.White);
            k115.SetSize(50);
            Game.AddSprite(k115);
            ln.Add(k115);
            Novcic k116 = new Novcic("sprites\\nov.png", 180, 380, 35, 35);
            k116.SetTransparentColor(Color.White);
            k116.SetSize(50);
            Game.AddSprite(k116);
            ln.Add(k116);
            Novcic k117 = new Novcic("sprites\\nov.png", 210, 380, 35, 35);
            k117.SetTransparentColor(Color.White);
            k117.SetSize(50);
            Game.AddSprite(k117);
            ln.Add(k117);
            Novcic k118 = new Novcic("sprites\\nov.png", 240, 380, 35, 35);
            k118.SetTransparentColor(Color.White);
            k118.SetSize(50);
            Game.AddSprite(k118);
            ln.Add(k118);

            Novcic k119 = new Novcic("sprites\\nov.png", 0, 410, 35, 35);
            k119.SetTransparentColor(Color.White);
            k119.SetSize(50);
            Game.AddSprite(k119);
            ln.Add(k119);
            Novcic k120 = new Novcic("sprites\\nov.png", 30, 410, 35, 35);
            k120.SetTransparentColor(Color.White);
            k120.SetSize(50);
            Game.AddSprite(k120);
            ln.Add(k120);
            Novcic k121 = new Novcic("sprites\\nov.png", 60, 410, 35, 35);
            k121.SetTransparentColor(Color.White);
            k121.SetSize(50);
            Game.AddSprite(k121);
            ln.Add(k121);
            Novcic k122 = new Novcic("sprites\\nov.png", 90, 410, 35, 35);
            k122.SetTransparentColor(Color.White);
            k122.SetSize(50);
            Game.AddSprite(k122);
            ln.Add(k122);
            Novcic k123 = new Novcic("sprites\\nov.png", 120, 410, 35, 35);
            k123.SetTransparentColor(Color.White);
            k123.SetSize(50);
            Game.AddSprite(k123);
            ln.Add(k123);
            Novcic k124 = new Novcic("sprites\\nov.png", 150, 410, 35, 35);
            k124.SetTransparentColor(Color.White);
            k124.SetSize(50);
            Game.AddSprite(k124);
            ln.Add(k124);
            Novcic k125 = new Novcic("sprites\\nov.png", 180, 410, 35, 35);
            k125.SetTransparentColor(Color.White);
            k125.SetSize(50);
            Game.AddSprite(k125);
            ln.Add(k125);
            Novcic k126 = new Novcic("sprites\\nov.png", 210, 410, 35, 35);
            k126.SetTransparentColor(Color.White);
            k126.SetSize(50);
            Game.AddSprite(k126);
            ln.Add(k126);
            Novcic k127 = new Novcic("sprites\\nov.png", 240, 410, 35, 35);
            k127.SetTransparentColor(Color.White);
            k127.SetSize(50);
            Game.AddSprite(k127);
            ln.Add(k127);

            Novcic k128 = new Novcic("sprites\\nov.png", 0, 440, 35, 35);
            k128.SetTransparentColor(Color.White);
            k128.SetSize(50);
            Game.AddSprite(k128);
            ln.Add(k128);
            Novcic k129 = new Novcic("sprites\\nov.png", 30, 440, 35, 35);
            k129.SetTransparentColor(Color.White);
            k129.SetSize(50);
            Game.AddSprite(k129);
            ln.Add(k129);
            Novcic k130 = new Novcic("sprites\\nov.png", 60, 440, 35, 35);
            k130.SetTransparentColor(Color.White);
            k130.SetSize(50);
            Game.AddSprite(k130);
            ln.Add(k130);
            Novcic k131 = new Novcic("sprites\\nov.png", 90, 440, 35, 35);
            k131.SetTransparentColor(Color.White);
            k131.SetSize(50);
            Game.AddSprite(k131);
            ln.Add(k131);
            Novcic k132 = new Novcic("sprites\\nov.png", 120, 440, 35, 35);
            k132.SetTransparentColor(Color.White);
            k132.SetSize(50);
            Game.AddSprite(k132);
            ln.Add(k132);
            Novcic k133 = new Novcic("sprites\\nov.png", 150, 440, 35, 35);
            k133.SetTransparentColor(Color.White);
            k133.SetSize(50);
            Game.AddSprite(k133);
            ln.Add(k133);
            Novcic k134 = new Novcic("sprites\\nov.png", 180, 440, 35, 35);
            k134.SetTransparentColor(Color.White);
            k134.SetSize(50);
            Game.AddSprite(k134);
            ln.Add(k134);
            Novcic k135 = new Novcic("sprites\\nov.png", 210, 440, 35, 35);
            k135.SetTransparentColor(Color.White);
            k135.SetSize(50);
            Game.AddSprite(k135);
            ln.Add(k135);
            Novcic k136 = new Novcic("sprites\\nov.png", 240, 440, 35, 35);
            k136.SetTransparentColor(Color.White);
            k136.SetSize(50);
            Game.AddSprite(k136);
            ln.Add(k136);

            Novcic k137 = new Novcic("sprites\\nov.png", 0, 470, 35, 35);
            k137.SetTransparentColor(Color.White);
            k137.SetSize(50);
            Game.AddSprite(k137);
            ln.Add(k137);
            Novcic k138 = new Novcic("sprites\\nov.png", 30, 470, 35, 35);
            k138.SetTransparentColor(Color.White);
            k138.SetSize(50);
            Game.AddSprite(k138);
            ln.Add(k138);
            Novcic k139 = new Novcic("sprites\\nov.png", 60, 470, 35, 35);
            k139.SetTransparentColor(Color.White);
            k139.SetSize(50);
            Game.AddSprite(k139);
            ln.Add(k139);
            Novcic k140 = new Novcic("sprites\\nov.png", 90, 470, 35, 35);
            k140.SetTransparentColor(Color.White);
            k140.SetSize(50);
            Game.AddSprite(k140);
            ln.Add(k140);
            Novcic k141 = new Novcic("sprites\\nov.png", 120, 470, 35, 35);
            k141.SetTransparentColor(Color.White);
            k141.SetSize(50);
            Game.AddSprite(k141);
            ln.Add(k141);
            Novcic k142 = new Novcic("sprites\\nov.png", 150, 470, 35, 35);
            k142.SetTransparentColor(Color.White);
            k142.SetSize(50);
            Game.AddSprite(k142);
            ln.Add(k142);
            Novcic k143 = new Novcic("sprites\\nov.png", 180, 470, 35, 35);
            k143.SetTransparentColor(Color.White);
            k143.SetSize(50);
            Game.AddSprite(k143);
            ln.Add(k143);
            Novcic k144 = new Novcic("sprites\\nov.png", 210, 470, 35, 35);
            k144.SetTransparentColor(Color.White);
            k144.SetSize(50);
            Game.AddSprite(k144);
            ln.Add(k144);
            Novcic k145 = new Novcic("sprites\\nov.png", 240, 470, 35, 35);
            k145.SetTransparentColor(Color.White);
            k145.SetSize(50);
            Game.AddSprite(k145);
            ln.Add(k145);

            Novcic k146 = new Novcic("sprites\\nov.png", 285, 350, 35, 35);
            k146.SetTransparentColor(Color.White);
            k146.SetSize(50);
            Game.AddSprite(k146);
            ln.Add(k146);
            Novcic k147 = new Novcic("sprites\\nov.png", 315, 350, 35, 35);
            k147.SetTransparentColor(Color.White);
            k147.SetSize(50);
            Game.AddSprite(k147);
            ln.Add(k147);
            Novcic k148 = new Novcic("sprites\\nov.png", 345, 350, 35, 35);
            k148.SetTransparentColor(Color.White);
            k148.SetSize(50);
            Game.AddSprite(k148);
            ln.Add(k148);
            Novcic k149 = new Novcic("sprites\\nov.png", 375, 350, 35, 35);
            k149.SetTransparentColor(Color.White);
            k149.SetSize(50);
            Game.AddSprite(k149);
            ln.Add(k149);
            Novcic k150 = new Novcic("sprites\\nov.png", 405, 350, 35, 35);
            k150.SetTransparentColor(Color.White);
            k150.SetSize(50);
            Game.AddSprite(k150);
            ln.Add(k150);
            Novcic k151 = new Novcic("sprites\\nov.png", 435, 350, 35, 35);
            k151.SetTransparentColor(Color.White);
            k151.SetSize(50);
            Game.AddSprite(k151);
            ln.Add(k151);
            Novcic k152 = new Novcic("sprites\\nov.png", 465, 350, 35, 35);
            k152.SetTransparentColor(Color.White);
            k152.SetSize(50);
            Game.AddSprite(k152);
            ln.Add(k152);

            Novcic k153 = new Novcic("sprites\\nov.png", 285, 380, 35, 35);
            k153.SetTransparentColor(Color.White);
            k153.SetSize(50);
            Game.AddSprite(k153);
            ln.Add(k153);
            Novcic k154 = new Novcic("sprites\\nov.png", 315, 380, 35, 35);
            k154.SetTransparentColor(Color.White);
            k154.SetSize(50);
            Game.AddSprite(k154);
            ln.Add(k154);
            Novcic k155 = new Novcic("sprites\\nov.png", 345, 380, 35, 35);
            k155.SetTransparentColor(Color.White);
            k155.SetSize(50);
            Game.AddSprite(k155);
            ln.Add(k155);
            Novcic k156 = new Novcic("sprites\\nov.png", 375, 380, 35, 35);
            k156.SetTransparentColor(Color.White);
            k156.SetSize(50);
            Game.AddSprite(k156);
            ln.Add(k156);
            Novcic k157 = new Novcic("sprites\\nov.png", 405, 380, 35, 35);
            k157.SetTransparentColor(Color.White);
            k157.SetSize(50);
            Game.AddSprite(k157);
            ln.Add(k157);
            Novcic k158 = new Novcic("sprites\\nov.png", 435, 380, 35, 35);
            k158.SetTransparentColor(Color.White);
            k158.SetSize(50);
            Game.AddSprite(k158);
            ln.Add(k158);
            Novcic k159 = new Novcic("sprites\\nov.png", 465, 380, 35, 35);
            k159.SetTransparentColor(Color.White);
            k159.SetSize(50);
            Game.AddSprite(k159);
            ln.Add(k159);

            Novcic k160 = new Novcic("sprites\\nov.png", 285, 410, 35, 35);
            k160.SetTransparentColor(Color.White);
            k160.SetSize(50);
            Game.AddSprite(k160);
            ln.Add(k160);
            Novcic k161 = new Novcic("sprites\\nov.png", 315, 410, 35, 35);
            k161.SetTransparentColor(Color.White);
            k161.SetSize(50);
            Game.AddSprite(k161);
            ln.Add(k161);
            Novcic k162 = new Novcic("sprites\\nov.png", 345, 410, 35, 35);
            k162.SetTransparentColor(Color.White);
            k162.SetSize(50);
            Game.AddSprite(k162);
            ln.Add(k162);
            Novcic k163 = new Novcic("sprites\\nov.png", 375, 410, 35, 35);
            k163.SetTransparentColor(Color.White);
            k163.SetSize(50);
            Game.AddSprite(k163);
            ln.Add(k163);
            Novcic k164 = new Novcic("sprites\\nov.png", 405, 410, 35, 35);
            k164.SetTransparentColor(Color.White);
            k164.SetSize(50);
            Game.AddSprite(k164);
            ln.Add(k164);
            Novcic k165 = new Novcic("sprites\\nov.png", 435, 410, 35, 35);
            k165.SetTransparentColor(Color.White);
            k165.SetSize(50);
            Game.AddSprite(k165);
            ln.Add(k165);
            Novcic k166 = new Novcic("sprites\\nov.png", 465, 410, 35, 35);
            k166.SetTransparentColor(Color.White);
            k166.SetSize(50);
            Game.AddSprite(k166);
            ln.Add(k166);

            Novcic k167 = new Novcic("sprites\\nov.png", 285, 440, 35, 35);
            k167.SetTransparentColor(Color.White);
            k167.SetSize(50);
            Game.AddSprite(k167);
            ln.Add(k167);
            Novcic k168 = new Novcic("sprites\\nov.png", 315, 440, 35, 35);
            k168.SetTransparentColor(Color.White);
            k168.SetSize(50);
            Game.AddSprite(k168);
            ln.Add(k168);
            Novcic k169 = new Novcic("sprites\\nov.png", 345, 440, 35, 35);
            k169.SetTransparentColor(Color.White);
            k169.SetSize(50);
            Game.AddSprite(k169);
            ln.Add(k169);
            Novcic k170 = new Novcic("sprites\\nov.png", 375, 440, 35, 35);
            k170.SetTransparentColor(Color.White);
            k170.SetSize(50);
            Game.AddSprite(k170);
            ln.Add(k170);
            Novcic k171 = new Novcic("sprites\\nov.png", 405, 440, 35, 35);
            k171.SetTransparentColor(Color.White);
            k171.SetSize(50);
            Game.AddSprite(k171);
            ln.Add(k171);
            Novcic k172 = new Novcic("sprites\\nov.png", 435, 440, 35, 35);
            k172.SetTransparentColor(Color.White);
            k172.SetSize(50);
            Game.AddSprite(k172);
            ln.Add(k172);
            Novcic k173 = new Novcic("sprites\\nov.png", 465, 440, 35, 35);
            k173.SetTransparentColor(Color.White);
            k173.SetSize(50);
            Game.AddSprite(k173);
            ln.Add(k173);

            Novcic k174 = new Novcic("sprites\\nov.png", 285, 470, 35, 35);
            k174.SetTransparentColor(Color.White);
            k174.SetSize(50);
            Game.AddSprite(k174);
            ln.Add(k174);
            Novcic k175 = new Novcic("sprites\\nov.png", 315, 470, 35, 35);
            k175.SetTransparentColor(Color.White);
            k175.SetSize(50);
            Game.AddSprite(k175);
            ln.Add(k175);
            Novcic k176 = new Novcic("sprites\\nov.png", 345, 470, 35, 35);
            k176.SetTransparentColor(Color.White);
            k176.SetSize(50);
            Game.AddSprite(k176);
            ln.Add(k176);
            Novcic k177 = new Novcic("sprites\\nov.png", 375, 470, 35, 35);
            k177.SetTransparentColor(Color.White);
            k177.SetSize(50);
            Game.AddSprite(k177);
            ln.Add(k177);
            Novcic k178 = new Novcic("sprites\\nov.png", 405, 470, 35, 35);
            k178.SetTransparentColor(Color.White);
            k178.SetSize(50);
            Game.AddSprite(k178);
            ln.Add(k178);
            Novcic k179 = new Novcic("sprites\\nov.png", 435, 470, 35, 35);
            k179.SetTransparentColor(Color.White);
            k179.SetSize(50);
            Game.AddSprite(k179);
            ln.Add(k179);
            Novcic k180 = new Novcic("sprites\\nov.png", 465, 470, 35, 35);
            k180.SetTransparentColor(Color.White);
            k180.SetSize(50);
            Game.AddSprite(k180);
            ln.Add(k180);

            Novcic k181 = new Novcic("sprites\\nov.png", 520, 350, 35, 35);
            k181.SetTransparentColor(Color.White);
            k181.SetSize(50);
            Game.AddSprite(k181);
            ln.Add(k181);
            Novcic k182 = new Novcic("sprites\\nov.png", 550, 350, 35, 35);
            k182.SetTransparentColor(Color.White);
            k182.SetSize(50);
            Game.AddSprite(k182);
            ln.Add(k182);
            Novcic k183 = new Novcic("sprites\\nov.png", 580, 350, 35, 35);
            k183.SetTransparentColor(Color.White);
            k183.SetSize(50);
            Game.AddSprite(k183);
            ln.Add(k183);
            Novcic k184 = new Novcic("sprites\\nov.png", 610, 350, 35, 35);
            k184.SetTransparentColor(Color.White);
            k184.SetSize(50);
            Game.AddSprite(k184);
            ln.Add(k184);
            Novcic k185 = new Novcic("sprites\\nov.png", 640, 350, 35, 35);
            k185.SetTransparentColor(Color.White);
            k185.SetSize(50);
            Game.AddSprite(k185);
            ln.Add(k185);
            Novcic k186 = new Novcic("sprites\\nov.png", 670, 350, 35, 35);
            k186.SetTransparentColor(Color.White);
            k186.SetSize(50);
            Game.AddSprite(k186);
            ln.Add(k186);
            Novcic k187 = new Novcic("sprites\\nov.png", 700, 350, 35, 35);
            k187.SetTransparentColor(Color.White);
            k187.SetSize(50);
            Game.AddSprite(k187);
            ln.Add(k187);

            Novcic k188 = new Novcic("sprites\\nov.png", 520, 380, 35, 35);
            k188.SetTransparentColor(Color.White);
            k188.SetSize(50);
            Game.AddSprite(k188);
            ln.Add(k188);
            Novcic k189 = new Novcic("sprites\\nov.png", 550, 380, 35, 35);
            k189.SetTransparentColor(Color.White);
            k189.SetSize(50);
            Game.AddSprite(k189);
            ln.Add(k189);
            Novcic k190 = new Novcic("sprites\\nov.png", 580, 380, 35, 35);
            k190.SetTransparentColor(Color.White);
            k190.SetSize(50);
            Game.AddSprite(k190);
            ln.Add(k190);
            Novcic k191 = new Novcic("sprites\\nov.png", 610, 380, 35, 35);
            k191.SetTransparentColor(Color.White);
            k191.SetSize(50);
            Game.AddSprite(k191);
            ln.Add(k191);
            Novcic k192 = new Novcic("sprites\\nov.png", 640, 380, 35, 35);
            k192.SetTransparentColor(Color.White);
            k192.SetSize(50);
            Game.AddSprite(k192);
            ln.Add(k192);
            Novcic k193 = new Novcic("sprites\\nov.png", 670, 380, 35, 35);
            k193.SetTransparentColor(Color.White);
            k193.SetSize(50);
            Game.AddSprite(k193);
            ln.Add(k193);
            Novcic k194 = new Novcic("sprites\\nov.png", 700, 380, 35, 35);
            k194.SetTransparentColor(Color.White);
            k194.SetSize(50);
            Game.AddSprite(k194);
            ln.Add(k194);

            Novcic k195 = new Novcic("sprites\\nov.png", 520, 410, 35, 35);
            k195.SetTransparentColor(Color.White);
            k195.SetSize(50);
            Game.AddSprite(k195);
            ln.Add(k195);
            Novcic k196 = new Novcic("sprites\\nov.png", 550, 410, 35, 35);
            k196.SetTransparentColor(Color.White);
            k196.SetSize(50);
            Game.AddSprite(k196);
            ln.Add(k196);
            Novcic k197 = new Novcic("sprites\\nov.png", 580, 410, 35, 35);
            k197.SetTransparentColor(Color.White);
            k197.SetSize(50);
            Game.AddSprite(k197);
            ln.Add(k197);
            Novcic k198 = new Novcic("sprites\\nov.png", 610, 410, 35, 35);
            k198.SetTransparentColor(Color.White);
            k198.SetSize(50);
            Game.AddSprite(k198);
            ln.Add(k198);
            Novcic k199 = new Novcic("sprites\\nov.png", 640, 410, 35, 35);
            k199.SetTransparentColor(Color.White);
            k199.SetSize(50);
            Game.AddSprite(k199);
            ln.Add(k199);
            Novcic k200 = new Novcic("sprites\\nov.png", 670, 410, 35, 35);
            k200.SetTransparentColor(Color.White);
            k200.SetSize(50);
            Game.AddSprite(k200);
            ln.Add(k200);
            Novcic k201 = new Novcic("sprites\\nov.png", 700, 410, 35, 35);
            k201.SetTransparentColor(Color.White);
            k201.SetSize(50);
            Game.AddSprite(k201);
            ln.Add(k201);

            Novcic k202 = new Novcic("sprites\\nov.png", 520, 440, 35, 35);
            k202.SetTransparentColor(Color.White);
            k202.SetSize(50);
            Game.AddSprite(k202);
            ln.Add(k202);
            Novcic k203 = new Novcic("sprites\\nov.png", 550, 440, 35, 35);
            k203.SetTransparentColor(Color.White);
            k203.SetSize(50);
            Game.AddSprite(k203);
            ln.Add(k203);
            Novcic k204 = new Novcic("sprites\\nov.png", 580, 440, 35, 35);
            k204.SetTransparentColor(Color.White);
            k204.SetSize(50);
            Game.AddSprite(k204);
            ln.Add(k204);
            Novcic k205 = new Novcic("sprites\\nov.png", 610, 440, 35, 35);
            k205.SetTransparentColor(Color.White);
            k205.SetSize(50);
            Game.AddSprite(k205);
            ln.Add(k205);
            Novcic k206 = new Novcic("sprites\\nov.png", 640, 440, 35, 35);
            k206.SetTransparentColor(Color.White);
            k206.SetSize(50);
            Game.AddSprite(k206);
            ln.Add(k206);
            Novcic k207 = new Novcic("sprites\\nov.png", 670, 440, 35, 35);
            k207.SetTransparentColor(Color.White);
            k207.SetSize(50);
            Game.AddSprite(k207);
            ln.Add(k207);
            Novcic k208 = new Novcic("sprites\\nov.png", 700, 440, 35, 35);
            k208.SetTransparentColor(Color.White);
            k208.SetSize(50);
            Game.AddSprite(k208);
            ln.Add(k208);

            Novcic k209 = new Novcic("sprites\\nov.png", 520, 470, 35, 35);
            k209.SetTransparentColor(Color.White);
            k209.SetSize(50);
            Game.AddSprite(k209);
            ln.Add(k209);
            Novcic k210 = new Novcic("sprites\\nov.png", 550, 470, 35, 35);
            k210.SetTransparentColor(Color.White);
            k210.SetSize(50);
            Game.AddSprite(k210);
            ln.Add(k210);
            Novcic k211 = new Novcic("sprites\\nov.png", 580, 470, 35, 35);
            k211.SetTransparentColor(Color.White);
            k211.SetSize(50);
            Game.AddSprite(k211);
            ln.Add(k211);
            Novcic k212 = new Novcic("sprites\\nov.png", 610, 470, 35, 35);
            k212.SetTransparentColor(Color.White);
            k212.SetSize(50);
            Game.AddSprite(k212);
            ln.Add(k212);
            Novcic k213 = new Novcic("sprites\\nov.png", 640, 470, 35, 35);
            k213.SetTransparentColor(Color.White);
            k213.SetSize(50);
            Game.AddSprite(k213);
            ln.Add(k213);
            Novcic k214 = new Novcic("sprites\\nov.png", 670, 470, 35, 35);
            k214.SetTransparentColor(Color.White);
            k214.SetSize(50);
            Game.AddSprite(k214);
            ln.Add(k214);
            Novcic k215 = new Novcic("sprites\\nov.png", 700, 470, 35, 35);
            k215.SetTransparentColor(Color.White);
            k215.SetSize(50);
            Game.AddSprite(k215);
            ln.Add(k215);

            Novcic k216 = new Novcic("sprites\\nov.png", 0, 200, 35, 35);
            k216.SetTransparentColor(Color.White);
            k216.SetSize(50);
            Game.AddSprite(k216);
            ln.Add(k216);
            Novcic k217 = new Novcic("sprites\\nov.png", 30, 200, 35, 35);
            k217.SetTransparentColor(Color.White);
            k217.SetSize(50);
            Game.AddSprite(k217);
            ln.Add(k217);
            Novcic k218 = new Novcic("sprites\\nov.png", 60, 200, 35, 35);
            k218.SetTransparentColor(Color.White);
            k218.SetSize(50);
            Game.AddSprite(k218);
            ln.Add(k218);
            Novcic k219 = new Novcic("sprites\\nov.png", 90, 200, 35, 35);
            k219.SetTransparentColor(Color.White);
            k219.SetSize(50);
            Game.AddSprite(k219);
            ln.Add(k219);
            Novcic k220 = new Novcic("sprites\\nov.png", 120, 200, 35, 35);
            k220.SetTransparentColor(Color.White);
            k220.SetSize(50);
            Game.AddSprite(k220);
            ln.Add(k220);
            Novcic k221 = new Novcic("sprites\\nov.png", 150, 200, 35, 35);
            k221.SetTransparentColor(Color.White);
            k221.SetSize(50);
            Game.AddSprite(k221);
            ln.Add(k221);
            Novcic k222 = new Novcic("sprites\\nov.png", 180, 200, 35, 35);
            k222.SetTransparentColor(Color.White);
            k222.SetSize(50);
            Game.AddSprite(k222);
            ln.Add(k222);
            Novcic k223 = new Novcic("sprites\\nov.png", 210, 200, 35, 35);
            k223.SetTransparentColor(Color.White);
            k223.SetSize(50);
            Game.AddSprite(k223);
            ln.Add(k223);

            Novcic k224 = new Novcic("sprites\\nov.png", 240, 200, 35, 35);
            k224.SetTransparentColor(Color.White);
            k224.SetSize(50);
            Game.AddSprite(k224);
            ln.Add(k224);
            Novcic k225 = new Novcic("sprites\\nov.png", 270, 200, 35, 35);
            k225.SetTransparentColor(Color.White);
            k225.SetSize(50);
            Game.AddSprite(k225);
            ln.Add(k225);
            Novcic k226 = new Novcic("sprites\\nov.png", 300, 200, 35, 35);
            k226.SetTransparentColor(Color.White);
            k226.SetSize(50);
            Game.AddSprite(k226);
            ln.Add(k226);
            Novcic k227 = new Novcic("sprites\\nov.png", 330, 200, 35, 35);
            k227.SetTransparentColor(Color.White);
            k227.SetSize(50);
            Game.AddSprite(k227);
            ln.Add(k227);
            Novcic k228 = new Novcic("sprites\\nov.png", 360, 200, 35, 35);
            k228.SetTransparentColor(Color.White);
            k228.SetSize(50);
            Game.AddSprite(k228);
            ln.Add(k228);
            Novcic k229 = new Novcic("sprites\\nov.png", 390, 200, 35, 35);
            k229.SetTransparentColor(Color.White);
            k229.SetSize(50);
            Game.AddSprite(k229);
            ln.Add(k229);
            Novcic k230 = new Novcic("sprites\\nov.png", 420, 200, 35, 35);
            k230.SetTransparentColor(Color.White);
            k230.SetSize(50);
            Game.AddSprite(k230);
            ln.Add(k230);
            Novcic k231 = new Novcic("sprites\\nov.png", 450, 200, 35, 35);
            k231.SetTransparentColor(Color.White);
            k231.SetSize(50);
            Game.AddSprite(k231);
            ln.Add(k231);
            Novcic k232 = new Novcic("sprites\\nov.png", 480, 200, 35, 35);
            k232.SetTransparentColor(Color.White);
            k232.SetSize(50);
            Game.AddSprite(k232);
            ln.Add(k232);
            Novcic k233 = new Novcic("sprites\\nov.png", 510, 200, 35, 35);
            k233.SetTransparentColor(Color.White);
            k233.SetSize(50);
            Game.AddSprite(k233);
            ln.Add(k233);
            Novcic k234 = new Novcic("sprites\\nov.png", 540, 200, 35, 35);
            k234.SetTransparentColor(Color.White);
            k234.SetSize(50);
            Game.AddSprite(k234);
            ln.Add(k234);
            Novcic k235 = new Novcic("sprites\\nov.png", 570, 200, 35, 35);
            k235.SetTransparentColor(Color.White);
            k235.SetSize(50);
            Game.AddSprite(k235);
            ln.Add(k235);
            Novcic k236 = new Novcic("sprites\\nov.png", 600, 200, 35, 35);
            k236.SetTransparentColor(Color.White);
            k236.SetSize(50);
            Game.AddSprite(k236);
            ln.Add(k236);
            Novcic k237 = new Novcic("sprites\\nov.png", 630, 200, 35, 35);
            k237.SetTransparentColor(Color.White);
            k237.SetSize(50);
            Game.AddSprite(k237);
            ln.Add(k237);
            Novcic k238 = new Novcic("sprites\\nov.png", 660, 200, 35, 35);
            k238.SetTransparentColor(Color.White);
            k238.SetSize(50);
            Game.AddSprite(k238);
            ln.Add(k238);
            Novcic k239 = new Novcic("sprites\\nov.png", 690, 200, 35, 35);
            k239.SetTransparentColor(Color.White);
            k239.SetSize(50);
            Game.AddSprite(k239);
            ln.Add(k239);

            Novcic k240 = new Novcic("sprites\\nov.png", 240, 230, 35, 35);
            k240.SetTransparentColor(Color.White);
            k240.SetSize(50);
            Game.AddSprite(k240);
            ln.Add(k240);
            Novcic k241 = new Novcic("sprites\\nov.png", 270, 230, 35, 35);
            k241.SetTransparentColor(Color.White);
            k241.SetSize(50);
            Game.AddSprite(k241);
            ln.Add(k241);
            Novcic k242 = new Novcic("sprites\\nov.png", 300, 230, 35, 35);
            k242.SetTransparentColor(Color.White);
            k242.SetSize(50);
            Game.AddSprite(k242);
            ln.Add(k242);
            Novcic k243 = new Novcic("sprites\\nov.png", 330, 230, 35, 35);
            k243.SetTransparentColor(Color.White);
            k243.SetSize(50);
            Game.AddSprite(k243);
            ln.Add(k243);
            Novcic k244 = new Novcic("sprites\\nov.png", 360, 230, 35, 35);
            k244.SetTransparentColor(Color.White);
            k244.SetSize(50);
            Game.AddSprite(k244);
            ln.Add(k244);
            Novcic k245 = new Novcic("sprites\\nov.png", 390, 230, 35, 35);
            k245.SetTransparentColor(Color.White);
            k245.SetSize(50);
            Game.AddSprite(k245);
            ln.Add(k245);
            Novcic k246 = new Novcic("sprites\\nov.png", 420, 230, 35, 35);
            k246.SetTransparentColor(Color.White);
            k246.SetSize(50);
            Game.AddSprite(k246);
            ln.Add(k246);
            Novcic k247 = new Novcic("sprites\\nov.png", 450, 230, 35, 35);
            k247.SetTransparentColor(Color.White);
            k247.SetSize(50);
            Game.AddSprite(k247);
            ln.Add(k247);
            Novcic k248 = new Novcic("sprites\\nov.png", 480, 230, 35, 35);
            k248.SetTransparentColor(Color.White);
            k248.SetSize(50);
            Game.AddSprite(k248);
            ln.Add(k248);
            Novcic k249 = new Novcic("sprites\\nov.png", 510, 230, 35, 35);
            k249.SetTransparentColor(Color.White);
            k249.SetSize(50);
            Game.AddSprite(k249);
            ln.Add(k249);
            Novcic k250 = new Novcic("sprites\\nov.png", 540, 230, 35, 35);
            k250.SetTransparentColor(Color.White);
            k250.SetSize(50);
            Game.AddSprite(k250);
            ln.Add(k250);
            Novcic k251 = new Novcic("sprites\\nov.png", 570, 230, 35, 35);
            k251.SetTransparentColor(Color.White);
            k251.SetSize(50);
            Game.AddSprite(k251);
            ln.Add(k251);
            Novcic k252 = new Novcic("sprites\\nov.png", 600, 230, 35, 35);
            k252.SetTransparentColor(Color.White);
            k252.SetSize(50);
            Game.AddSprite(k252);
            ln.Add(k252);
            Novcic k253 = new Novcic("sprites\\nov.png", 630, 230, 35, 35);
            k253.SetTransparentColor(Color.White);
            k253.SetSize(50);
            Game.AddSprite(k253);
            ln.Add(k253);
            Novcic k254 = new Novcic("sprites\\nov.png", 660, 230, 35, 35);
            k254.SetTransparentColor(Color.White);
            k254.SetSize(50);
            Game.AddSprite(k254);
            ln.Add(k254);
            Novcic k255 = new Novcic("sprites\\nov.png", 690, 230, 35, 35);
            k255.SetTransparentColor(Color.White);
            k255.SetSize(50);
            Game.AddSprite(k255);
            ln.Add(k255);
            Novcic k256 = new Novcic("sprites\\nov.png", 0, 230, 35, 35);
            k256.SetTransparentColor(Color.White);
            k256.SetSize(50);
            Game.AddSprite(k256);
            ln.Add(k256);
            Novcic k257 = new Novcic("sprites\\nov.png", 30, 230, 35, 35);
            k257.SetTransparentColor(Color.White);
            k257.SetSize(50);
            Game.AddSprite(k257);
            ln.Add(k257);
            Novcic k258 = new Novcic("sprites\\nov.png", 60, 230, 35, 35);
            k258.SetTransparentColor(Color.White);
            k258.SetSize(50);
            Game.AddSprite(k258);
            ln.Add(k258);
            Novcic k259 = new Novcic("sprites\\nov.png", 90, 230, 35, 35);
            k259.SetTransparentColor(Color.White);
            k259.SetSize(50);
            Game.AddSprite(k259);
            ln.Add(k259);
            Novcic k260 = new Novcic("sprites\\nov.png", 120, 230, 35, 35);
            k260.SetTransparentColor(Color.White);
            k260.SetSize(50);
            Game.AddSprite(k260);
            ln.Add(k260);
            Novcic k261 = new Novcic("sprites\\nov.png", 150, 230, 35, 35);
            k261.SetTransparentColor(Color.White);
            k261.SetSize(50);
            Game.AddSprite(k261);
            ln.Add(k261);
            Novcic k262 = new Novcic("sprites\\nov.png", 180, 230, 35, 35);
            k262.SetTransparentColor(Color.White);
            k262.SetSize(50);
            Game.AddSprite(k262);
            ln.Add(k262);
            Novcic k263 = new Novcic("sprites\\nov.png", 210, 230, 35, 35);
            k263.SetTransparentColor(Color.White);
            k263.SetSize(50);
            Game.AddSprite(k263);
            ln.Add(k263);

            Novcic k264= new Novcic("sprites\\nov.png", 240, 260, 35, 35);
            k264.SetTransparentColor(Color.White);
            k264.SetSize(50);
            Game.AddSprite(k264);
            ln.Add(k264);
            Novcic k265 = new Novcic("sprites\\nov.png", 270, 260, 35, 35);
            k265.SetTransparentColor(Color.White);
            k265.SetSize(50);
            Game.AddSprite(k265);
            ln.Add(k265);
            Novcic k266 = new Novcic("sprites\\nov.png", 300, 260, 35, 35);
            k266.SetTransparentColor(Color.White);
            k266.SetSize(50);
            Game.AddSprite(k266);
            ln.Add(k266);
            Novcic k267 = new Novcic("sprites\\nov.png", 330, 260, 35, 35);
            k267.SetTransparentColor(Color.White);
            k267.SetSize(50);
            Game.AddSprite(k267);
            ln.Add(k267);
            Novcic k268 = new Novcic("sprites\\nov.png", 360, 260, 35, 35);
            k268.SetTransparentColor(Color.White);
            k268.SetSize(50);
            Game.AddSprite(k268);
            ln.Add(k268);
            Novcic k269 = new Novcic("sprites\\nov.png", 390, 260, 35, 35);
            k269.SetTransparentColor(Color.White);
            k269.SetSize(50);
            Game.AddSprite(k269);
            ln.Add(k269);
            Novcic k270 = new Novcic("sprites\\nov.png", 420, 260, 35, 35);
            k270.SetTransparentColor(Color.White);
            k270.SetSize(50);
            Game.AddSprite(k270);
            ln.Add(k270);
            Novcic k271 = new Novcic("sprites\\nov.png", 450, 260, 35, 35);
            k271.SetTransparentColor(Color.White);
            k271.SetSize(50);
            Game.AddSprite(k271);
            ln.Add(k271);
            Novcic k272 = new Novcic("sprites\\nov.png", 480, 260, 35, 35);
            k272.SetTransparentColor(Color.White);
            k272.SetSize(50);
            Game.AddSprite(k272);
            ln.Add(k272);
            Novcic k273 = new Novcic("sprites\\nov.png", 510, 260, 35, 35);
            k273.SetTransparentColor(Color.White);
            k273.SetSize(50);
            Game.AddSprite(k273);
            ln.Add(k273);
            Novcic k274 = new Novcic("sprites\\nov.png", 540, 260, 35, 35);
            k274.SetTransparentColor(Color.White);
            k274.SetSize(50);
            Game.AddSprite(k274);
            ln.Add(k274);
            Novcic k275 = new Novcic("sprites\\nov.png", 570, 260, 35, 35);
            k275.SetTransparentColor(Color.White);
            k275.SetSize(50);
            Game.AddSprite(k275);
            ln.Add(k275);
            Novcic k276 = new Novcic("sprites\\nov.png", 600, 260, 35, 35);
            k276.SetTransparentColor(Color.White);
            k276.SetSize(50);
            Game.AddSprite(k276);
            ln.Add(k276);
            Novcic k277 = new Novcic("sprites\\nov.png", 630, 260, 35, 35);
            k277.SetTransparentColor(Color.White);
            k277.SetSize(50);
            Game.AddSprite(k277);
            ln.Add(k277);
            Novcic k278 = new Novcic("sprites\\nov.png", 660, 260, 35, 35);
            k278.SetTransparentColor(Color.White);
            k278.SetSize(50);
            Game.AddSprite(k278);
            ln.Add(k278);
            Novcic k279 = new Novcic("sprites\\nov.png", 690, 260, 35, 35);
            k279.SetTransparentColor(Color.White);
            k279.SetSize(50);
            Game.AddSprite(k279);
            ln.Add(k279);
            Novcic k280 = new Novcic("sprites\\nov.png", 0, 260, 35, 35);
            k280.SetTransparentColor(Color.White);
            k280.SetSize(50);
            Game.AddSprite(k280);
            ln.Add(k280);
            Novcic k281 = new Novcic("sprites\\nov.png", 30, 260, 35, 35);
            k281.SetTransparentColor(Color.White);
            k281.SetSize(50);
            Game.AddSprite(k281);
            ln.Add(k281);
            Novcic k282 = new Novcic("sprites\\nov.png", 60, 260, 35, 35);
            k282.SetTransparentColor(Color.White);
            k282.SetSize(50);
            Game.AddSprite(k282);
            ln.Add(k282);
            Novcic k283 = new Novcic("sprites\\nov.png", 90, 260, 35, 35);
            k283.SetTransparentColor(Color.White);
            k283.SetSize(50);
            Game.AddSprite(k283);
            ln.Add(k283);
            Novcic k284 = new Novcic("sprites\\nov.png", 120, 260, 35, 35);
            k284.SetTransparentColor(Color.White);
            k284.SetSize(50);
            Game.AddSprite(k284);
            ln.Add(k284);
            Novcic k285 = new Novcic("sprites\\nov.png", 150, 260, 35, 35);
            k285.SetTransparentColor(Color.White);
            k285.SetSize(50);
            Game.AddSprite(k285);
            ln.Add(k285);
            Novcic k286 = new Novcic("sprites\\nov.png", 180, 260, 35, 35);
            k286.SetTransparentColor(Color.White);
            k286.SetSize(50);
            Game.AddSprite(k286);
            ln.Add(k286);
            Novcic k287 = new Novcic("sprites\\nov.png", 210, 260, 35, 35);
            k287.SetTransparentColor(Color.White);
            k287.SetSize(50);
            Game.AddSprite(k287);
            ln.Add(k287);



            pacman = new Pacman("sprites\\pacman.png", 3, 150, 75, 75);
            pacman.SetTransparentColor(Color.White);
            pacman.SetSize(50);
            pacman.RotationStyle = "AllAround";
            Game.AddSprite(pacman);

            prvi = new Duh("sprites\\inky.png", 600, 0, 75, 75);
            prvi.SetTransparentColor(Color.White);
            prvi.SetSize(50);
            prvi.RotationStyle = "AllAround";
            Game.AddSprite(prvi);

            drugi = new Duh("sprites\\pinky.png", 5, 450, 75, 75);
            drugi.SetTransparentColor(Color.White);
            drugi.SetSize(50);
            drugi.RotationStyle = "AllAround";
            Game.AddSprite(drugi);

            treci = new Duh("sprites\\blinky.png", 400, 400, 75, 75);
            treci.SetTransparentColor(Color.White);
            treci.SetSize(50);
            treci.RotationStyle = "AllAround";
            Game.AddSprite(treci);
            //3. scripts that start
            Game.StartScript(ZapocniIgru);


            //kad završi level
            //allSprites.Clear();
            //SetupGame2()
        }

        /* Scripts */
        delegate void DelegatTipaVoid(string text);
        private void PostaviTekstNaLabelu(string t)
        {
            // InvokeRequired required compares the thread ID of the  
            // calling thread to the thread ID of the creating thread.  
            // If these threads are different, it returns true.  
            if (this.lblRez.InvokeRequired)
            {
                DelegatTipaVoid d = new DelegatTipaVoid(PostaviTekstNaLabelu);
                this.Invoke(d, new object[] { t });
            }
            else
            {
                this.lblRez.Text = t;
            }
        }
        private int ZapocniIgru()
        {
            pacman.Zivot = 3;
            pacman.Kretanje = true;
            rezultat = 0;
            PostaviTekstNaLabelu("Rezultat:" + rezultat.ToString());

            kretanje = true;
            foreach(Novcic n in ln)
            {
                n.Vidljiv = true;
                n.SetVisible(true);
            }
            Wait(0.5);
            Glavna();
            return 0;
        }
        private int Glavna()
        {
            
                
            
            kretanje = true;
            
            pacman.X = 3;
            pacman.Y = 150;
            prvi.X = 600;
            prvi.Y = 0;
            drugi.X = 5;
            drugi.Y = 450;
            treci.X = 400;
            treci.Y = 400;

            START = true;

            if (START)
            {
                pacman.Kretanje = true;
                Wait(0.5);
                Game.StartScript(PacmanSeKrece);
                Wait(0.1);
                Game.StartScript(KretanjeDuhova);
               // break;
            };
            return 0;
        }

        private int PacmanSeKrece()
        {
            while (pacman.Kretanje) //ili neki drugi uvjet
            {
                if (sensing.KeyPressed(Keys.Right))
                {
                    pacman.SetHeading(0);
                    pacman.X += 20;

                }
                else if (sensing.KeyPressed(Keys.Left))
                {
                    pacman.SetHeading(180);
                    pacman.X -= 20;

                }
                else if (sensing.KeyPressed(Keys.Up))
                {
                    pacman.SetHeading(270);
                    pacman.Y -= 20;
                }
                else if (sensing.KeyPressed(Keys.Down))
                {
                    pacman.SetHeading(90);
                    pacman.Y += 20;
                }
                else
                {
                    pacman.X = pacman.X;
                    pacman.Y = pacman.Y;
                }
                if (pacman.TouchingSprite(z1) || pacman.TouchingSprite(z2) || pacman.TouchingSprite(z3) || pacman.TouchingSprite(z4))
                {
                   
                    kretanje = false;
                    pacman.Kretanje = false;
                    pacman.Zivot--;
                    if (pacman.Zivot != 0)
                    {
                        
                        
                        Wait(0.2);
                        Glavna();
                        break;
                    }
                    else
                    {
                        RangLista(rezultat);
                        DialogResult odgovor = MessageBox.Show("Želite li ponovno igrati?", "Da", MessageBoxButtons.YesNo);
                        START = false;
                        Wait(0.2);
                        if (odgovor == DialogResult.Yes)
                        {
                            
                            ZapocniIgru();
                            break;
                        }
                        else
                        {
                            KrajIgre();
                            break;
                        }

                    }
                }
                foreach(Novcic n in ln)
                {
                    if(pacman.TouchingSprite(n) && n.Vidljiv==true)
                    {
                        n.SetVisible(false);
                        n.Vidljiv = false;
                        rezultat++;
                        if(rezultat==287)
                        {
                            MessageBox.Show("Čestitam!Prešli ste prvu razinu!");
                            KrajIgre();
                        }
                        PostaviTekstNaLabelu("Rezultat:" + rezultat.ToString());
                    }
                }
                Wait(0.1);
            }
            return 0;
        }
        private int KretanjeDuhova()
        {
            Random r = new Random();
            int pozX = GameOptions.RightEdge - 100;
            int pozY = r.Next(0, GameOptions.DownEdge - 10);

           // Game.StartScript(SmjerDuhova);


            while (kretanje)
            {
                prvi.PointToSprite(pacman);
                prvi.MoveSteps(2);
                drugi.PointToSprite(pacman);
                drugi.MoveSteps(2);
                treci.PointToSprite(pacman);
                treci.MoveSteps(2);
                if (prvi.TouchingSprite(pacman))
                {
                    pacman.Kretanje = false;
                    kretanje = false;
                    pacman.Zivot--;
                    if (pacman.Zivot != 0)
                    {
                        
                        Wait(0.2);
                        Glavna();
                        break;
                    }
                    else
                    {
                        RangLista(rezultat);
                        DialogResult odgovor = MessageBox.Show("Želite li ponovno igrati?", "Upitnik", MessageBoxButtons.YesNo);
                        Wait(0.2);
                        if (odgovor == DialogResult.Yes)
                        {
                           
                            ZapocniIgru();
                            break;
                        }
                        else
                        {
                            KrajIgre();
                            break;
                        }

                    }
                }
                if (drugi.TouchingSprite(pacman))
                {
                    
                    kretanje = false;
                    pacman.Zivot--;
                    if (pacman.Zivot != 0)
                    {
                       
                        Wait(0.2);
                        Glavna();
                        break;
                    }
                    else
                    {
                        RangLista(rezultat);
                        DialogResult odgovor = MessageBox.Show("Želite li ponovno igrati?", "Upitnik", MessageBoxButtons.YesNo);
                        Wait(0.2);
                        if (odgovor == DialogResult.Yes)
                        {
                            pacman.Kretanje = false;
                            ZapocniIgru();
                            break;
                        }
                        else
                        {
                            KrajIgre();
                            break;
                        }

                    }
                }
                if (treci.TouchingSprite(pacman))
                {
                    pacman.Kretanje = false;
                    kretanje = false;
                    pacman.Zivot--;
                    if (pacman.Zivot != 0)
                    {
                       
                        Wait(0.2);
                        Glavna();
                        break;
                    }
                    else
                    {
                        RangLista(rezultat);
                        DialogResult odgovor = MessageBox.Show("Želite li ponovno igrati?", "Upitnik", MessageBoxButtons.YesNo);
                        Wait(0.2);
                        if (odgovor == DialogResult.Yes)
                        {
                            
                            ZapocniIgru();
                            break;
                        }
                        else
                        {
                            KrajIgre();
                            break;
                        }

                    }
                }
                List<Zid> lista = new List<Zid>();
                lista.Add(z1);
                lista.Add(z2);
                lista.Add(z3);
                lista.Add(z4);
                foreach (Zid z in lista)
                {
                    if(prvi.TouchingSprite(z))
                    {
                        prvi.X +=prvi.X;
                        prvi.Y += prvi.Y;
                    }
                }
                foreach (Zid z in lista)
                {
                    if (drugi.TouchingSprite(z))
                    {
                        drugi.X +=drugi.X;
                        drugi.Y += drugi.Y;
                    }
                }
                foreach (Zid z in lista)
                {
                    if (treci.TouchingSprite(z))
                    {
                        treci.X += treci.X;
                        treci.Y+= treci.Y;
                    }
                }
                Wait(0.01);
            }
            return 0;
        }
        /*private int SmjerDuhova()
        {
            while (true)
            {
                prvi.PointToSprite(pacman);
                drugi.PointToSprite(pacman);
                treci.PointToSprite(pacman);
                if (prvi.TouchingSprite(pacman) || drugi.TouchingSprite(pacman) || treci.TouchingSprite(pacman))
                {
                   
                        pacman.Zivot--;
                        if (pacman.Zivot > 0)
                        {
                            pacman.X = 3;
                            pacman.Y = 100;
                            prvi.X = 600;
                            prvi.Y = 0;
                            drugi.X = 5;
                            drugi.Y = 450;
                            treci.X = 400;
                            treci.Y = 400;

                        }
                        else
                        {
                            
                            DialogResult odgovor = MessageBox.Show("Želite li ponovno igrati?", "Da", MessageBoxButtons.YesNo);
                            if (odgovor == DialogResult.Yes)
                            {
                                ZapocniIgru();
                            }
                            else
                            {
                                KrajIgre();
                            }

                        }
                    }
                }
                return 0;
            }*/
        private void RangLista(int r)
        {
           
            SortedList <int,string> sl=new SortedList<int,string>();

            sl.Add(r, this.Player);
            
            string imeD = "poredak.txt";
            using (StreamReader sr = File.OpenText(imeD))
            {
                string linija;
                while ((linija = sr.ReadLine()) != null)
                {

                    string[] s = linija.Split('\t');
                    if (s.Length == 2)
                    {
                        if(sl.ContainsKey(Convert.ToInt32(s[1])))
                        {
                            break;
                        }
                        else
                            sl.Add(Convert.ToInt32(s[1]), s[0]);
                    }
                }

            }
            using (StreamWriter sw = new StreamWriter(imeD))
            {
               
                for(int i=sl.Count-1;i>=0;i--)
                {
                    sw.WriteLine(sl.Values[i] +'\t' + Convert.ToString(sl.Keys[i]));
                }
               /*foreach(int w in sl.Keys)
                {
                    sw.WriteLine(sl[w]+'\t'+Convert.ToInt32(w));
                }*/

            }
            
            POREDAK p = new POREDAK();
            p.frmPoredak = this;
            p.Pomocna = sl;
            p.ShowDialog();
          
        }
        private void IzbrisiLikove()
        {
            BGL.spriteCount = 0;
            
            BGL.allSprites.Clear();
            
            GC.Collect();
        }
        private void KrajIgre()
        {
            IzbrisiLikove();
            MessageBox.Show("Hvala na sudjelovanju!");
            Application.Exit();
        }
        /* ------------ GAME CODE END ------------ */


    }
}
