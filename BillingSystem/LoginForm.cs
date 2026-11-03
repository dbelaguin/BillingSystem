using BillingSystem.Database;
using BillingSystem.Utils;
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
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            this.Text = "Billing System v1.0 - Login (J. Edio)";
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Step 1: Make sure both fields are filled
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter your username.",
                    "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please enter your password.",
                    "Login", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPassword.Focus();
                return;
            }

            // Step 2: Query the Users table to check credentials
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Parameterized query — safe from SQL injection
                    string sql = @"SELECT UserID, FullName, Role
                           FROM   Users
                           WHERE  Username = @Username
                             AND  Password = @Password;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", txtUsername.Text.Trim());
                        cmd.Parameters.AddWithValue("@Password", txtPassword.Text);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate AppSession with the logged-in user's details
                                AppSession.CurrentUserID = reader.GetInt32("UserID");
                                AppSession.CurrentUsername = txtUsername.Text.Trim();
                                AppSession.CurrentFullName = reader.GetString("FullName");
                                AppSession.CurrentRole = reader.GetString("Role");

                                // Write a LOGIN audit log entry
                                AuditLogger.Log("LOGIN",
                                    $"{AppSession.CurrentFullName} ({AppSession.CurrentRole}) logged in.");

                                // Open the Customer List Form
                                CustomerListForm listForm = new CustomerListForm();
                                listForm.Show();
                                this.Hide();
                            }


                            else
                            {
                                // No match found — wrong credentials
                                MessageBox.Show(
                                    "Invalid username or password.\nPlease try again.",
                                    "Login Failed",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                                txtPassword.Clear();
                                txtPassword.Focus();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Show an error if the database cannot be reached
                MessageBox.Show(
                    "Database error:\n" + ex.Message,
                    "Connection Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }


        private void LoginForm_Load(object sender, EventArgs e)
        {
            // Test the database connection when the form opens.
            // This gives a clear warning if MySQL is not running.
            if (!DatabaseConnection.TestConnection())
            {
                MessageBox.Show(
                "Cannot connect to the database.\n\n" +
                "Please make sure:\n" +
                " 1. MySQL Server is running.\n" +
                " 2. BillingDB database exists.\n" +
                " 3. The password in DatabaseConnection.cs is correct.",
                "Database Connection Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
            txtUsername.Focus();
        }



    }
}
