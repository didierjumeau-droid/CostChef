using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CostChef
{
    public partial class InventoryHistoryForm : Form
    {
        private readonly int _ingredientId;

        private Label lblTitle;
        private DataGridView dataGridViewHistory;
        private ComboBox cmbFilterType;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private Button btnApplyFilter;
        private Button btnClose;

        public InventoryHistoryForm(int ingredientId)
        {
            _ingredientId = ingredientId;
            InitializeComponent();
            LoadIngredientName();
            LoadHistoryData();
        }

        private void InitializeComponent()
        {
            this.lblTitle = new Label();
            this.dataGridViewHistory = new DataGridView();
            this.cmbFilterType = new ComboBox();
            this.dtpStart = new DateTimePicker();
            this.dtpEnd = new DateTimePicker();
            this.btnApplyFilter = new Button();
            this.btnClose = new Button();

            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(920, 520);
            this.Text = "Inventory History";
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Title
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Text = "Inventory History";

            // Filter controls
            var lblFilter = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(22, 60),
                Text = "Type:"
            };

            this.cmbFilterType.Location = new System.Drawing.Point(70, 57);
            this.cmbFilterType.Size = new System.Drawing.Size(150, 21);
            this.cmbFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbFilterType.Items.AddRange(new object[]
            {
                "All",
                "addition",
                "removal",
                "waste",
                "correction",
                "adjustment",
                "purchase",
                "usage"
            });
            this.cmbFilterType.SelectedIndex = 0;

            var lblFrom = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(240, 60),
                Text = "From:"
            };

            this.dtpStart.Location = new System.Drawing.Point(285, 57);
            this.dtpStart.Format = DateTimePickerFormat.Short;

            var lblTo = new Label
            {
                AutoSize = true,
                Location = new System.Drawing.Point(460, 60),
                Text = "To:"
            };

            this.dtpEnd.Location = new System.Drawing.Point(490, 57);
            this.dtpEnd.Format = DateTimePickerFormat.Short;

            this.btnApplyFilter.Location = new System.Drawing.Point(680, 55);
            this.btnApplyFilter.Size = new System.Drawing.Size(100, 25);
            this.btnApplyFilter.Text = "Apply Filter";
            this.btnApplyFilter.Click += btnApplyFilter_Click;

            this.btnClose.Location = new System.Drawing.Point(800, 55);
            this.btnClose.Size = new System.Drawing.Size(80, 25);
            this.btnClose.Text = "Close";
            this.btnClose.Click += btnClose_Click;

            // DataGridView
            this.dataGridViewHistory.Location = new System.Drawing.Point(20, 100);
            this.dataGridViewHistory.Size = new System.Drawing.Size(880, 390);
            this.dataGridViewHistory.ReadOnly = true;
            this.dataGridViewHistory.AllowUserToAddRows = false;
            this.dataGridViewHistory.AllowUserToDeleteRows = false;
            this.dataGridViewHistory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewHistory.MultiSelect = false;
            this.dataGridViewHistory.RowHeadersVisible = false;
            this.dataGridViewHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ConfigureGridColumns();

            this.Controls.Add(this.lblTitle);
            this.Controls.Add(lblFilter);
            this.Controls.Add(this.cmbFilterType);
            this.Controls.Add(lblFrom);
            this.Controls.Add(this.dtpStart);
            this.Controls.Add(lblTo);
            this.Controls.Add(this.dtpEnd);
            this.Controls.Add(this.btnApplyFilter);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dataGridViewHistory);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void ConfigureGridColumns()
        {
            string currencySymbol = AppSettings.CurrencySymbol ?? "$";
            var currencyFormat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
            currencyFormat.CurrencySymbol = currencySymbol;

            this.dataGridViewHistory.Columns.Clear();

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MovementDate",
                HeaderText = "Date",
                DataPropertyName = "ChangeDate",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle { Format = "g" }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "MovementType",
                HeaderText = "Type",
                DataPropertyName = "ChangeType",
                ReadOnly = true
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "PreviousStock",
                HeaderText = "Previous",
                DataPropertyName = "PreviousStock",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "NewStock",
                HeaderText = "New",
                DataPropertyName = "NewStock",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "QuantityChange",
                HeaderText = "Change",
                DataPropertyName = "ChangeAmount",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "+0.##;-0.##",
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UnitCost",
                HeaderText = $"Unit Cost ({currencySymbol})",
                DataPropertyName = "UnitCost",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C4",
                    FormatProvider = currencyFormat,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ValueChange",
                HeaderText = "Value Change",
                DataPropertyName = "ValueChange",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "C2",
                    FormatProvider = currencyFormat,
                    Alignment = DataGridViewContentAlignment.MiddleRight
                }
            });

            this.dataGridViewHistory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Reason",
                HeaderText = "Reason",
                DataPropertyName = "Reason",
                ReadOnly = true
            });
        }

        private void LoadIngredientName()
        {
            try
            {
                var list = DatabaseContext.GetInventoryLevels();
                var item = list.FirstOrDefault(i => i.IngredientId == _ingredientId);
                if (item != null)
                {
                    this.lblTitle.Text = $"Inventory History - {item.IngredientName}";
                    this.Text = $"Inventory History - {item.IngredientName} - CostChef v3.0";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading ingredient name: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadHistoryData()
        {
            try
            {
                string type = cmbFilterType.SelectedItem?.ToString() ?? "All";
                DateTime? start = dtpStart.Value.Date;
                DateTime? end = dtpEnd.Value.Date;

                var history = DatabaseContext.GetInventoryHistory(
                    _ingredientId,
                    type,
                    start,
                    end);

                this.dataGridViewHistory.DataSource = history;

                // Row coloring: green for positive, red for negative
                foreach (DataGridViewRow row in this.dataGridViewHistory.Rows)
                {
                    if (row.DataBoundItem is InventoryHistory h)
                    {
                        if (h.ChangeAmount > 0)
                            row.DefaultCellStyle.BackColor = Color.LightGreen;
                        else if (h.ChangeAmount < 0)
                            row.DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading history: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnApplyFilter_Click(object sender, EventArgs e)
        {
            LoadHistoryData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
