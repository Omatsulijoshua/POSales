using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace POSales
{
    class DBConnect
    {
        SqlConnection cn = new SqlConnection();
        SqlCommand cm = new SqlCommand();
        SqlDataReader dr;
        private string con;

        public DBConnect()
        {
            EnsureStoreColumns();
        }

        private void EnsureStoreColumns()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(myConnection()))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(
                        "IF COL_LENGTH('dbo.tbStore', 'vat_type') IS NULL ALTER TABLE dbo.tbStore ADD vat_type NVARCHAR(50) NULL; " +
                        "IF COL_LENGTH('dbo.tbStore', 'vat_percent') IS NULL ALTER TABLE dbo.tbStore ADD vat_percent DECIMAL(18, 2) NULL;", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch { }
        }
        public string myConnection()
        {
            string configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbconnection.txt");
            if (System.IO.File.Exists(configPath))
            {
                string savedConnection = System.IO.File.ReadAllText(configPath).Trim();
                if (!string.IsNullOrEmpty(savedConnection))
                {
                    return savedConnection;
                }
            }
            return @"Server=OMATSULI-TOJU-J;Database=MdemyPOS;Trusted_Connection=True;Encrypt=False;";
        }


        // @"Data Source=DESKTOP-C4UJ6TV\SQLEXPRESS;
        //@"Data Source = OMATSULI - TOJU - J;
        //@"Data Source=192.168.0.153;
        //Encrypt=True;
        //           TrustServerCertificate=True;

        public DataTable getTable(string qury)
        {
            cn.ConnectionString = myConnection();
            cm = new SqlCommand(qury, cn);
            SqlDataAdapter adapter = new SqlDataAdapter(cm);
            DataTable table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public void ExecuteQuery(String sql)
        {
            try
            {
                cn.ConnectionString = myConnection();
                cn.Open();
                cm = new SqlCommand(sql, cn);
                cm.ExecuteNonQuery();
                cn.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
           
        }

        public String getPassword(string username)
        {
            string password = "";
            cn.ConnectionString = myConnection();
            cn.Open();
            cm = new SqlCommand("SELECT password FROM tbUser WHERE username = '"+ username + "'", cn);
            dr= cm.ExecuteReader();
            dr.Read();
            if(dr.HasRows)
            {
                password = dr["password"].ToString();
            }
            dr.Close();
            cn.Close();
            return password;
        }

        public double ExtractData(string sql)
        {
          
            cn = new SqlConnection();
            cn.ConnectionString = myConnection();
            cn.Open();
            cm = new SqlCommand(sql, cn);
            double data = double.Parse(cm.ExecuteScalar().ToString());
            cn.Close();
            return data;

        }

        public string getStoreName()
        {
            string storeName = "";
            try
            {
                using (SqlConnection tempCn = new SqlConnection(myConnection()))
                {
                    tempCn.Open();
                    using (SqlCommand tempCm = new SqlCommand("SELECT TOP 1 store FROM tbStore", tempCn))
                    {
                        object result = tempCm.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            storeName = result.ToString();
                        }
                    }
                }
            }
            catch
            {
                // Fallback in case of database not configured or table doesn't exist yet
            }
            return storeName;
        }

        public string GetVatType()
        {
            string type = "Old";
            try
            {
                using (SqlConnection tempCn = new SqlConnection(myConnection()))
                {
                    tempCn.Open();
                    using (SqlCommand tempCm = new SqlCommand("SELECT TOP 1 ISNULL(vat_type, 'Old') FROM tbStore", tempCn))
                    {
                        object result = tempCm.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            type = result.ToString();
                        }
                    }
                }
            }
            catch { }
            return type;
        }

        public double GetVatPercent()
        {
            double percent = 12.0;
            try
            {
                using (SqlConnection tempCn = new SqlConnection(myConnection()))
                {
                    tempCn.Open();
                    using (SqlCommand tempCm = new SqlCommand("SELECT TOP 1 vat_percent FROM tbStore", tempCn))
                    {
                        object result = tempCm.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            percent = Convert.ToDouble(result);
                        }
                        else
                        {
                            percent = GetVatType() == "New" ? 7.5 : 12.0;
                        }
                    }
                }
            }
            catch { }
            return percent;
        }
    }
}
