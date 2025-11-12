using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Text;

namespace CostChef
{
    public partial class DatabaseIntegrityTest : Form
    {
        private TextBox txtResults;
        
        public DatabaseIntegrityTest()
        {
            InitializeComponent();
            RunIntegrityTest();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Database Integrity Test";
            
            this.txtResults = new TextBox
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(776, 576),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 9)
            };
            
            this.Controls.Add(txtResults);
            this.ResumeLayout(false);
        }

        private void RunIntegrityTest()
        {
            StringBuilder results = new StringBuilder();
            results.AppendLine("=== DATABASE INTEGRITY TEST ===\n");
            
            string connectionString = "Data Source=costchef.db;Version=3;";
            
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // Test 1: Count all suppliers
                    results.AppendLine("1. SUPPLIER COUNT TEST:");
                    using (var cmd = new SQLiteCommand("SELECT COUNT(*) as count FROM suppliers", connection))
                    {
                        var count = cmd.ExecuteScalar();
                        results.AppendLine($"   Total suppliers in database: {count}");
                    }

                    // Test 2: List ALL suppliers with their IDs
                    results.AppendLine("\n2. ALL SUPPLIERS WITH IDs:");
                    using (var cmd = new SQLiteCommand("SELECT id, name, contact_person FROM suppliers ORDER BY id", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.AppendLine($"   ID: {reader["id"]}, Name: '{reader["name"]}', Contact: '{reader["contact_person"]}'");
                        }
                    }

                    // Test 3: Check ingredient-supplier relationships
                    results.AppendLine("\n3. INGREDIENT-SUPPLIER RELATIONSHIPS:");
                    string query = @"
                        SELECT i.id as ing_id, i.name as ing_name, i.supplier_id, 
                               s.id as sup_id, s.name as sup_name
                        FROM ingredients i 
                        LEFT JOIN suppliers s ON i.supplier_id = s.id 
                        WHERE i.supplier_id IS NOT NULL
                        ORDER BY s.name, i.name";
                        
                    using (var cmd = new SQLiteCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var supId = reader["sup_id"] == DBNull.Value ? "NULL" : reader["sup_id"].ToString();
                            var supName = reader["sup_name"] == DBNull.Value ? "ORPHANED" : reader["sup_name"].ToString();
                            results.AppendLine($"   Ingredient: '{reader["ing_name"]}' (ID:{reader["ing_id"]})");
                            results.AppendLine($"     -> SupplierID: {reader["supplier_id"]}, Actual Supplier: '{supName}' (ID:{supId})");
                            results.AppendLine();
                        }
                    }

                    // Test 4: Try a manual update test
                    results.AppendLine("4. MANUAL UPDATE TEST:");
                    results.AppendLine("   This will test if we can write and immediately read back data...");
                    
                    // Create a test supplier if needed
                    using (var cmd = new SQLiteCommand("INSERT OR IGNORE INTO suppliers (name) VALUES ('TEST SUPPLIER')", connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Get the test supplier ID
                    int testSupplierId;
                    using (var cmd = new SQLiteCommand("SELECT id FROM suppliers WHERE name = 'TEST SUPPLIER'", connection))
                    {
                        testSupplierId = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    
                    results.AppendLine($"   Test Supplier ID: {testSupplierId}");
                    
                    // Update an ingredient with this supplier
                    using (var cmd = new SQLiteCommand("UPDATE ingredients SET supplier_id = @supId WHERE id = 1", connection))
                    {
                        cmd.Parameters.AddWithValue("@supId", testSupplierId);
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Immediately read it back
                    using (var cmd = new SQLiteCommand("SELECT i.name, s.name as sup_name FROM ingredients i LEFT JOIN suppliers s ON i.supplier_id = s.id WHERE i.id = 1", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            results.AppendLine($"   Ingredient 1 now has supplier: '{reader["sup_name"]}'");
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                results.AppendLine($"ERROR: {ex.Message}");
                results.AppendLine($"Stack: {ex.StackTrace}");
            }
            
            txtResults.Text = results.ToString();
        }
    }
}