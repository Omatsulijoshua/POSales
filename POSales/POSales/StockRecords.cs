using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace POSales
{
    public class StockRecords : Form
    {
        private readonly DBConnect dbcon = new DBConnect();
        private readonly SqlConnection cn;
        private readonly DataGridView dgvRecords = new DataGridView();
        private readonly DateTimePicker dtFrom = new DateTimePicker();
        private readonly DateTimePicker dtTo = new DateTimePicker();
        private readonly Button btnLoad = new Button();
        private readonly Label lblTotal = new Label();

        public StockRecords()
        {
            cn = new SqlConnection(dbcon.myConnection());
            InitializeComponent();
            ModernUI.Apply(this);
            LoadStockRecords();
        }

        private void InitializeComponent()
        {
            Text = "STOCK RECORDS";
            Name = "StockRecords";
            BackColor = Color.White;
            Font = new Font("Century Gothic", 11.25F, FontStyle.Regular);
            FormBorderStyle = FormBorderStyle.None;

            Panel topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.White,
                Padding = new Padding(22, 16, 22, 14)
            };

            Label filterLabel = new Label
            {
                AutoSize = true,
                Text = "Filter By Date : From",
                Location = new Point(22, 28)
            };

            dtFrom.Format = DateTimePickerFormat.Short;
            dtFrom.Location = new Point(178, 24);
            dtFrom.Width = 116;

            Label toLabel = new Label
            {
                AutoSize = true,
                Text = "To",
                Location = new Point(308, 28)
            };

            dtTo.Format = DateTimePickerFormat.Short;
            dtTo.Location = new Point(338, 24);
            dtTo.Width = 116;

            btnLoad.Text = "Load Data";
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.Location = new Point(474, 18);
            btnLoad.Size = new Size(128, 38);
            btnLoad.Click += btnLoad_Click;

            lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotal.Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold);
            lblTotal.TextAlign = ContentAlignment.MiddleRight;
            lblTotal.Location = new Point(682, 22);
            lblTotal.Size = new Size(270, 32);
            lblTotal.Text = "Amount: 0.00";
            topPanel.Resize += topPanel_Resize;

            topPanel.Controls.Add(filterLabel);
            topPanel.Controls.Add(dtFrom);
            topPanel.Controls.Add(toLabel);
            topPanel.Controls.Add(dtTo);
            topPanel.Controls.Add(btnLoad);
            topPanel.Controls.Add(lblTotal);

            dgvRecords.AllowUserToAddRows = false;
            dgvRecords.AllowUserToResizeRows = false;
            dgvRecords.BackgroundColor = Color.White;
            dgvRecords.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvRecords.Dock = DockStyle.Fill;
            dgvRecords.EnableHeadersVisualStyles = false;
            dgvRecords.RowHeadersVisible = false;
            dgvRecords.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvRecords.Columns.Add(CreateColumn("No", "No", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("RecordType", "Type", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("ReferenceNo", "Reference#", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Pcode", "Pcode", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Description", "Description", DataGridViewAutoSizeColumnMode.Fill));
            dgvRecords.Columns.Add(CreateColumn("Action", "Action", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Qty", "Qty", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Price", "Price", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Amount", "Amount", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("RecordDate", "Date / Time", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("RecordedBy", "Recorded By", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Supplier", "Supplier", DataGridViewAutoSizeColumnMode.AllCells));
            dgvRecords.Columns.Add(CreateColumn("Remarks", "Remarks", DataGridViewAutoSizeColumnMode.Fill));

            Controls.Add(dgvRecords);
            Controls.Add(topPanel);
            ClientSize = new Size(984, 561);
        }

        private DataGridViewTextBoxColumn CreateColumn(string name, string header, DataGridViewAutoSizeColumnMode mode)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                HeaderText = header,
                AutoSizeMode = mode
            };
        }

        private void topPanel_Resize(object sender, EventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel != null)
                lblTotal.SetBounds(Math.Max(620, panel.ClientSize.Width - 302), 22, 270, 32);
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            LoadStockRecords();
        }

        private void LoadStockRecords()
        {
            try
            {
                dgvRecords.Rows.Clear();
                decimal grandTotal = 0;
                int i = 0;

                cn.Open();
                SqlCommand cm = new SqlCommand(@"
                    SELECT RecordType, ReferenceNo, Pcode, Description, Action, Qty, Price, Amount, RecordDate, RecordedBy, Supplier, Remarks
                    FROM (
                        SELECT
                            'Stock In' AS RecordType,
                            s.refno AS ReferenceNo,
                            s.pcode AS Pcode,
                            p.pdesc AS Description,
                            'Stock In' AS Action,
                            ISNULL(s.qty, 0) AS Qty,
                            ISNULL(p.price, 0) AS Price,
                            ISNULL(s.qty, 0) * ISNULL(p.price, 0) AS Amount,
                            s.sdate AS RecordDate,
                            s.stockinby AS RecordedBy,
                            ISNULL(sp.supplier, '') AS Supplier,
                            '' AS Remarks
                        FROM tbStockIn AS s
                        LEFT JOIN tbProduct AS p ON p.pcode = s.pcode
                        LEFT JOIN tbSupplier AS sp ON sp.id = s.supplierid
                        WHERE CAST(s.sdate AS date) BETWEEN @from AND @to
                            AND s.status LIKE 'Done'

                        UNION ALL

                        SELECT
                            'Adjustment' AS RecordType,
                            a.referenceno AS ReferenceNo,
                            a.pcode AS Pcode,
                            p.pdesc AS Description,
                            a.action AS Action,
                            ISNULL(a.qty, 0) AS Qty,
                            ISNULL(p.price, 0) AS Price,
                            ISNULL(a.qty, 0) * ISNULL(p.price, 0) AS Amount,
                            a.sdate AS RecordDate,
                            a.[user] AS RecordedBy,
                            '' AS Supplier,
                            a.remarks AS Remarks
                        FROM tbAdjustment AS a
                        LEFT JOIN tbProduct AS p ON p.pcode = a.pcode
                        WHERE CAST(a.sdate AS date) BETWEEN @from AND @to
                    ) AS StockActivity
                    ORDER BY RecordDate DESC, ReferenceNo DESC", cn);

                cm.Parameters.Add("@from", SqlDbType.Date).Value = dtFrom.Value.Date;
                cm.Parameters.Add("@to", SqlDbType.Date).Value = dtTo.Value.Date;

                SqlDataReader dr = cm.ExecuteReader();
                while (dr.Read())
                {
                    i++;
                    decimal amount = ToDecimal(dr["Amount"]);
                    grandTotal += amount;

                    dgvRecords.Rows.Add(
                        i,
                        dr["RecordType"].ToString(),
                        dr["ReferenceNo"].ToString(),
                        dr["Pcode"].ToString(),
                        dr["Description"].ToString(),
                        dr["Action"].ToString(),
                        dr["Qty"].ToString(),
                        FormatMoney(dr["Price"]),
                        amount.ToString("#,##0.00"),
                        FormatDateTime(dr["RecordDate"]),
                        dr["RecordedBy"].ToString(),
                        dr["Supplier"].ToString(),
                        dr["Remarks"].ToString());
                }
                dr.Close();
                cn.Close();

                lblTotal.Text = "Amount: " + grandTotal.ToString("#,##0.00");
            }
            catch (Exception ex)
            {
                if (cn.State == ConnectionState.Open)
                    cn.Close();

                MessageBox.Show(ex.Message, "Stock Records", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value)
                return 0;

            decimal result;
            return decimal.TryParse(value.ToString(), out result) ? result : 0;
        }

        private string FormatMoney(object value)
        {
            return ToDecimal(value).ToString("#,##0.00");
        }

        private string FormatDateTime(object value)
        {
            if (value == null || value == DBNull.Value)
                return "";

            return DateTime.Parse(value.ToString()).ToString("dd-MMM-yyyy hh:mm tt");
        }
    }
}
