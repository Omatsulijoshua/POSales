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
    public partial class Void : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();
        SqlDataReader dr;
        CancelOrder cancelOrder;
        public Void(CancelOrder cancel)
        {
            InitializeComponent();
            ModernUI.Apply(this);
            cn = new SqlConnection(dbcon.myConnection());        
            cancelOrder = cancel;
        }

        private void btnVoid_Click(object sender, EventArgs e)
        {
            try
            {
                string user = "";
                cn.Open();
                cm = new SqlCommand("Select * From tbUser Where username = @username and password = @password", cn);
                cm.Parameters.AddWithValue("@username", txtUsername.Text);
                cm.Parameters.AddWithValue("@password", txtPass.Text);
                dr = cm.ExecuteReader();
                dr.Read();
                if (dr.HasRows)
                {
                    user = dr["username"].ToString();
                }
                dr.Close();
                cn.Close();

                if (user == "")
                {
                    MessageBox.Show("Invalid username or password.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int soldQty = int.Parse(cancelOrder.txtQty.Text);
                int cancelQty = (int)cancelOrder.udCancelQty.Value;
                double soldTotal = double.Parse(cancelOrder.txtTotal.Text);
                double cancelTotal = soldQty <= 0 ? 0 : soldTotal / soldQty * cancelQty;
                int remainingQty = soldQty - cancelQty;
                double remainingTotal = soldTotal - cancelTotal;

                SaveCancelOrder(user, cancelQty, cancelTotal);

                if (cancelOrder.cboInventory.Text.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    cn.Open();
                    cm = new SqlCommand("UPDATE tbProduct SET qty = qty + @qty WHERE pcode = @pcode", cn);
                    cm.Parameters.AddWithValue("@qty", cancelQty);
                    cm.Parameters.AddWithValue("@pcode", cancelOrder.txtPcode.Text);
                    cm.ExecuteNonQuery();
                    cn.Close();
                }

                cn.Open();
                if (remainingQty <= 0)
                {
                    cm = new SqlCommand("UPDATE tbCart SET qty = 0, total = 0, status = 'Cancelled' WHERE id = @id", cn);
                }
                else
                {
                    cm = new SqlCommand("UPDATE tbCart SET qty = @qty, total = @total WHERE id = @id", cn);
                    cm.Parameters.AddWithValue("@qty", remainingQty);
                    cm.Parameters.AddWithValue("@total", remainingTotal);
                }
                cm.Parameters.AddWithValue("@id", cancelOrder.txtId.Text);
                cm.ExecuteNonQuery();
                cn.Close();

                MessageBox.Show("Order transaction successfully cancelled!", "Cancel Order", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
                cancelOrder.ReloadSoldList();
                cancelOrder.Dispose();
            }
            catch (Exception ex)
            {
                cn.Close();
                MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        public void SaveCancelOrder(string user, int cancelQty, double cancelTotal)
        {
            try
            {
                cn.Open();
                cm = new SqlCommand("insert into tbCancel (transno, pcode, price, qty, total, sdate, voidby, cancelledby, reason, action) values (@transno, @pcode, @price, @qty, @total, @sdate, @voidby, @cancelledby, @reason, @action)", cn);
                cm.Parameters.AddWithValue("@transno", cancelOrder.txtTransno.Text);
                cm.Parameters.AddWithValue("@pcode", cancelOrder.txtPcode.Text);
                cm.Parameters.AddWithValue("@price",double.Parse(cancelOrder.txtPrice.Text));
                cm.Parameters.AddWithValue("@qty", cancelQty);
                cm.Parameters.AddWithValue("@total", cancelTotal);
                cm.Parameters.AddWithValue("@sdate", DateTime.Now);
                cm.Parameters.AddWithValue("@voidby", user);
                cm.Parameters.AddWithValue("@cancelledby", cancelOrder.txtCancelBy.Text);
                cm.Parameters.AddWithValue("@reason", cancelOrder.txtReason.Text);
                cm.Parameters.AddWithValue("@action", cancelOrder.cboInventory.Text);
                cm.ExecuteNonQuery();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void picClose_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void Void_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}
