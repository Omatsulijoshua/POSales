using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSales
{
    public partial class DbConfigForm : Form
    {
        DBConnect dbcon = new DBConnect();

        public DbConfigForm()
        {
            InitializeComponent();
            ModernUI.Apply(this);
        }

        private void DbConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                string connStr = dbcon.myConnection();
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connStr);

                txtServer.Text = builder.DataSource;
                txtDatabase.Text = builder.InitialCatalog;
                
                if (builder.IntegratedSecurity)
                {
                    cboAuth.SelectedIndex = 0; // Windows Authentication
                    txtUser.Text = "";
                    txtPassword.Text = "";
                }
                else
                {
                    cboAuth.SelectedIndex = 1; // SQL Server Authentication
                    txtUser.Text = builder.UserID;
                    txtPassword.Text = builder.Password;
                }

                chkEncrypt.Checked = builder.Encrypt;
                chkTrustCert.Checked = builder.TrustServerCertificate;
                
                UpdateUiState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading current database configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cboAuth.SelectedIndex = 0;
                UpdateUiState();
            }
        }

        private void cboAuth_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void UpdateUiState()
        {
            bool isSqlServerAuth = (cboAuth.SelectedIndex == 1);
            txtUser.Enabled = isSqlServerAuth;
            txtPassword.Enabled = isSqlServerAuth;
        }

        private string BuildConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = txtServer.Text.Trim();
            builder.InitialCatalog = txtDatabase.Text.Trim();
            
            if (cboAuth.SelectedIndex == 0)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = txtUser.Text;
                builder.Password = txtPassword.Text;
            }

            builder.Encrypt = chkEncrypt.Checked;
            builder.TrustServerCertificate = chkTrustCert.Checked;

            return builder.ConnectionString;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text.Trim()) || string.IsNullOrEmpty(txtDatabase.Text.Trim()))
            {
                MessageBox.Show("Please fill in Server Name and Database Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connStr = BuildConnectionString();
            using (SqlConnection testConn = new SqlConnection(connStr))
            {
                try
                {
                    btnTest.Enabled = false;
                    this.Cursor = Cursors.WaitCursor;
                    testConn.Open();
                    MessageBox.Show("Connection successfully established!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to connect: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    testConn.Close();
                    btnTest.Enabled = true;
                    this.Cursor = Cursors.Default;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text.Trim()) || string.IsNullOrEmpty(txtDatabase.Text.Trim()))
            {
                MessageBox.Show("Please fill in Server Name and Database Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connStr = BuildConnectionString();
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconnection.txt");
                File.WriteAllText(configPath, connStr);

                MessageBox.Show("Database configuration saved successfully!", "Configuration Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to save database configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void DbConfigForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.Dispose();
            }
        }
    }
}
