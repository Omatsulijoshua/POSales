using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace POSales
{
    public partial class Product : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        public Product()
        {
            InitializeComponent();
            ModernUI.Apply(this);
            cn = new SqlConnection(dbcon.myConnection());
            LoadProduct();
        }

        public void LoadProduct()
        {
            int i = 0;
            dgvProduct.Rows.Clear();
            cm = new SqlCommand("SELECT p.pcode, p.barcode, p.pdesc, b.brand, c.category, p.price, p.reorder FROM tbProduct AS p INNER JOIN tbBrand AS b ON b.id = p.bid INNER JOIN tbCategory AS c on c.id = p.cid WHERE CONCAT(p.pdesc, b.brand, c.category) LIKE '%" +txtSearch.Text+ "%'",cn);
            cn.Open();
            dr = cm.ExecuteReader();
            while (dr.Read())
            {
                i++;
                dgvProduct.Rows.Add(i, dr[0].ToString(), dr[1].ToString(), dr[2].ToString(), dr[3].ToString(), dr[4].ToString(), dr[5].ToString(), dr[6].ToString());
            }
            dr.Close();
            cn.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            ProductModule productModule = new ProductModule(this);
            productModule.ShowDialog();
        }

        private void dgvProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string colName = dgvProduct.Columns[e.ColumnIndex].Name;

            if (colName == "Edit")
            {
                ProductModule product = new ProductModule(this);
                product.txtPcode.Text = dgvProduct.Rows[e.RowIndex].Cells[1].Value.ToString();
                product.txtBarcode.Text = dgvProduct.Rows[e.RowIndex].Cells[2].Value.ToString();
                product.txtPdesc.Text = dgvProduct.Rows[e.RowIndex].Cells[3].Value.ToString();
                product.cboBrand.Text = dgvProduct.Rows[e.RowIndex].Cells[4].Value.ToString();
                product.cboCategory.Text = dgvProduct.Rows[e.RowIndex].Cells[5].Value.ToString();
                product.txtPrice.Text = dgvProduct.Rows[e.RowIndex].Cells[6].Value.ToString();
                product.UDReOrder.Value = int.Parse(dgvProduct.Rows[e.RowIndex].Cells[7].Value.ToString());

                product.txtPcode.Enabled = false;
                product.btnSave.Enabled = false;
                product.btnUpdate.Enabled = true;
                product.ShowDialog();
            }
            else if (colName == "Delete")
            {
                if (MessageBox.Show("Are you sure you want to delete this record?",
                    "Delete Record", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    cn.Open();
                    cm = new SqlCommand("UPDATE tbProduct SET IsActive = 0 WHERE pcode = @pcode", cn);
                    cm.Parameters.AddWithValue("@pcode", dgvProduct[1, e.RowIndex].Value.ToString());
                    cm.ExecuteNonQuery();
                    cn.Close();

                    MessageBox.Show("Product has been successfully deleted (soft delete).",
                        "Point Of Sales", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }

            // Reload product list to reflect changes
            LoadProduct();
        }


        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProduct();
        }

        private void Product_Load(object sender, EventArgs e)
        {

        }

        private void btn_add_multiple_Click(object sender, EventArgs e)
        {
            try
            {
                // Select CSV file
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                if (openFileDialog.ShowDialog() != DialogResult.OK) return;

                string[] lines = File.ReadAllLines(openFileDialog.FileName);
                int inserted = 0, skipped = 0;

                cn.Open();

                foreach (string line in lines.Skip(1)) // skip header
                {
                    string[] data = line.Split(',');

                    if (data.Length < 7)
                    {
                        skipped++;
                        MessageBox.Show("Skipped row (not enough columns): " + line);
                        continue;
                    }

                    string pcode = data[0].Trim();
                    string barcode = data[1].Trim();
                    string pdesc = data[2].Trim();
                    string brandName = data[3].Trim();
                    string categoryName = data[4].Trim();

                    if (!decimal.TryParse(data[5], out decimal price))
                    {
                        skipped++;
                        MessageBox.Show($"Invalid price in row: {line}");
                        continue;
                    }

                    if (!int.TryParse(data[6], out int reorder))
                    {
                        skipped++;
                        MessageBox.Show($"Invalid reorder value in row: {line}");
                        continue;
                    }

                    // Get Brand ID (case-insensitive)
                    SqlCommand getBrand = new SqlCommand("SELECT id FROM tbBrand WHERE LOWER(brand) = LOWER(@brand)", cn);
                    getBrand.Parameters.AddWithValue("@brand", brandName);
                    object brandIdObj = getBrand.ExecuteScalar();
                    if (brandIdObj == null)
                    {
                        skipped++;
                        MessageBox.Show("Brand not found: " + brandName);
                        continue;
                    }
                    int bid = Convert.ToInt32(brandIdObj);

                    // Get Category ID (case-insensitive)
                    SqlCommand getCategory = new SqlCommand("SELECT id FROM tbCategory WHERE LOWER(category) = LOWER(@category)", cn);
                    getCategory.Parameters.AddWithValue("@category", categoryName);
                    object categoryIdObj = getCategory.ExecuteScalar();
                    if (categoryIdObj == null)
                    {
                        skipped++;
                        MessageBox.Show("Category not found: " + categoryName);
                        continue;
                    }
                    int cid = Convert.ToInt32(categoryIdObj);

                    // Insert Product
                    SqlCommand insertCmd = new SqlCommand(@"
                INSERT INTO tbProduct 
                (pcode, barcode, pdesc, bid, cid, price, reorder, IsActive) 
                VALUES (@pcode, @barcode, @pdesc, @bid, @cid, @price, @reorder, 1)", cn);

                    insertCmd.Parameters.AddWithValue("@pcode", pcode);
                    insertCmd.Parameters.AddWithValue("@barcode", barcode);
                    insertCmd.Parameters.AddWithValue("@pdesc", pdesc);
                    insertCmd.Parameters.AddWithValue("@bid", bid);
                    insertCmd.Parameters.AddWithValue("@cid", cid);
                    insertCmd.Parameters.AddWithValue("@price", price);
                    insertCmd.Parameters.AddWithValue("@reorder", reorder);

                    insertCmd.ExecuteNonQuery();
                    inserted++;
                }

                cn.Close();

                MessageBox.Show($"{inserted} products inserted successfully. {skipped} rows skipped.",
                    "Bulk Insert", MessageBoxButtons.OK, MessageBoxIcon.Information);

                LoadProduct(); // refresh grid
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Error while inserting products: " + ex.Message,
                    "Bulk Insert Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void sample_file_download_Click(object sender, EventArgs e)
        {
            try
            {
                // Choose save location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                saveFileDialog.FileName = "Product_Sample.csv";

                if (saveFileDialog.ShowDialog() != DialogResult.OK) return;

                // Fetch existing brands and categories from DB
                List<string> brands = new List<string>();
                List<string> categories = new List<string>();

                cn.Open();

                SqlCommand brandCmd = new SqlCommand("SELECT brand FROM tbBrand", cn);
                SqlDataReader br = brandCmd.ExecuteReader();
                while (br.Read()) brands.Add(br[0].ToString());
                br.Close();

                SqlCommand catCmd = new SqlCommand("SELECT category FROM tbCategory", cn);
                SqlDataReader cr = catCmd.ExecuteReader();
                while (cr.Read()) categories.Add(cr[0].ToString());
                cr.Close();

                cn.Close();

                // Build CSV content
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("pcode,barcode,pdesc,brand,category,price,reorder");

                // Example row using first brand and category if available
                string exampleBrand = brands.Count > 0 ? brands[0] : "BrandName";
                string exampleCategory = categories.Count > 0 ? categories[0] : "CategoryName";

                sb.AppendLine($"P001,1234567890,Sample Product,{exampleBrand},{exampleCategory},100,10");
                sb.AppendLine($"P002,0987654321,Another Product,{exampleBrand},{exampleCategory},150,20");

                // Save file
                File.WriteAllText(saveFileDialog.FileName, sb.ToString());

                MessageBox.Show("Sample CSV file created successfully!",
                    "Sample File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show("Error creating sample CSV: " + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
