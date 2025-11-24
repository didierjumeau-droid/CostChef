using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace CostChef
{
    public partial class IngredientsForm : Form
    {
        private DataGridView dataGridViewIngredients;
        private Button btnClose;
        private Button btnRefresh;
        private Button btnAddIngredient;
        private Button btnEditIngredient;
        private Label lblSummary;

        private BindingSource _ingredients = new BindingSource();

        public IngredientsForm()
        {
            InitializeComponent();
            LoadIngredientData();
        }

        private void InitializeComponent()
        {
            this.dataGridViewIngredients = new DataGridView();
            this.btnClose = new Button();
            this.btnRefresh = new Button();
            this.btnAddIngredient = new Button();
            this.btnEditIngredient = new Button();
            this.lblSummary = new Label();

            this.SuspendLayout();

            // Data grid
            this.dataGridViewIngredients.Location = new Point(12, 12);
            this.dataGridViewIngredients.Size = new Size(780, 350);
            this.dataGridViewIngredients.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            this.dataGridViewIngredients.ReadOnly = true;
            this.dataGridViewIngredients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewIngredients.MultiSelect = false;
            this.dataGridViewIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewIngredients.AllowUserToAddRows = false;
            this.dataGridViewIngredients.AllowUserToDeleteRows = false;

            // Refresh button
            this.btnRefresh.Text = "Refresh";
            this.btnRefresh.Location = new Point(12, 380);
            this.btnRefresh.Click += btnRefresh_Click;

            // Add Ingredient button
            this.btnAddIngredient.Text = "Add";
            this.btnAddIngredient.Location = new Point(110, 380);
            this.btnAddIngredient.Click += btnAddIngredient_Click;

            // Edit Ingredient button
            this.btnEditIngredient.Text = "Edit";
            this.btnEditIngredient.Location = new Point(200, 380);
            this.btnEditIngredient.Click += btnEditIngredient_Click;

            // Close button
            this.btnClose.Text = "Close";
            this.btnClose.Location = new Point(700, 380);
            this.btnClose.Click += btnClose_Click;

            // Summary label
            this.lblSummary.Location = new Point(300, 380);
            this.lblSummary.AutoSize = true;

            // Form
            this.ClientSize = new Size(800, 430);
            this.Controls.Add(this.dataGridViewIngredients);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.btnAddIngredient);
            this.Controls.Add(this.btnEditIngredient);
            this.Controls.Add(this.lblSummary);

            this.Text = "Ingredients";
            this.StartPosition = FormStartPosition.CenterParent;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void LoadIngredientData()
        {
            // Use the full ingredients list from DB
            var ingredients = DatabaseContext.GetAllIngredients();

            _ingredients.DataSource = ingredients;
            dataGridViewIngredients.DataSource = _ingredients;

            // Hide technical columns (ID, SupplierId)
            if (dataGridViewIngredients.Columns.Contains("Id"))
                dataGridViewIngredients.Columns["Id"].Visible = false;

            if (dataGridViewIngredients.Columns.Contains("SupplierId"))
                dataGridViewIngredients.Columns["SupplierId"].Visible = false;

            // NEW: align Unit & UnitPrice to the right
            if (dataGridViewIngredients.Columns.Contains("Unit"))
            {
                dataGridViewIngredients.Columns["Unit"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            if (dataGridViewIngredients.Columns.Contains("UnitPrice"))
            {
                var col = dataGridViewIngredients.Columns["UnitPrice"];
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                col.DefaultCellStyle.Format = "N2"; // 2 decimals, no extra dependencies
            }

            lblSummary.Text = $"Total Ingredients: {ingredients.Count}";
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadIngredientData();
        }

        private void btnAddIngredient_Click(object sender, EventArgs e)
        {
            var form = new IngredientEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadIngredientData();
            }
        }

        private void btnEditIngredient_Click(object sender, EventArgs e)
        {
            if (dataGridViewIngredients.SelectedRows.Count == 0)
                return;

            var item = (Ingredient)dataGridViewIngredients.SelectedRows[0].DataBoundItem;
            var form = new IngredientEditForm(item);
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadIngredientData();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
