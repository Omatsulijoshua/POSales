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

namespace POSales
{
    public partial class Store : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        bool havestoreinfo = false;
        public Store()
        {
            InitializeComponent();
            ModernUI.Apply(this);
            cn = new SqlConnection(dbcon.myConnection());
            LoadStore();
        }

        public void LoadStore()
        {
            try
            {
                cn.Open();
                cm = new SqlCommand("SELECT * FROM tbStore", cn);
                dr = cm.ExecuteReader();
                if (dr.Read())
                {
                    havestoreinfo = true;
                    txtStName.Text = dr["store"].ToString();
                    txtAddress.Text = dr["address"].ToString();
                    cboVatType.Text = dr["vat_type"] != DBNull.Value && !string.IsNullOrEmpty(dr["vat_type"].ToString()) ? dr["vat_type"].ToString() : "Old";
                    txtVatPercent.Text = dr["vat_percent"] != DBNull.Value ? Convert.ToDouble(dr["vat_percent"]).ToString("0.00") : "12.00";
                }
                else
                {
                    txtStName.Clear();
                    txtAddress.Clear();
                    cboVatType.SelectedIndex = 0;
                    txtVatPercent.Text = "12.00";
                }
                dr.Close();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Save store details?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    double vatPercent = 12.00;
                    double.TryParse(txtVatPercent.Text, out vatPercent);

                    cn.Open();
                    if(havestoreinfo)
                    {
                        cm = new SqlCommand("UPDATE tbStore SET store = @store, address = @address, vat_type = @vat_type, vat_percent = @vat_percent", cn);
                    }
                    else
                    {
                        cm = new SqlCommand("INSERT INTO tbStore (store, address, vat_type, vat_percent) VALUES (@store, @address, @vat_type, @vat_percent)", cn);
                    }
                    cm.Parameters.AddWithValue("@store", txtStName.Text);
                    cm.Parameters.AddWithValue("@address", txtAddress.Text);
                    cm.Parameters.AddWithValue("@vat_type", cboVatType.Text);
                    cm.Parameters.AddWithValue("@vat_percent", vatPercent);
                    cm.ExecuteNonQuery();
                    cn.Close();

                    MessageBox.Show("Store detail has been successfully saved!", "Save Record", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                if (cn.State == ConnectionState.Open) cn.Close();
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void Store_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.Escape)
            { this.Dispose(); }
        }
    }
}
