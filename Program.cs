using System;
using System.Windows.Forms;

namespace CostChef
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Initialize database BEFORE showing splash screen
            try
            {
                DatabaseContext.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing database: {ex.Message}\n\nThe application will continue but some features may not work properly.", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            
            // Show splash screen
            using (var splashScreen = new SplashScreenForm())
            {
                if (splashScreen.ShowDialog() == DialogResult.OK)
                {
                    // Splash screen completed, start main application
                    Application.Run(new MainForm());
                }
            }
        }
    }
}