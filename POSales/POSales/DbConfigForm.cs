using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
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
        private ComboBox cboConnectionMode;
        private Label labelMode;
        private Label labelLocalPath;
        private TextBox txtLocalPath;
        private Button btnBrowseLocal;
        private Button btnCreateLocal;
        private Label labelLocalInfo;

        private string ConfigPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconnection.txt"); }
        }

        private string SqlProfilePath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconnection_sql.txt"); }
        }

        private string LocalProfilePath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconnection_local.txt"); }
        }

        private string DefaultLocalDatabasePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "IRAS SPOT POS",
                    "Data",
                    "MdemyPOS_Local.mdf");
            }
        }

        public DbConfigForm()
        {
            InitializeComponent();
            BuildLocalModeControls();
            ModernUI.Apply(this);
        }

        private void DbConfigForm_Load(object sender, EventArgs e)
        {
            try
            {
                string connStr = dbcon.myConnection();
                bool activeLocal = IsLocalConnection(connStr);
                string sqlConnection = File.Exists(SqlProfilePath)
                    ? File.ReadAllText(SqlProfilePath).Trim()
                    : (activeLocal ? @"Server=OMATSULI-TOJU-J;Database=MdemyPOS;Trusted_Connection=True;Encrypt=False;" : connStr);
                string localConnection = File.Exists(LocalProfilePath)
                    ? File.ReadAllText(LocalProfilePath).Trim()
                    : BuildLocalConnectionString(DefaultLocalDatabasePath);

                LoadSqlFields(sqlConnection);
                txtLocalPath.Text = GetLocalDatabasePath(activeLocal ? connStr : localConnection);
                cboConnectionMode.SelectedIndex = activeLocal ? 1 : 0;
                UpdateUiState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading current database configuration: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtLocalPath.Text = DefaultLocalDatabasePath;
                cboConnectionMode.SelectedIndex = 0;
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
            bool isLocal = cboConnectionMode != null && cboConnectionMode.SelectedIndex == 1;

            txtServer.Enabled = !isLocal;
            txtDatabase.Enabled = !isLocal;
            cboAuth.Enabled = !isLocal;
            chkEncrypt.Enabled = !isLocal;
            chkTrustCert.Enabled = !isLocal;
            txtUser.Enabled = !isLocal && isSqlServerAuth;
            txtPassword.Enabled = !isLocal && isSqlServerAuth;

            if (txtLocalPath != null)
            {
                txtLocalPath.Enabled = isLocal;
                btnBrowseLocal.Enabled = isLocal;
                btnCreateLocal.Enabled = isLocal;
                labelLocalInfo.Visible = isLocal;
            }
        }

        private string BuildConnectionString()
        {
            if (cboConnectionMode.SelectedIndex == 1)
                return BuildLocalConnectionString(txtLocalPath.Text.Trim());

            return BuildSqlConnectionStringFromFields();
        }

        private string BuildSqlConnectionStringFromFields()
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

        private string BuildLocalConnectionString(string mdfPath)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = @"(LocalDB)\MSSQLLocalDB";
            builder.InitialCatalog = "MdemyPOSLocal";
            builder.AttachDBFilename = mdfPath;
            builder.IntegratedSecurity = true;
            builder.ConnectTimeout = 30;
            return builder.ConnectionString;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (cboConnectionMode.SelectedIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(txtLocalPath.Text) || !File.Exists(txtLocalPath.Text.Trim()))
                {
                    MessageBox.Show("Please create or select the local database file first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (string.IsNullOrEmpty(txtServer.Text.Trim()) || string.IsNullOrEmpty(txtDatabase.Text.Trim()))
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
                    if (cboConnectionMode.SelectedIndex == 1)
                        EnsureLocalDbInstance();
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
            if (cboConnectionMode.SelectedIndex == 1)
            {
                if (string.IsNullOrWhiteSpace(txtLocalPath.Text) || !File.Exists(txtLocalPath.Text.Trim()))
                {
                    MessageBox.Show("Please create or select the local database file first.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else if (string.IsNullOrEmpty(txtServer.Text.Trim()) || string.IsNullOrEmpty(txtDatabase.Text.Trim()))
            {
                MessageBox.Show("Please fill in Server Name and Database Name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string connStr = BuildConnectionString();
                if (cboConnectionMode.SelectedIndex == 1)
                    EnsureLocalDbInstance();
                File.WriteAllText(ConfigPath, connStr);

                if (cboConnectionMode.SelectedIndex == 1)
                {
                    File.WriteAllText(LocalProfilePath, connStr);
                    if (!string.IsNullOrWhiteSpace(txtServer.Text) && !string.IsNullOrWhiteSpace(txtDatabase.Text))
                        File.WriteAllText(SqlProfilePath, BuildSqlConnectionStringFromFields());
                }
                else
                {
                    File.WriteAllText(SqlProfilePath, connStr);
                }

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

        private void BuildLocalModeControls()
        {
            ClientSize = new Size(620, 520);

            labelMode = new Label { Name = "labelMode", Text = "Connection Mode:", AutoSize = true, Location = new Point(40, 35) };
            cboConnectionMode = new ComboBox { Name = "cboConnectionMode", DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(210, 32), Size = new Size(330, 28) };
            cboConnectionMode.Items.AddRange(new object[] { "SQL Server", "Local Database File" });
            cboConnectionMode.SelectedIndexChanged += cboConnectionMode_SelectedIndexChanged;

            ShiftControl(labelServer, 40);
            ShiftControl(txtServer, 40);
            ShiftControl(labelDatabase, 40);
            ShiftControl(txtDatabase, 40);
            ShiftControl(labelAuth, 40);
            ShiftControl(cboAuth, 40);
            ShiftControl(labelUser, 40);
            ShiftControl(txtUser, 40);
            ShiftControl(labelPassword, 40);
            ShiftControl(txtPassword, 40);
            ShiftControl(chkEncrypt, 40);
            ShiftControl(chkTrustCert, 40);

            labelLocalPath = new Label { Name = "labelLocalPath", Text = "Local Database:", AutoSize = true, Location = new Point(40, 330) };
            txtLocalPath = new TextBox { Name = "txtLocalPath", Location = new Point(210, 327), Size = new Size(330, 26) };
            btnBrowseLocal = new Button { Name = "btnBrowseLocal", Text = "Browse", Location = new Point(548, 325), Size = new Size(62, 30) };
            btnBrowseLocal.Click += btnBrowseLocal_Click;
            btnCreateLocal = new Button { Name = "btnCreateLocal", Text = "Create Local Copy", Location = new Point(210, 365), Size = new Size(170, 35) };
            btnCreateLocal.Click += btnCreateLocal_Click;
            labelLocalInfo = new Label
            {
                Name = "labelLocalInfo",
                Text = "Local mode uses a database file saved on this computer. Create it from the SQL database once, then Save.",
                AutoSize = false,
                Location = new Point(210, 404),
                Size = new Size(380, 42)
            };

            btnTest.Location = new Point(40, 435);
            btnSave.Location = new Point(400, 435);
            btnCancel.Location = new Point(510, 435);
            panelFooter.Height = 50;

            Controls.Add(labelMode);
            Controls.Add(cboConnectionMode);
            Controls.Add(labelLocalPath);
            Controls.Add(txtLocalPath);
            Controls.Add(btnBrowseLocal);
            Controls.Add(btnCreateLocal);
            Controls.Add(labelLocalInfo);
        }

        private void ShiftControl(Control control, int y)
        {
            control.Location = new Point(control.Left, control.Top + y);
        }

        private void cboConnectionMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUiState();
        }

        private void btnBrowseLocal_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "SQL Server database file (*.mdf)|*.mdf|All files (*.*)|*.*";
                dialog.FileName = Path.GetFileName(txtLocalPath.Text);
                dialog.InitialDirectory = Directory.Exists(Path.GetDirectoryName(txtLocalPath.Text))
                    ? Path.GetDirectoryName(txtLocalPath.Text)
                    : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                if (dialog.ShowDialog() == DialogResult.OK)
                    txtLocalPath.Text = dialog.FileName;
            }
        }

        private void btnCreateLocal_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLocalPath.Text))
                txtLocalPath.Text = DefaultLocalDatabasePath;

            if (string.IsNullOrEmpty(txtServer.Text.Trim()) || string.IsNullOrEmpty(txtDatabase.Text.Trim()))
            {
                MessageBox.Show("Enter the SQL Server details first so the local copy can be created from it.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Create a local copy from the SQL Server database now?", "Create Local Database", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                btnCreateLocal.Enabled = false;
                Cursor = Cursors.WaitCursor;
                CreateLocalDatabaseCopy(BuildSqlConnectionStringFromFields(), txtLocalPath.Text.Trim());
                File.WriteAllText(LocalProfilePath, BuildLocalConnectionString(txtLocalPath.Text.Trim()));
                MessageBox.Show("Local database copy created successfully. Click Save to use Local Database File mode.", "Local Database Ready", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create local database copy: " + ex.Message, "Local Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnCreateLocal.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        private void LoadSqlFields(string connectionString)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            txtServer.Text = builder.DataSource;
            txtDatabase.Text = builder.InitialCatalog;

            if (builder.IntegratedSecurity)
            {
                cboAuth.SelectedIndex = 0;
                txtUser.Text = "";
                txtPassword.Text = "";
            }
            else
            {
                cboAuth.SelectedIndex = 1;
                txtUser.Text = builder.UserID;
                txtPassword.Text = builder.Password;
            }

            chkEncrypt.Checked = builder.Encrypt;
            chkTrustCert.Checked = builder.TrustServerCertificate;
        }

        private bool IsLocalConnection(string connectionString)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                return !string.IsNullOrWhiteSpace(builder.AttachDBFilename) ||
                       builder.DataSource.IndexOf("LocalDB", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private string GetLocalDatabasePath(string connectionString)
        {
            try
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);
                return string.IsNullOrWhiteSpace(builder.AttachDBFilename)
                    ? DefaultLocalDatabasePath
                    : builder.AttachDBFilename;
            }
            catch
            {
                return DefaultLocalDatabasePath;
            }
        }

        private void CreateLocalDatabaseCopy(string sourceConnectionString, string localMdfPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(localMdfPath));
            string localLogPath = Path.ChangeExtension(localMdfPath, ".ldf");
            string backupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "IRAS SPOT POS");
            Directory.CreateDirectory(backupFolder);
            string backupPath = Path.Combine(backupFolder, "MdemyPOS_LocalCopy.bak");
            if (File.Exists(backupPath))
                File.Delete(backupPath);
            string databaseName = new SqlConnectionStringBuilder(sourceConnectionString).InitialCatalog;

            using (SqlConnection source = new SqlConnection(sourceConnectionString))
            {
                source.Open();
                using (SqlCommand backup = new SqlCommand(
                    "BACKUP DATABASE [" + databaseName.Replace("]", "]]") + "] TO DISK = N'" + EscapeSqlLiteral(backupPath) + "' WITH COPY_ONLY, INIT", source))
                {
                    backup.CommandTimeout = 0;
                    backup.ExecuteNonQuery();
                }
            }

            DataTable files = new DataTable();
            EnsureLocalDbInstance();
            using (SqlConnection local = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30"))
            {
                local.Open();
                using (SqlCommand fileList = new SqlCommand("RESTORE FILELISTONLY FROM DISK = N'" + EscapeSqlLiteral(backupPath) + "'", local))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(fileList))
                        adapter.Fill(files);
                }

                string dataLogicalName = files.Rows.Cast<DataRow>().First(r => r["Type"].ToString() == "D")["LogicalName"].ToString();
                string logLogicalName = files.Rows.Cast<DataRow>().First(r => r["Type"].ToString() == "L")["LogicalName"].ToString();

                string restoreSql =
                    "IF DB_ID('MdemyPOSLocal') IS NOT NULL BEGIN ALTER DATABASE [MdemyPOSLocal] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [MdemyPOSLocal]; END " +
                    "RESTORE DATABASE [MdemyPOSLocal] FROM DISK = N'" + EscapeSqlLiteral(backupPath) + "' WITH REPLACE, " +
                    "MOVE N'" + EscapeSqlLiteral(dataLogicalName) + "' TO N'" + EscapeSqlLiteral(localMdfPath) + "', " +
                    "MOVE N'" + EscapeSqlLiteral(logLogicalName) + "' TO N'" + EscapeSqlLiteral(localLogPath) + "'";

                using (SqlCommand restore = new SqlCommand(restoreSql, local))
                {
                    restore.CommandTimeout = 0;
                    restore.ExecuteNonQuery();
                }
            }

            if (File.Exists(backupPath))
                File.Delete(backupPath);
        }

        private void EnsureLocalDbInstance()
        {
            RunLocalDbCommand("info MSSQLLocalDB", false);
            RunLocalDbCommand("create MSSQLLocalDB", false);
            RunLocalDbCommand("start MSSQLLocalDB", true);
        }

        private void RunLocalDbCommand(string arguments, bool throwOnFailure)
        {
            try
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "SqlLocalDB.exe";
                info.Arguments = arguments;
                info.CreateNoWindow = true;
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;

                using (Process process = Process.Start(info))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (throwOnFailure && process.ExitCode != 0)
                        throw new InvalidOperationException("Could not start SQL Server LocalDB. " + output + " " + error);
                }
            }
            catch (Win32Exception ex)
            {
                throw new InvalidOperationException("SQL Server LocalDB is not installed on this computer. Install SQL Server Express LocalDB, then try Local Database File mode again.", ex);
            }
        }

        private string EscapeSqlLiteral(string value)
        {
            return value.Replace("'", "''");
        }
    }
}
