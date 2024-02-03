
namespace OTTER
{
    partial class IzbornikForma
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnUpis = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnUpis
            // 
            this.btnUpis.Location = new System.Drawing.Point(144, 201);
            this.btnUpis.Name = "btnUpis";
            this.btnUpis.Size = new System.Drawing.Size(141, 58);
            this.btnUpis.TabIndex = 0;
            this.btnUpis.Text = "Upišite korisničko ime";
            this.btnUpis.UseVisualStyleBackColor = true;
            this.btnUpis.Click += new System.EventHandler(this.btnUpis_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(144, 125);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(141, 22);
            this.textBox1.TabIndex = 1;
            // 
            // IzbornikForma
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.btnUpis);
            this.Name = "IzbornikForma";
            this.Text = "IzbornikForma";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnUpis;
        private System.Windows.Forms.TextBox textBox1;
    }
}