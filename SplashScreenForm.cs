using System;
using System.Drawing;
using System.IO;
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
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Text = "CostChef - Starting...";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = System.Drawing.Color.White;
            this.ShowInTaskbar = false;

            // Picture Box - 300x300 logo
            this.pictureBox.Size = new System.Drawing.Size(300, 300);
            this.pictureBox.Location = new System.Drawing.Point(150, 30);
            this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox.BorderStyle = BorderStyle.None;

            // Load logo immediately
            LoadLogoImage();

            // Title Label
            this.lblTitle.Text = "CostChef";
            this.lblTitle.Location = new System.Drawing.Point(0, 350);
            this.lblTitle.Size = new System.Drawing.Size(600, 40);
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.DodgerBlue;
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Version Label
            this.lblVersion.Text = "Food cost made easier";
            this.lblVersion.Location = new System.Drawing.Point(0, 395);
            this.lblVersion.Size = new System.Drawing.Size(600, 25);
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular);
            this.lblVersion.ForeColor = System.Drawing.Color.DarkGray;
            this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Loading Label
            this.lblLoading.Text = "Loading...";
            this.lblLoading.Location = new System.Drawing.Point(0, 430);
            this.lblLoading.Size = new System.Drawing.Size(600, 20);
            this.lblLoading.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular);
            this.lblLoading.ForeColor = System.Drawing.Color.Gray;
            this.lblLoading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // Progress Bar
            this.progressBar.Location = new System.Drawing.Point(150, 425);
            this.progressBar.Size = new System.Drawing.Size(300, 8);
            this.progressBar.Style = ProgressBarStyle.Marquee;

            // Add controls
            this.Controls.AddRange(new Control[] {
                pictureBox, lblTitle, lblVersion, lblLoading, progressBar
            });

            // Timer
            this.timer.Interval = 2500;
            this.timer.Tick += Timer_Tick;

            this.ResumeLayout(false);
        }

        private void LoadLogoImage()
        {
            try
            {
                // Look for logo in these locations
                string[] possiblePaths = {
                    "logo.png",
                    "logo.jpg",
                    "logo.bmp",
                    "splash.png", 
                    "splash.jpg",
                    "images/logo.png",
                    "images/logo.jpg",
                    "assets/logo.png",
                    "assets/logo.jpg"
                };

                string logoPath = null;
                foreach (string path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        logoPath = path;
                        break;
                    }
                }

                if (logoPath != null)
                {
                    // Load the image file
                    Image loadedImage = Image.FromFile(logoPath);
                    pictureBox.Image = loadedImage;
                }
                else
                {
                    // Fallback to drawn logo
                    CreateFallbackLogo();
                }
            }
            catch (Exception ex)
            {
                // If image loading fails, use the drawn fallback
                CreateFallbackLogo();
                System.Diagnostics.Debug.WriteLine($"Error loading logo: {ex.Message}");
            }
        }

        private void CreateFallbackLogo()
        {
            // Create a bitmap for the fallback logo
            Bitmap fallbackImage = new Bitmap(300, 300);
            using (Graphics g = Graphics.FromImage(fallbackImage))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.White);
                
                // Chef hat - scaled for 300x300
                var hatBrush = new SolidBrush(Color.DodgerBlue);
                var hatRect = new Rectangle(50, 40, 200, 80);
                g.FillEllipse(hatBrush, hatRect);
                
                // Hat base
                var baseBrush = new SolidBrush(Color.White);
                var baseRect = new Rectangle(30, 80, 240, 40);
                g.FillRectangle(baseBrush, baseRect);
                
                // Utensils
                var utensilPen = new Pen(Color.Gray, 6);
                g.DrawLine(utensilPen, 120, 140, 120, 240);
                g.DrawLine(utensilPen, 180, 140, 180, 240);
                
                // Spoon bowl
                g.FillEllipse(new SolidBrush(Color.LightGray), 90, 230, 60, 30);
                
                // Fork prongs
                for (int i = 0; i < 3; i++)
                {
                    g.DrawLine(utensilPen, 170, 140 + (i * 10), 190, 140 + (i * 10));
                }
            }
            
            pictureBox.Image = fallbackImage;
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