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
            
            // Show splash screen first
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