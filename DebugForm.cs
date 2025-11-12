using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.IO;

namespace CostChef
{
    public partial class DebugForm : Form
    {
        private TextBox txtResults;
        private Button btnClose;

        public DebugForm()
        {
            InitializeComponent();
            RunDatabaseInvestigation();
        }
// Add this method to your DebugForm class
private void SearchForCachedSuppliers()
{
    string results = txtResults.Text;
    results += "\n5. SEARCHING FOR CACHED/HARDCODED SUPPLIER DATA:\n";
    
    try
    {
        // Search for common supplier names in the code
        string[] knownSupplierNames = { "Local Market", "Quality Meats Ltd", "Wholesale Foods Inc", "Fresh Produce Co" };
        
        foreach (string supplierName in knownSupplierNames)
        {
            results += $"   Searching for: '{supplierName}'\n";
            
            // Check if this exists in any JSON/XML files
            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories)
                                .Where(f => !f.EndsWith(".db") && !f.EndsWith(".exe"));
            
            foreach (var file in files)
            {
                try
                {
                    var content = File.ReadAllText(file);
                    if (content.Contains(supplierName))
                    {
                        results += $"     FOUND in: {Path.GetFileName(file)}\n";
                    }
                }
                catch { /* Ignore unreadable files */ }
            }
        }
        
        results += "   Search completed.\n";
    }
    catch (Exception ex)
    {
        results += $"   ERROR during search: {ex.Message}\n";
    }
    
    txtResults.Text = results;
}
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Database Debug Investigation";
            this.StartPosition = FormStartPosition.CenterParent;

            // Results textbox
            this.txtResults = new TextBox
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(776, 500),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true
            };

            // Close button
            this.btnClose = new Button
            {
                Text = "Close",
                Location = new System.Drawing.Point(698, 525),
                Size = new System.Drawing.Size(90, 30)
            };
            this.btnClose.Click += (s, e) => this.Close();

            this.Controls.AddRange(new Control[] { txtResults, btnClose });
            this.ResumeLayout(false);
        }

        private void RunDatabaseInvestigation()
        {
            string connectionString = "Data Source=costchef.db;Version=3;";
            string results = "=== DATABASE DEBUG INVESTIGATION ===\n\n";

            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // 1. Check all suppliers in database
                    results += "1. ALL SUPPLIERS IN DATABASE:\n";
                    using (var command = new SQLiteCommand("SELECT * FROM suppliers ORDER BY id", connection))
                    using (var reader = command.ExecuteReader())
                    {
                        bool hasSuppliers = false;
                        while (reader.Read())
                        {
                            hasSuppliers = true;
                            results += $"   ID: {reader["id"]}, Name: '{reader["name"]}', Contact: '{reader["contact_person"]}'\n";
                        }
                        if (!hasSuppliers)
                            results += "   NO SUPPLIERS FOUND!\n";
                    }

                    // 2. Check ingredients with supplier relationships
                    results += "\n2. INGREDIENTS WITH SUPPLIER RELATIONSHIPS:\n";
                    string query = @"
                        SELECT i.id, i.name as ingredient_name, i.supplier_id, s.name as supplier_name 
                        FROM ingredients i 
                        LEFT JOIN suppliers s ON i.supplier_id = s.id 
                        WHERE i.supplier_id IS NOT NULL 
                        ORDER BY s.name, i.name";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        bool hasRelationships = false;
                        while (reader.Read())
                        {
                            hasRelationships = true;
                            var supplierId = reader["supplier_id"];
                            var supplierName = reader["supplier_name"]?.ToString() ?? "NULL/ORPHANED";
                            results += $"   Ingredient: '{reader["ingredient_name"]}' -> SupplierID: {supplierId}, SupplierName: '{supplierName}'\n";
                        }
                        if (!hasRelationships)
                            results += "   NO SUPPLIER RELATIONSHIPS FOUND!\n";
                    }

                    // 3. Check for orphaned supplier relationships
                    results += "\n3. ORPHANED SUPPLIER RELATIONSHIPS (supplier_id but no matching supplier):\n";
                    query = @"SELECT i.name, i.supplier_id 
                             FROM ingredients i 
                             WHERE i.supplier_id IS NOT NULL 
                             AND i.supplier_id NOT IN (SELECT id FROM suppliers)";

                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        bool hasOrphans = false;
                        while (reader.Read())
                        {
                            hasOrphans = true;
                            results += $"   Ingredient: '{reader["name"]}' -> Orphaned SupplierID: {reader["supplier_id"]}\n";
                        }
                        if (!hasOrphans)
                            results += "   NO ORPHANED RELATIONSHIPS FOUND!\n";
                    }

                    // 4. Check database file info
                    results += $"\n4. DATABASE FILE INFO:\n";
                    results += $"   File exists: {File.Exists("costchef.db")}\n";
                    if (File.Exists("costchef.db"))
                    {
                        var fileInfo = new FileInfo("costchef.db");
                        results += $"   File size: {fileInfo.Length} bytes\n";
                        results += $"   Last modified: {fileInfo.LastWriteTime}\n";
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                results += $"\nERROR: {ex.Message}\n";
            }

            txtResults.Text = results;
        }
    }
}