using BillingSystem.Database;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace BillingSystem
{
    public partial class frmBillingHistory : Form
    {
        private readonly int _customerId;

        public frmBillingHistory(int customerId)
        {
            InitializeComponent();
            _customerId = customerId;
        }

        private void frmBillingHistory_Load(object sender, EventArgs e)
        {
            LoadCustomerName();
            LoadBillingRecords();
        }

        private void LoadCustomerName()
        {
            string sql = "SELECT FullName FROM customers WHERE CustomerID = @id";

            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", _customerId);
                conn.Open();
                object result = cmd.ExecuteScalar();
                lblCustomerName.Text = "Billing History - " + (result?.ToString() ?? "Unknown Customer");
            }
        }

        private void LoadBillingRecords()
        {
            string sql = @"SELECT BillingMonth, PreviousReading, PresentReading,
                                  Consumption, RatePerCubic, TotalAmount, Status
                           FROM billing
                           WHERE CustomerID = @id
                           ORDER BY BillingDate DESC";

            var table = new DataTable();

            using (var conn = DatabaseConnection.GetConnection())
            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", _customerId);
                conn.Open();
                using (var adapter = new MySqlDataAdapter(cmd))
                {
                    adapter.Fill(table);
                }
            }

            dgvBilling.DataSource = table;

            dgvBilling.Columns["BillingMonth"].HeaderText = "Billing Month";
            dgvBilling.Columns["PreviousReading"].HeaderText = "Previous Reading";
            dgvBilling.Columns["PresentReading"].HeaderText = "Present Reading";
            dgvBilling.Columns["RatePerCubic"].HeaderText = "Rate Per Cubic";
            dgvBilling.Columns["TotalAmount"].HeaderText = "Total Amount";
            dgvBilling.Columns["RatePerCubic"].DefaultCellStyle.Format = "N2";
            dgvBilling.Columns["TotalAmount"].DefaultCellStyle.Format = "N2";

            // Popup 2: zero billing records
            if (table.Rows.Count == 0)
            {
                MessageBox.Show("No billing records were found for this customer.",
                    "No Records", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}