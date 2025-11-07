using System;
using System.Drawing;
using System.Windows.Forms;

namespace CostChef
{
    public partial class SplashScreenForm : Form
    {
        private System.Windows.Forms.Timer timer;
        private PictureBox pictureBox;
        private Label lblTitle;
        private Label lblVersion;
        private Label lblLoading;
        private ProgressBar progressBar;

        public SplashScreenForm()
        {
            InitializeComponent();
            this.Shown += SplashScreenForm_Shown;
        }

        private void InitializeComponent()
        {
            this.timer = new System.Windows.Forms.Timer();
            this.pictureBox = new PictureBox();
            this.lblTitle = new Label();
            this.lblVersion = new Label();
            this.lblLoading = new Label();
            this.progressBar = new ProgressBar();

            // Form
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(500, 350);
            this.Text = "CostChef - Starting...";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.White;
            this.ShowInTaskbar = false;

            // Picture Box - You can replace this with your actual logo
            this.pictureBox.Size = new System.Drawing.Size(150, 150);
            this.pictureBox.Location = new System.Drawing.Point(175, 50);
            this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox.BackColor = System.Drawing.Color.Transparent;
            
            // Create a simple chef hat logo programmatically
            this.pictureBox.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // Chef hat
                var hatBrush = new SolidBrush(System.Drawing.Color.DodgerBlue);
                var hatRect = new Rectangle(25, 20, 100, 40);
                g.FillEllipse(hatBrush, hatRect);
                
                // Hat base
                var baseBrush = new SolidBrush(System.Drawing.Color.White);
                var baseRect = new Rectangle(15, 40, 120, 20);
                g.FillRectangle(baseBrush, baseRect);
                
                // Utensils
                var utensilPen = new Pen(System.Drawing.Color.Gray, 3);
                g.DrawLine(utensilPen, 60, 70, 60, 120); // Spoon handle
                g.DrawLine(utensilPen, 90, 70, 90, 120); // Fork handle
                
                // Spoon bowl
                g.FillEllipse(new SolidBrush(System.Drawing.Color.LightGray), 45, 115, 30, 15);
                
                // Fork prongs
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(utensilPen, 85, 70 + (i * 5), 95, 70 + (i * 5));
                }
            };

            // Title Label
            this.lblTitle.Text = "CostChef";
            this.lblTitle.Location = new System.Drawing.Point(0, 210);
            this.lblTitle.Size = new System.Drawing.Size(500, 40);
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Version Label
            this.lblVersion.Text = "Simple Menu Costing Tool v1.1";
            this.lblVersion.Location = new System.Drawing.Point(0, 250);
            this.lblVersion.Size = new System.Drawing.Size(500, 20);
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular);
            this.lblVersion.ForeColor = System.Drawing.Color.DarkGray;
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Loading Label
            this.lblLoading.Text = "Loading...";
            this.lblLoading.Location = new System.Drawing.Point(0, 300);
            this.lblLoading.Size = new System.Drawing.Size(500, 20);
            this.lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular);
            this.lblLoading.ForeColor = System.Drawing.Color.Gray;
            this.lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Progress Bar
            this.progressBar.Location = new System.Drawing.Point(100, 280);
            this.progressBar.Size = new System.Drawing.Size(300, 10);
            this.progressBar.Style = ProgressBarStyle.Marquee;

            // Add controls
            this.Controls.AddRange(new Control[] {
                pictureBox, lblTitle, lblVersion, lblLoading, progressBar
            });

            // Timer
            this.timer.Interval = 2500; // 2.5 seconds
            this.timer.Tick += Timer_Tick;

            this.ResumeLayout(false);
        }

        private void SplashScreenForm_Shown(object sender, EventArgs e)
        {
            // Initialize database in background
            System.Threading.Tasks.Task.Run(() =>
            {
                DatabaseContext.InitializeDatabase();
            });
            
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}