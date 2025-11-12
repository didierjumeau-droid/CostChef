using System;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Text;
using System.IO;

namespace CostChef
{
    public partial class RawDatabaseDump : Form
    {
        private TextBox txtResults;
        
        public RawDatabaseDump()
        {
            InitializeComponent();
            DumpRawData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Text = "Raw Database Dump";
            
            this.txtResults = new TextBox
            {
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(776, 576),
                Multiline = true,
                ScrollBars = ScrollBars.Both,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 8)
            };
            
            this.Controls.Add(txtResults);
            this.ResumeLayout(false);
        }

        private void DumpRawData()
        {
            StringBuilder results = new StringBuilder();
            results.AppendLine("=== RAW DATABASE DUMP ===\n");
            
            string connectionString = "Data Source=costchef.db;Version=3;";
            
            try
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();

                    // DUMP 1: Raw suppliers data with hex values
                    results.AppendLine("1. RAW SUPPLIERS DATA (with hex values):");
                    results.AppendLine("=========================================");
                    
                    using (var cmd = new SQLiteCommand("SELECT id, name, hex(name) as name_hex, contact_person FROM suppliers ORDER BY id", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            results.AppendLine($"ID: {reader["id"]}");
                            results.AppendLine($"  Name: '{reader["name"]}'");
                            results.AppendLine($"  Name (hex): {reader["name_hex"]}");
                            results.AppendLine($"  Contact: '{reader["contact_person"]}'");
                            results.AppendLine();
                        }
                    }

                    // DUMP 2: Check for encoding issues
                    results.AppendLine("2. ENCODING ANALYSIS:");
                    results.AppendLine("=====================");
                    
                    using (var cmd = new SQLiteCommand("SELECT name, LENGTH(name) as name_len FROM suppliers", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var name = reader["name"].ToString();
                            var length = reader["name_len"];
                            results.AppendLine($"Supplier: '{name}'");
                            results.AppendLine($"  Length: {length} characters");
                            results.AppendLine($"  Has null chars: {name.Contains('\0')}");
                            results.AppendLine($"  Has weird chars: {HasWeirdCharacters(name)}");
                            results.AppendLine();
                        }
                    }

                    // DUMP 3: Manual byte-level inspection of specific supplier
                    results.AppendLine("3. MANUAL 'Local Market' SEARCH:");
                    results.AppendLine("================================");
                    
                    using (var cmd = new SQLiteCommand("SELECT id, name FROM suppliers WHERE name LIKE '%Local Market%'", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var id = reader["id"];
                            var name = reader["name"].ToString();
                            results.AppendLine($"FOUND: ID={id}, Name='{name}'");
                            results.AppendLine($"Name bytes: {BitConverter.ToString(System.Text.Encoding.UTF8.GetBytes(name))}");
                        }
                        else
                        {
                            results.AppendLine("NOT FOUND via SQL LIKE query (encoding issue!)");
                        }
                    }

                    // DUMP 4: Try different search methods
                    results.AppendLine("4. ALTERNATIVE SEARCH METHODS:");
                    results.AppendLine("==============================");
                    
                    // Method 1: Substring search
                    using (var cmd = new SQLiteCommand("SELECT id, name FROM suppliers WHERE INSTR(name, 'Local') > 0", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        results.AppendLine("Search with INSTR('Local'):");
                        while (reader.Read())
                        {
                            results.AppendLine($"  ID: {reader["id"]}, Name: '{reader["name"]}'");
                        }
                    }

                    // Method 2: Raw byte search
                    using (var cmd = new SQLiteCommand("SELECT id, name FROM suppliers", connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        results.AppendLine("\nAll suppliers (raw):");
                        while (reader.Read())
                        {
                            var name = reader["name"].ToString();
                            if (name.Contains("Local") || System.Text.Encoding.UTF8.GetBytes(name).Any(b => b == 0x4C)) // 'L'
                            {
                                results.AppendLine($"  ID: {reader["id"]}, Name: '{reader["name"]}'");
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                results.AppendLine($"ERROR: {ex.Message}");
            }
            
            txtResults.Text = results.ToString();
        }

        private bool HasWeirdCharacters(string text)
        {
            foreach (char c in text)
            {
                if (c < 32 && c != '\t' && c != '\n' && c != '\r') // Control chars
                    return true;
                if (c > 126) // Non-ASCII
                    return true;
            }
            return false;
        }
    }
}