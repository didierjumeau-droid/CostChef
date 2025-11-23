using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace CostChef
{
    public partial class ImportPreviewForm : Form
    {
        // FIX: Removed 'ImportExportService.' prefix to reference the standalone class
        private SafeImportResult _importResult;

        // FIX: Removed 'ImportExportService.' prefix from the parameter type
        public ImportPreviewForm(SafeImportResult importResult) 
        {
            _importResult = importResult;
            InitializeComponent();
            SetupPreview();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            ClientSize = new Size(600, 450);
            Name = "ImportPreviewForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Import Preview - Review Before Importing";
            MaximizeBox = false;
            MinimizeBox = false;

            // Add the Run Import button
            var btnRunImport = new Button
            {
                Text = "Run Import",
                Location = new Point(480, 410),
                Size = new Size(100, 30),
                DialogResult = DialogResult.OK
            };
            Controls.Add(btnRunImport);

            ResumeLayout(false);
        }

        private void SetupPreview()
        {
            Controls.Clear();

            // Title
            var lblTitle = new Label 
            { 
                Text = "Import Preview", 
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(300, 25)
            };

            // Summary
            var summaryText = $"New ingredients to import: {_importResult.NewIngredients}\n" +
                            $"New suppliers to create: {_importResult.NewSuppliers}\n" +
                            $"Duplicate ingredients (will be skipped): {_importResult.DuplicateNames.Count}";
            
            var lblSummary = new Label
            {
                Text = summaryText,
                Location = new Point(20, 50),
                Size = new Size(500, 60),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Tab Control for detailed view
            var tabControl = new TabControl
            {
                Location = new Point(20, 120),
                Size = new Size(560, 280)
            };
            
            // Tabs
            var tabNewIngredients = new TabPage("New Ingredients");
            var tabNewSuppliers = new TabPage("New Suppliers");
            var tabDuplicates = new TabPage("Duplicates (Skipped)");

            SetupNewIngredientsTab(tabNewIngredients);
            SetupNewSuppliersTab(tabNewSuppliers);
            SetupDuplicatesTab(tabDuplicates);

            tabControl.TabPages.Add(tabNewIngredients);
            tabControl.TabPages.Add(tabNewSuppliers);
            tabControl.TabPages.Add(tabDuplicates);

            Controls.AddRange(new Control[] { lblTitle, lblSummary, tabControl });
        }

        private void SetupNewIngredientsTab(TabPage tab)
        {
            var listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 10),
                Size = new Size(520, 200)
            };

            listView.Columns.Add("Ingredient Name", 200);
            listView.Columns.Add("Unit Price", 100);
            listView.Columns.Add("Unit", 100);
            listView.Columns.Add("Category", 100);

            foreach (var ingredient in _importResult.IngredientsToImport)
            {
                var item = new ListViewItem(ingredient.Name);
                item.SubItems.Add(ingredient.UnitPrice.ToString("F4"));
                item.SubItems.Add(ingredient.Unit);
                item.SubItems.Add(ingredient.Category);
                item.ForeColor = Color.Green;
                listView.Items.Add(item);
            }

            tab.Controls.Add(listView);
        }

        private void SetupNewSuppliersTab(TabPage tab)
        {
            var listView = new ListView
            {
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Location = new Point(10, 10),
                Size = new Size(520, 200)
            };

            listView.Columns.Add("Supplier Name", 300);
            listView.Columns.Add("Status", 200);

            foreach (var supplier in _importResult.SuppliersToImport)
            {
                var item = new ListViewItem(supplier.Name);
                item.SubItems.Add("Will be created during import");
                item.ForeColor = Color.Blue;
                listView.Items.Add(item);
            }

            tab.Controls.Add(listView);
        }

        private void SetupDuplicatesTab(TabPage tab)
        {
            var listBox = new ListBox
            {
                Location = new Point(10, 10),
                Size = new Size(520, 200),
                ForeColor = Color.Red
            };

            foreach (var name in _importResult.DuplicateNames.Take(100))
            {
                listBox.Items.Add(name);
            }

            if (_importResult.DuplicateNames.Count > 100)
            {
                listBox.Items.Add($"... and {_importResult.DuplicateNames.Count - 100} more");
            }

            tab.Controls.Add(listBox);
        }
    }
}