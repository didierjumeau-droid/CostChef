using System;
using System.IO;
using System.Windows.Forms;

namespace CostChef
{
    public static class DatabaseReset
    {
        public static void ResetDatabase()
        {
            try
            {
                // Close any open connections first
                System.Threading.Thread.Sleep(100);
                
                // Backup current database
                if (File.Exists("costchef.db"))
                {
                    string backupName = $"costchef_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                    File.Copy("costchef.db", backupName);
                }
                
                // Delete the corrupted database
                if (File.Exists("costchef.db"))
                {
                    File.Delete("costchef.db");
                }
                
                // Create fresh database
                DatabaseContext.InitializeDatabase();
                
                MessageBox.Show("✅ Database reset successfully!\n\n" +
                    "The phantom suppliers should now be gone.\n" +
                    "Please re-import your ingredients.", 
                    "Database Reset", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error resetting database: {ex.Message}", 
                    "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}