using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public class PurchaseEntryForm : Form
    {
        private DateTimePicker dtpPurchaseDate;
        private ComboBox cmbSupplier;
        private TextBox txtNote;
        private DataGridView dataGridViewItems;
        private Button btnAddRow;
        private Button btnRemoveRow;
        private Button btnSave;
        private Button btnCancel;
        private Button btnNewIngredient;

        private List<Ingredient> _ingredients = new List<Ingredient>();
        private List<Supplier> _suppliers = new List<Supplier>();

        private NumberFormatInfo _currencyFormat;

        public PurchaseEntryForm()
        {
            InitializeComponent();
            InitializeCurrencyFormat();
            LoadLookups();
            ConfigureGridColumns();
            AddEmptyRow();
        }

        private void InitializeCurrencyFormat()
        {
            _currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            _currencyFormat.CurrencySymbol = AppSettings.CurrencySymbol;
        }

        private void InitializeComponent()
        {
            this.Text = "Record Purchases";
            this.StartPosition = FormStartPosition.CenterParent;
            this.ClientSize = new System.Drawing.Size(900, 550);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            dtpPurchaseDate = new DateTimePicker
            {
                Location = new System.Drawing.Point(20, 20),
                Width = 200,
                Value = DateTime.Today
            };

            var lblDate = new Label
            {
                Text = "Purchase Date:",
                Location = new System.Drawing.Point(20, 0),
                AutoSize = true
            };

            cmbSupplier = new ComboBox
            {
                Location = new System.Drawing.Point(240, 20),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            var lblSupplier = new Label
            {
                Text = "Supplier:",
                Location = new System.Drawing.Point(240, 0),
                AutoSize = true
            };

            txtNote = new TextBox
            {
                Location = new System.Drawing.Point(520, 20),
                Width = 350
            };

            var lblNote = new Label
            {
                Text = "Note / Invoice #:",
                Location = new System.Drawing.Point(520, 0),
                AutoSize = true
            };

            dataGridViewItems = new DataGridView
            {
                Location = new System.Drawing.Point(20, 60),
                Size = new System.Drawing.Size(850, 360),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            dataGridViewItems.CellValueChanged += DataGridViewItems_CellValueChanged;
            dataGridViewItems.CurrentCellDirtyStateChanged += DataGridViewItems_CurrentCellDirtyStateChanged;

            btnAddRow = new Button
            {
                Text = "Add Row",
                Location = new System.Drawing.Point(20, 430),
                Size = new System.Drawing.Size(100, 30)
            };
            btnAddRow.Click += (s, e) => AddEmptyRow();

            btnRemoveRow = new Button
            {
                Text = "Remove Row",
                Location = new System.Drawing.Point(130, 430),
                Size = new System.Drawing.Size(110, 30)
            };
            btnRemoveRow.Click += BtnRemoveRow_Click;

            btnNewIngredient = new Button
            {
                Text = "New Ingredient…",
                Location = new System.Drawing.Point(260, 430),
                Size = new System.Drawing.Size(130, 30)
            };
            btnNewIngredient.Click += BtnNewIngredient_Click;

            btnSave = new Button
            {
                Text = "Save Purchases",
                Location = new System.Drawing.Point(580, 430),
                Size = new System.Drawing.Size(130, 30)
            };
            btnSave.Click += BtnSave_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new System.Drawing.Point(710, 430),
                Size = new System.Drawing.Size(100, 30)
            };
            btnCancel.Click += (s, e) => this.Close();

            this.Controls.Add(lblDate);
            this.Controls.Add(dtpPurchaseDate);
            this.Controls.Add(lblSupplier);
            this.Controls.Add(cmbSupplier);
            this.Controls.Add(lblNote);
            this.Controls.Add(txtNote);
            this.Controls.Add(dataGridViewItems);
            this.Controls.Add(btnAddRow);
            this.Controls.Add(btnRemoveRow);
            this.Controls.Add(btnNewIngredient);
            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void LoadLookups()
        {
            try
            {
                _ingredients = DatabaseContext
                    .GetAllIngredients()
                    .OrderBy(i => i.Name)
                    .ToList();

                _suppliers = DatabaseContext
                    .GetAllSuppliers()
                    .OrderBy(s => s.Name)
                    .ToList();

                // Supplier combo
                cmbSupplier.Items.Clear();
                cmbSupplier.Items.Add(new Supplier { Id = 0, Name = "(No Supplier)" });
                foreach (var s in _suppliers)
                    cmbSupplier.Items.Add(s);

                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "Id";

                if (cmbSupplier.Items.Count > 0)
                    cmbSupplier.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredients/suppliers: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConfigureGridColumns()
        {
            dataGridViewItems.Columns.Clear();

            var ingredientColumn = new DataGridViewComboBoxColumn
            {
                Name = "IngredientColumn",
                HeaderText = "Ingredient",
                DataSource = _ingredients,
                DisplayMember = "Name",
                ValueMember = "Id",
                AutoComplete = true
            };

            var qtyColumn = new DataGridViewTextBoxColumn
            {
                Name = "QuantityColumn",
                HeaderText = "Quantity",
                Width = 80
            };

            var unitColumn = new DataGridViewTextBoxColumn
            {
                Name = "UnitColumn",
                HeaderText = "Unit",
                ReadOnly = true,
                Width = 80
            };

            var unitPriceColumn = new DataGridViewTextBoxColumn
            {
                Name = "UnitPriceColumn",
                HeaderText = "Unit Price",
                Width = 100,
                DefaultCellStyle =
                {
                    Format = "C2",
                    FormatProvider = _currencyFormat,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };

            var totalCostColumn = new DataGridViewTextBoxColumn
            {
                Name = "TotalCostColumn",
                HeaderText = "Total Cost",
                ReadOnly = true,
                Width = 120,
                DefaultCellStyle =
                {
                    Format = "C2",
                    FormatProvider = _currencyFormat,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            };

            dataGridViewItems.Columns.Add(ingredientColumn);
            dataGridViewItems.Columns.Add(qtyColumn);
            dataGridViewItems.Columns.Add(unitColumn);
            dataGridViewItems.Columns.Add(unitPriceColumn);
            dataGridViewItems.Columns.Add(totalCostColumn);
        }

        private void AddEmptyRow()
        {
            dataGridViewItems.Rows.Add();
        }

        private void BtnRemoveRow_Click(object sender, EventArgs e)
        {
            if (dataGridViewItems.CurrentRow != null && !dataGridViewItems.CurrentRow.IsNewRow)
            {
                dataGridViewItems.Rows.Remove(dataGridViewItems.CurrentRow);
            }
        }

        private void BtnNewIngredient_Click(object sender, EventArgs e)
        {
            using (var form = new IngredientEditForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadIngredients();
                }
            }
        }

        private void ReloadIngredients()
        {
            try
            {
                _ingredients = DatabaseContext
                    .GetAllIngredients()
                    .OrderBy(i => i.Name)
                    .ToList();

                var ingredientColumn =
                    dataGridViewItems.Columns["IngredientColumn"] as DataGridViewComboBoxColumn;

                if (ingredientColumn != null)
                {
                    ingredientColumn.DataSource = null;
                    ingredientColumn.DataSource = _ingredients;
                    ingredientColumn.DisplayMember = "Name";
                    ingredientColumn.ValueMember = "Id";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reloading ingredients: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DataGridViewItems_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dataGridViewItems.IsCurrentCellDirty)
            {
                dataGridViewItems.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void DataGridViewItems_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            var row = dataGridViewItems.Rows[e.RowIndex];
            var columnName = dataGridViewItems.Columns[e.ColumnIndex].Name;

            if (columnName == "IngredientColumn")
            {
                OnIngredientChanged(row);
            }
            else if (columnName == "QuantityColumn" || columnName == "UnitPriceColumn")
            {
                RecalculateRowTotal(row);
            }
        }

        private void OnIngredientChanged(DataGridViewRow row)
        {
            var ingredientValue = row.Cells["IngredientColumn"].Value;
            if (ingredientValue == null)
                return;

            int ingredientId = Convert.ToInt32(ingredientValue);
            var ingredient = _ingredients.FirstOrDefault(i => i.Id == ingredientId);
            if (ingredient == null)
                return;

            row.Cells["UnitColumn"].Value = ingredient.Unit;
            row.Cells["UnitPriceColumn"].Value = ingredient.UnitPrice;

            RecalculateRowTotal(row);
        }

        private void RecalculateRowTotal(DataGridViewRow row)
        {
            decimal qty = ParseDecimal(row.Cells["QuantityColumn"].Value);
            decimal unitPrice = ParseDecimal(row.Cells["UnitPriceColumn"].Value);

            if (qty > 0 && unitPrice > 0)
            {
                decimal total = qty * unitPrice;
                row.Cells["TotalCostColumn"].Value = total;
            }
            else
            {
                row.Cells["TotalCostColumn"].Value = null;
            }
        }

        private decimal ParseDecimal(object value)
        {
            if (value == null)
                return 0m;

            if (value is decimal d)
                return d;

            if (decimal.TryParse(Convert.ToString(value),
                                 NumberStyles.Any,
                                 CultureInfo.CurrentCulture,
                                 out var result))
            {
                return result;
            }

            return 0m;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                SavePurchases();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving purchases: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePurchases()
        {
            // Extract supplier
            int? supplierId = null;
            if (cmbSupplier.SelectedItem is Supplier s && s.Id > 0)
                supplierId = s.Id;

            DateTime purchaseDate = dtpPurchaseDate.Value.Date;
            string note = txtNote.Text?.Trim() ?? "";
            string reasonBase = $"Purchase on {purchaseDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(note))
                reasonBase += $" - {note}";

            // Validate at least one line
            var rows = dataGridViewItems.Rows.Cast<DataGridViewRow>()
                .Where(r => !r.IsNewRow)
                .ToList();

            if (rows.Count == 0)
            {
                MessageBox.Show("Please add at least one purchased item.", "Validation",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Load inventory levels once
            var inventoryLevels = DatabaseContext.GetInventoryLevels();

            // CHANGE: store current stock directly (decimal) so we can update it per line
            var invLookup = inventoryLevels.ToDictionary(i => i.IngredientId, i => i.CurrentStock);

            int processedLines = 0;

            foreach (var row in rows)
            {
                var ingredientValue = row.Cells["IngredientColumn"].Value;
                if (ingredientValue == null)
                    continue;

                int ingredientId = Convert.ToInt32(ingredientValue);

                decimal qty = ParseDecimal(row.Cells["QuantityColumn"].Value);
                if (qty <= 0)
                    continue;

                decimal unitPrice = ParseDecimal(row.Cells["UnitPriceColumn"].Value);

                // Use latest in-memory stock per ingredient (handles multiple lines for same item)
                decimal currentStock;
                if (!invLookup.TryGetValue(ingredientId, out currentStock))
                {
                    currentStock = 0m;
                }

                decimal newStock = currentStock + qty;

                // Update inventory (as "purchase")
                DatabaseContext.UpdateInventoryLevel(
                    ingredientId,
                    newStock,
                    null,
                    null,
                    "purchase",
                    reasonBase,
                    null);

                // Update in-memory lookup so subsequent lines for same ingredient build on this
                invLookup[ingredientId] = newStock;

                // Update ingredient price only if > 0, and respecting supplier rule
                if (unitPrice > 0)
                {
                    DatabaseContext.UpdateIngredientPriceFromPurchase(
                        ingredientId,
                        unitPrice,
                        supplierId,
                        reasonBase);
                }

                processedLines++;
            }

            if (processedLines == 0)
            {
                MessageBox.Show("No valid purchase lines were entered.", "Nothing to save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show($"Saved {processedLines} purchase line(s).", "Purchases Saved",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
