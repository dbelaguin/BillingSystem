using BillingSystem.Database;
using BillingSystem.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BillingSystem
{
    public partial class frmChangePassword : Form
    {
        public frmChangePassword()
        {
            InitializeComponent();
            AcceptButton = btnSavePassword;
            CancelButton = btnCancel;
        }

        private void btnSavePassword_Click(object sender, EventArgs e)
        {
            string currentPassword = txtCurrentPassword.Text;
            string newPassword = txtNewPassword.Text;
            string retypePassword = txtRetypePassword.Text;

            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowValidationMessage("Please enter your current password.");
                txtCurrentPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(newPassword))
            {
                ShowValidationMessage("Please enter a new password.");
                txtNewPassword.Focus();
                return;
            }

            if (string.IsNullOrEmpty(retypePassword))
            {
                ShowValidationMessage("Please retype your new password.");
                txtRetypePassword.Focus();
                return;
            }

            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Verify the current password using the logged-in user's unique ID.
                    const string selectSql =
                        "SELECT Password FROM Users WHERE UserID = @UserID LIMIT 1;";

                    string storedPassword;
                    using (var selectCmd = new MySqlCommand(selectSql, conn))
                    {
                        selectCmd.Parameters.AddWithValue("@UserID", AppSession.CurrentUserID);
                        object result = selectCmd.ExecuteScalar();

                        if (result == null || result == DBNull.Value)
                        {
                            MessageBox.Show("The logged-in account could not be found.",
                                "Account Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        storedPassword = Convert.ToString(result) ?? string.Empty;
                    }

                    // Validation order: verify identity first, then matching entries,
                    // then password complexity. No database update occurs before all pass.
                    if (currentPassword != storedPassword)
                    {
                        ShowValidationMessage("Current password is incorrect.");
                        txtCurrentPassword.Clear();
                        txtCurrentPassword.Focus();
                        return;
                    }

                    if (newPassword != retypePassword)
                    {
                        ShowValidationMessage("New Password and Retype Password do not match.");
                        txtRetypePassword.Focus();
                        return;
                    }

                    List<string> unmetRules = new List<string>();
                    if (newPassword.Length < 8)
                        unmetRules.Add("At least 8 characters long");
                    if (!Regex.IsMatch(newPassword, "[A-Z]"))
                        unmetRules.Add("At least one uppercase letter (A-Z)");
                    if (!Regex.IsMatch(newPassword, "[a-z]"))
                        unmetRules.Add("At least one lowercase letter (a-z)");
                    if (!Regex.IsMatch(newPassword, "[0-9]"))
                        unmetRules.Add("At least one numeric digit (0-9)");
                    if (!Regex.IsMatch(newPassword, "[^a-zA-Z0-9]"))
                        unmetRules.Add("At least one special character (e.g. !@#$%^&*)");

                    if (unmetRules.Count > 0)
                    {
                        MessageBox.Show(
                            "Your new password does not meet these requirements:\n\n- " +
                            string.Join("\n- ", unmetRules),
                            "Password Requirements",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
                        txtNewPassword.Focus();
                        return;
                    }

                    const string updateSql =
                        "UPDATE Users SET Password = @NewPassword WHERE UserID = @UserID;";

                    using (var updateCmd = new MySqlCommand(updateSql, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@NewPassword", newPassword);
                        updateCmd.Parameters.AddWithValue("@UserID", AppSession.CurrentUserID);

                        int rowsAffected = updateCmd.ExecuteNonQuery();
                        if (rowsAffected != 1)
                        {
                            MessageBox.Show("The password was not changed. Please try again.",
                                "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }

                AuditLogger.Log("CHANGE_PASSWORD",
                    $"{AppSession.CurrentUsername} changed their password.");

                MessageBox.Show("Your password has been changed successfully.",
                    "Password Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while changing your password:\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowValidationMessage(string message)
        {
            MessageBox.Show(message, "Change Password",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
