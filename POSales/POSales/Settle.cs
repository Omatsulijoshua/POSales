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
        private string cashEntryString = "";
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

            txtVatable.Text = cashier.lblVatable.Text;
            txtVat.Text = cashier.lblVat.Text;

            string vatType = dbcon.GetVatType();
            Label lblAmount = Controls.Find("lblAmount", true).OfType<Label>().FirstOrDefault();
            Label lblVat = Controls.Find("lblVat", true).OfType<Label>().FirstOrDefault();
            Label lblCash = Controls.Find("lblCash", true).OfType<Label>().FirstOrDefault();
            Label lblChange = Controls.Find("lblChange", true).OfType<Label>().FirstOrDefault();

            if (vatType == "New")
            {
                txtVatable.Visible = true;
                txtVat.Visible = true;
                if (lblAmount != null) lblAmount.Visible = true;
                if (lblVat != null) lblVat.Visible = true;
            }
            else
            {
                txtVatable.Visible = false;
                txtVat.Visible = false;
                if (lblAmount != null) lblAmount.Visible = false;
                if (lblVat != null) lblVat.Visible = false;

                this.ClientSize = new Size(330, 500);
                this.MinimumSize = new Size(330, 500);
                this.MaximumSize = new Size(330, 500);

                txtCash.Top = txtSale.Top + 40;
                if (lblCash != null) lblCash.Top = txtSale.Top + 40;
                txtChange.Top = txtSale.Top + 80;
                if (lblChange != null) lblChange.Top = txtSale.Top + 80;

                Label paymentLabel = Controls.Find("lblPaymentType", true).OfType<Label>().FirstOrDefault();
                if (paymentLabel != null) paymentLabel.Top = txtSale.Top + 118;

                ComboBox paymentType = Controls.Find("cboPaymentType", true).OfType<ComboBox>().FirstOrDefault();
                if (paymentType != null) paymentType.Top = txtSale.Top + 142;

                int keypadTop = txtSale.Top + 192;
                int key = 64;
                int gap = 8;
                int margin = 18;
                int[] xs = { margin, margin + key + gap, margin + (key + gap) * 2, margin + (key + gap) * 3 };

                Button seven = Controls.Find("btnSeven", true).OfType<Button>().FirstOrDefault();
                Button eight = Controls.Find("btnEight", true).OfType<Button>().FirstOrDefault();
                Button nine = Controls.Find("btnNine", true).OfType<Button>().FirstOrDefault();
                Button clear = Controls.Find("btnClear", true).OfType<Button>().FirstOrDefault();
                Button four = Controls.Find("btnFour", true).OfType<Button>().FirstOrDefault();
                Button five = Controls.Find("btnFive", true).OfType<Button>().FirstOrDefault();
                Button six = Controls.Find("btnSix", true).OfType<Button>().FirstOrDefault();
                Button zero = Controls.Find("btnZero", true).OfType<Button>().FirstOrDefault();
                Button one = Controls.Find("btnOne", true).OfType<Button>().FirstOrDefault();
                Button two = Controls.Find("btnTwo", true).OfType<Button>().FirstOrDefault();
                Button three = Controls.Find("btnThree", true).OfType<Button>().FirstOrDefault();
                Button dZero = Controls.Find("btnDZero", true).OfType<Button>().FirstOrDefault();
                Button enter = Controls.Find("btnEnter", true).OfType<Button>().FirstOrDefault();

                if (seven != null) seven.SetBounds(xs[0], keypadTop, key, 52);
                if (eight != null) eight.SetBounds(xs[1], keypadTop, key, 52);
                if (nine != null) nine.SetBounds(xs[2], keypadTop, key, 52);
                if (clear != null) clear.SetBounds(xs[3], keypadTop, key, 52);
                if (four != null) four.SetBounds(xs[0], keypadTop + 58, key, 52);
                if (five != null) five.SetBounds(xs[1], keypadTop + 58, key, 52);
                if (six != null) six.SetBounds(xs[2], keypadTop + 58, key, 52);
                if (zero != null) zero.SetBounds(xs[3], keypadTop + 58, key, 52);
                if (one != null) one.SetBounds(xs[0], keypadTop + 116, key, 52);
                if (two != null) two.SetBounds(xs[1], keypadTop + 116, key, 52);
                if (three != null) three.SetBounds(xs[2], keypadTop + 116, key, 52);
                if (dZero != null) dZero.SetBounds(xs[3], keypadTop + 116, key, 52);

                if (enter != null) enter.SetBounds(margin, keypadTop + 178, this.ClientSize.Width - margin * 2, 52);
            }
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
            AppendCash(".");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            cashEntryString = "";
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

            cashEntryString = ExtractDecimalDigits(txtCash.Text);
            SetCashDisplay();
        }

        private void txtCash_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) || e.KeyChar == '.')
            {
                AppendCash(e.KeyChar.ToString());
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)Keys.Back)
            {
                if (cashEntryString.Length > 0)
                    cashEntryString = cashEntryString.Substring(0, cashEntryString.Length - 1);

                SetCashDisplay();
                e.Handled = true;
                return;
            }

            if (e.KeyChar != (char)Keys.Enter)
                e.Handled = true;
        }

        private void AppendCash(string value)
        {
            if (value == ".")
            {
                if (!cashEntryString.Contains("."))
                {
                    if (string.IsNullOrEmpty(cashEntryString))
                        cashEntryString = "0.";
                    else
                        cashEntryString += ".";
                }
            }
            else
            {
                string digits = new string(value.Where(char.IsDigit).ToArray());
                cashEntryString += digits;
            }
            SetCashDisplay();
            txtCash.Focus();
        }

        private void SetCashDisplay()
        {
            double cash = 0;
            if (!string.IsNullOrWhiteSpace(cashEntryString))
            {
                double.TryParse(cashEntryString, NumberStyles.Any, CultureInfo.InvariantCulture, out cash);
            }

            updatingCashText = true;

            if (cashEntryString.Contains("."))
            {
                int dotIndex = cashEntryString.IndexOf('.');
                string wholePart = cashEntryString.Substring(0, dotIndex);
                string decimalPart = cashEntryString.Substring(dotIndex + 1);

                double wholeVal = 0;
                double.TryParse(wholePart, out wholeVal);

                string formattedWhole = wholeVal.ToString("#,##0");
                if (string.IsNullOrEmpty(formattedWhole)) formattedWhole = "0";

                txtCash.Text = formattedWhole + "." + decimalPart;
            }
            else
            {
                txtCash.Text = cash.ToString("#,##0.00");
            }

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

        private static string ExtractDecimalDigits(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            StringBuilder sb = new StringBuilder();
            bool hasDot = false;
            foreach (char c in value)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);
                }
                else if (c == '.' && !hasDot)
                {
                    sb.Append(c);
                    hasDot = true;
                }
            }
            string res = sb.ToString();
            if (res.StartsWith("."))
                res = "0" + res;
            else if (res.Length > 1 && res.StartsWith("0") && !res.StartsWith("0."))
                res = res.TrimStart('0');

            return res;
        }

        private void Settle_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) this.Dispose();
            else if (e.KeyCode == Keys.Enter) btnEnter.PerformClick();            
        }
    }
}
