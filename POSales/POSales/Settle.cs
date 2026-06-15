using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSales
{
    public partial class Settle : Form
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        DBConnect dbcon = new DBConnect();        
        Cashier cashier;
        private string cashEntryDigits = "";
        private bool updatingCashText;
        public Settle(Cashier cash)
        {
            InitializeComponent();
            ModernUI.Apply(this);
            cn = new SqlConnection(dbcon.myConnection());
            this.KeyPreview = true;
            cashier = cash;
            txtCash.KeyPress += txtCash_KeyPress;
            SetCashDisplay();
        }

        private ComboBox PaymentTypeBox
        {
            get { return Controls.Find("cboPaymentType", true).OfType<ComboBox>().FirstOrDefault(); }
        }

        private string SelectedPaymentType
        {
            get
            {
                ComboBox paymentType = PaymentTypeBox;
                return paymentType == null || string.IsNullOrWhiteSpace(paymentType.Text) ? "Cash" : paymentType.Text.Trim();
            }
        }

        private void EnsurePaymentTypeColumn()
        {
            using (SqlConnection connection = new SqlConnection(dbcon.myConnection()))
            using (SqlCommand command = new SqlCommand("IF COL_LENGTH('dbo.tbCart', 'paymenttype') IS NULL ALTER TABLE dbo.tbCart ADD paymenttype NVARCHAR(50) NULL", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private void btnOne_Click(object sender, EventArgs e)
        {
            AppendCash(btnOne.Text);
        }

        private void btnTwo_Click(object sender, EventArgs e)
        {
            AppendCash(btnTwo.Text);
        }

        private void btnThree_Click(object sender, EventArgs e)
        {
            AppendCash(btnThree.Text);
        }

        private void btnFour_Click(object sender, EventArgs e)
        {
            AppendCash(btnFour.Text);
        }

        private void btnFive_Click(object sender, EventArgs e)
        {
            AppendCash(btnFive.Text);
        }

        private void btnSix_Click(object sender, EventArgs e)
        {
            AppendCash(btnSix.Text);
        }

        private void btnSeven_Click(object sender, EventArgs e)
        {
            AppendCash(btnSeven.Text);
        }

        private void btnEight_Click(object sender, EventArgs e)
        {
            AppendCash(btnEight.Text);
        }

        private void btnNine_Click(object sender, EventArgs e)
        {
            AppendCash(btnNine.Text);
        }

        private void btnZero_Click(object sender, EventArgs e)
        {
            AppendCash(btnZero.Text);
        }

        private void btnDZero_Click(object sender, EventArgs e)
        {
            AppendCash(btnDZero.Text);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cashEntryDigits = "";
            SetCashDisplay();
            txtCash.Focus();
        }

        private void btnEnter_Click(object sender, EventArgs e)
        {
            try
            {
                SetCashDisplay();
                if ((ParseMoney(txtChange.Text) < 0) || (ParseMoney(txtCash.Text) <= 0))
                {
                    MessageBox.Show("Insufficient amount, Please enter the corret amount!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                else
                {
                    EnsurePaymentTypeColumn();
                    string paymentType = SelectedPaymentType;
                    for(int i=0; i< cashier.dgvCash.Rows.Count; i++ )
                    {
                        cn.Open();
                        cm = new SqlCommand("UPDATE tbProduct SET qty = qty - " + int.Parse(cashier.dgvCash.Rows[i].Cells[5].Value.ToString()) + "WHERE pcode= '" + cashier.dgvCash.Rows[i].Cells[2].Value.ToString() + "'", cn);
                        cm.ExecuteNonQuery();
                        cn.Close();

                        cn.Open();
                        cm = new SqlCommand("UPDATE tbCart SET status = 'Sold', paymenttype = @paymenttype WHERE id = @id", cn);
                        cm.Parameters.AddWithValue("@paymenttype", paymentType);
                        cm.Parameters.AddWithValue("@id", cashier.dgvCash.Rows[i].Cells[1].Value.ToString());
                        cm.ExecuteNonQuery();
                        cn.Close();
                    }
                    Recept recept = new Recept(cashier);
                    recept.LoadRecept(txtCash.Text, txtChange.Text, paymentType);
                    recept.ShowDialog();

                    MessageBox.Show("Payment successfully saved!", "Payment", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cashier.GetTranNo();
                    cashier.LoadCart();
                    this.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtCash_TextChanged(object sender, EventArgs e)
        {
            if (updatingCashText)
                return;

            cashEntryDigits = ExtractWholeNumberDigits(txtCash.Text);
            SetCashDisplay();
        }

        private void txtCash_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar))
            {
                AppendCash(e.KeyChar.ToString());
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Back)
            {
                if (cashEntryDigits.Length > 0)
                    cashEntryDigits = cashEntryDigits.Substring(0, cashEntryDigits.Length - 1);

                SetCashDisplay();
                e.Handled = true;
                return;
            }

            if (e.KeyChar != (char)Keys.Enter)
                e.Handled = true;
        }

        private void AppendCash(string value)
        {
            cashEntryDigits += new string(value.Where(char.IsDigit).ToArray());
            cashEntryDigits = cashEntryDigits.TrimStart('0');
            SetCashDisplay();
            txtCash.Focus();
        }

        private void SetCashDisplay()
        {
            double cash = 0;
            if (!string.IsNullOrWhiteSpace(cashEntryDigits))
                double.TryParse(cashEntryDigits, out cash);

            updatingCashText = true;
            txtCash.Text = cash.ToString("#,##0.00");
            txtCash.SelectionStart = txtCash.Text.Length;
            updatingCashText = false;
            UpdateChange();
        }

        private void UpdateChange()
        {
            try
            {
                double sale = ParseMoney(txtSale.Text);
                double cash = ParseMoney(txtCash.Text);
                txtChange.Text = (cash - sale).ToString("#,##0.00");
            }
            catch (Exception)
            {
                txtChange.Text = "0.00";
            }
        }

        private static double ParseMoney(string value)
        {
            double result;
            if (double.TryParse(value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.CurrentCulture, out result))
                return result;

            if (double.TryParse(value, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result))
                return result;

            return 0;
        }

        private static string ExtractWholeNumberDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            int decimalIndex = value.IndexOf('.');
            string whole = decimalIndex >= 0 ? value.Substring(0, decimalIndex) : value;
            return new string(whole.Where(char.IsDigit).ToArray()).TrimStart('0');
        }

        private void Settle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Dispose();
            else if (e.KeyCode == Keys.Enter) btnEnter.PerformClick();            
        }
    }
}
