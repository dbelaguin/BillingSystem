using BillingSystem.Database;
using BillingSystem.Utils;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using MySql.Data.MySqlClient;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using BillingSystem.Utils;



namespace BillingSystem
{
    public partial class CustomerListForm : Form
    {
        // Stores the CustomerID of the currently selected row.
        // 0 means no customer is currently selected.
        private int _selectedCustomerId = 0;

        private bool canEdit = true;

        public CustomerListForm()
        {
            InitializeComponent();
            this.Shown += (s, e) => { dgvCustomers.ClearSelection(); dgvCustomers.CurrentCell = null; };
            ConfigureDataGridView();
        }

        private void ApplyTheme()
        {
            // Form background
            this.BackColor = AppTheme.BackgroundColor;

            // Action buttons
            btnAdd.BackColor = AppTheme.SuccessColor;
            btnAdd.ForeColor = System.Drawing.Color.White;
            btnDelete.BackColor = AppTheme.DangerColor;
            btnDelete.ForeColor = System.Drawing.Color.White;
            btnAnalytics.BackColor = AppTheme.PrimaryColor;
            btnAnalytics.ForeColor = System.Drawing.Color.White;
            btnExportExcel.BackColor = AppTheme.SecondaryColor;
            btnExportExcel.ForeColor = System.Drawing.Color.White;
            btnExportPdf.BackColor = AppTheme.SecondaryColor;
            btnExportPdf.ForeColor = System.Drawing.Color.White;
            btnAuditLog.BackColor = AppTheme.SecondaryColor;
            btnAuditLog.ForeColor = System.Drawing.Color.White;
            btnManagePermissions.BackColor = AppTheme.DangerColor;
            btnManagePermissions.ForeColor = System.Drawing.Color.White;

            // DataGridView header colors
            dgvCustomers.ColumnHeadersDefaultCellStyle.BackColor = AppTheme.PrimaryColor;
            dgvCustomers.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dgvCustomers.ColumnHeadersDefaultCellStyle.Font =
               new System.Drawing.Font(
              "Arial",
                10,
                System.Drawing.FontStyle.Bold
                );

            // Alternating row colors
            dgvCustomers.AlternatingRowsDefaultCellStyle.BackColor = AppTheme.GridRowAlt;
        }



        private void ApplyPermissions()
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string sql = @"SELECT PermissionName, IsAllowed
                           FROM   UserPermissions
                           WHERE  Role = @Role;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Role", AppSession.CurrentRole);

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string permName = reader.GetString("PermissionName");
                                bool isAllowed = reader.GetBoolean("IsAllowed");

                                switch (permName)
                                {
                                    case "AddCustomer":
                                        btnAdd.Enabled = isAllowed; break;
                                    case "EditCustomer":
                                        // Edit is triggered by double-click — remember the
                                        // permission so CellDoubleClick can check it
                                        canEdit = isAllowed;
                                        break;
                                    case "DeleteCustomer":
                                        btnDelete.Enabled = isAllowed; break;
                                    case "Analytics":
                                        btnAnalytics.Enabled = isAllowed; break;
                                    case "ExportExcel":
                                        btnExportExcel.Enabled = isAllowed; break;
                                    case "ExportPdf":
                                        btnExportPdf.Enabled = isAllowed; break;
                                    case "AuditLogs":
                                        btnAuditLog.Enabled = isAllowed; break;
                                    case "ManagePermissions":
                                        btnManagePermissions.Enabled = isAllowed; break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading permissions:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void ConfigureDataGridView()
        {


            dgvCustomers.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            dgvCustomers.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            dgvCustomers.MultiSelect = false;
            dgvCustomers.ReadOnly = true;
            dgvCustomers.AllowUserToAddRows = false;
        }

        private void CustomerListForm_Load(object sender, EventArgs e)
        {
            ApplyTheme();
            LoadCustomers();
            ApplyPermissions();
            InitStatusStrip();
        }




        private void LoadCustomers()
        {
            try
            {
                // Clear existing rows
                dgvCustomers.Rows.Clear();

                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT CustomerID, FullName, Address, ContactNumber, Email, Balance FROM Customers ORDER BY FullName;";
                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int customerId = reader.GetInt32("CustomerID");
                                string fullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? "(No Name)" : reader.GetString("FullName");
                                string address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString("Address");
                                string contactNumber = reader.IsDBNull(reader.GetOrdinal("ContactNumber")) ? "" : reader.GetString("ContactNumber");
                                string email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email");
                                decimal balance = reader.IsDBNull(reader.GetOrdinal("Balance")) ? 0 : reader.GetDecimal("Balance");

                                dgvCustomers.Rows.Add(customerId, fullName, address, contactNumber, email, balance);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading customers:\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void dgvCustomers_SelectionChanged(object sender, EventArgs e)
        {
            // If no row is selected (e.g., grid is empty), do nothing
            if (dgvCustomers.CurrentRow == null) return;

            // Read the CustomerID value from the selected row
            var idCell = dgvCustomers.CurrentRow.Cells["CustomerID"].Value;

            if (idCell != null && int.TryParse(idCell.ToString(), out int id))
            {
                _selectedCustomerId = id;
            }
        }

        private void DgvCustomers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dgvCustomers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!canEdit)
                return;
            // e.RowIndex is -1 when the header row is double-clicked — ignore it
            if (e.RowIndex < 0) return;

            OpenEditForm();
        }

        private void OpenEditForm()
        {
            if (_selectedCustomerId == 0)
            {
                MessageBox.Show("Please select a customer to edit.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Open AddCustomerForm in EDIT mode, passing the selected CustomerID
            AddCustomerForm editForm = new AddCustomerForm(_selectedCustomerId);

            // Refresh the grid automatically once the edit form closes
            editForm.FormClosed += (s, args) => LoadCustomers();

            editForm.ShowDialog(this);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Step 1: Make sure a customer is selected
            if (_selectedCustomerId == 0)
            {
                MessageBox.Show("Please select a customer to delete.",
                    "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Step 2: Confirm before deleting — this cannot be undone
            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete this customer?\n" +
                "All billing records for this customer will also be deleted.",
                "Confirm Delete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            // Step 3: Only delete if the user clicked Yes
            if (confirm == DialogResult.Yes)
            {
                DeleteCustomer(_selectedCustomerId);
            }
            // If the user clicked No, do nothing — the record is preserved
        }

        private void DeleteCustomer(int customerId)
        {
            try
            {
                using (var conn = DatabaseConnection.GetConnection())
                {
                    conn.Open();

                    // Parameterized DELETE — removes exactly one row
                    string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID;";

                    using (var cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerID", customerId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {

                            AuditLogger.Log("DELETE_CUSTOMER",
    $"Customer ID {customerId} deleted by {AppSession.CurrentUsername}.");

                            MessageBox.Show("Customer deleted successfully.",
                                "Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadCustomers();   // Refresh the grid
                            _selectedCustomerId = 0;   // Clear selection tracker
                        }




                        else
                        {
                            MessageBox.Show("Customer could not be deleted. It may no longer exist.",
                                "Delete Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting customer:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAnalytics_Click(object sender, EventArgs e)
        {
            AuditLogger.Log("VIEW_ANALYTICS",
    $"{AppSession.CurrentUsername} opened the Analytics Dashboard.");


            // Open the Analytics Dashboard as a dialog so the
            // Customer List stays open in the background
            frmAnalytics analyticsForm = new frmAnalytics();
            analyticsForm.ShowDialog(this);
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            // Make sure there is something to export 
            if (dgvCustomers.Rows.Count == 0)
            {
                AuditLogger.Log("EXPORT_EXCEL",
    $"{AppSession.CurrentUsername} exported customer list to Excel.");


                MessageBox.Show("There are no records to export.",
                    "Export to Excel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Let the user choose where to save the file 
            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                saveDialog.FileName = "CustomerList.xlsx";

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var workbook = new XLWorkbook())
                    {

                        var worksheet = workbook.Worksheets.Add("Customers");

                        // Write column headers in row 1 
                        for (int col = 0; col < dgvCustomers.Columns.Count; col++)
                        {
                            worksheet.Cell(1, col + 1).Value = dgvCustomers.Columns[col].HeaderText;
                            worksheet.Cell(1, col + 1).Style.Font.Bold = true;
                        }

                        // Write each data row starting from row 2 
                        for (int row = 0; row < dgvCustomers.Rows.Count; row++)
                        {
                            for (int col = 0; col < dgvCustomers.Columns.Count;
                                col++)
                            {
                                var cellValue = dgvCustomers.Rows[row].Cells[col].Value;
                                worksheet.Cell(row + 2, col + 1).Value = cellValue?.ToString() ?? "";
                            }
                        }

                        // Auto-adjust column widths to fit the content 
                        worksheet.Columns().AdjustToContents();

                        workbook.SaveAs(saveDialog.FileName);
                    }

                    MessageBox.Show("Customer list exported successfully to Excel.",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting to Excel:\n{ex.Message}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            // Make sure there is something to export
            if (dgvCustomers.Rows.Count == 0)
            {
                AuditLogger.Log("EXPORT_PDF",
    $"{AppSession.CurrentUsername} exported customer list to PDF.");


                MessageBox.Show("There are no records to export.",
                    "Export to PDF", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "PDF Document (*.pdf)|*.pdf";
                saveDialog.FileName = "CustomerList.pdf";

                if (saveDialog.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (PdfDocument document = new PdfDocument())
                    {
                        // Create a new page set to Landscape orientation
                        PdfPage page = document.AddPage();
                        page.Orientation = PdfSharp.PageOrientation.Landscape;

                        XGraphics gfx = XGraphics.FromPdfPage(page);
                        {
                            XFont titleFont = new XFont("Arial", 16, XFontStyleEx.Bold);
                            XFont headerFont = new XFont("Arial", 10, XFontStyleEx.Bold);
                            XFont cellFont = new XFont("Arial", 9, XFontStyleEx.Regular);

                            // Title
                            gfx.DrawString("Customer List Report", titleFont, XBrushes.Black,
                                new XRect(0, 20, page.Width.Point, 30), XStringFormats.TopCenter);

                            int columnCount = dgvCustomers.Columns.Count;
                            double margin = 30;
                            double tableWidth = page.Width.Point - (margin * 2);
                            double colWidth = tableWidth / columnCount;
                            double rowHeight = 22;
                            double y = 60;

                            // Draw column headers
                            double x = margin;
                            for (int col = 0; col < columnCount; col++)
                            {
                                gfx.DrawString(dgvCustomers.Columns[col].HeaderText, headerFont,
                                    XBrushes.Black, new XRect(x, y, colWidth, rowHeight),
                                    XStringFormats.CenterLeft);
                                x += colWidth;
                            }

                            y += rowHeight;
                            gfx.DrawLine(XPens.Black, margin, y, page.Width.Point - margin, y);

                            // Draw each data row
                            foreach (DataGridViewRow row in dgvCustomers.Rows)
                            {
                                x = margin;
                                y += rowHeight;

                                // Start a new page if we run out of vertical space
                                if (y > page.Height.Point - margin)
                                {
                                    page = document.AddPage();
                                    page.Orientation = PdfSharp.PageOrientation.Landscape;
                                    gfx.Dispose();
                                    gfx = XGraphics.FromPdfPage(page);
                                    y = 40;
                                }

                                for (int col = 0; col < columnCount; col++)
                                {
                                    string text = row.Cells[col].Value?.ToString() ?? "";
                                    gfx.DrawString(text, cellFont, XBrushes.Black,
                                        new XRect(x, y, colWidth, rowHeight),
                                        XStringFormats.CenterLeft);
                                    x += colWidth;
                                }
                            }
                        }

                        gfx.Dispose();   // finish drawing before saving
                        document.Save(saveDialog.FileName);
                    }

                    MessageBox.Show("Customer list exported successfully to PDF.",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting to PDF:\n{ex.Message}",
                        "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // TODO: Add your code for the Add button
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // TODO: Add your search filtering code
        }

        private void txtSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            // TODO: Add your search key-press code
        }

        private void btnSearch_Click_1(object sender, EventArgs e)
        {
            // TODO: Add your search button code
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            AuditLogger.Log("LOGOUT",
    $"{AppSession.CurrentFullName} ({AppSession.CurrentRole}) logged out.");
            AppSession.Clear();


        }



        private void btnAuditLog_Click(object sender, EventArgs e)
        {
            frmAuditLogs auditForm = new frmAuditLogs();
            auditForm.ShowDialog(this);
        }
        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            using (frmChangePassword changePasswordForm = new frmChangePassword())
            {
                changePasswordForm.ShowDialog(this);
            }
        }

        private void btnManagePermissions_Click(object sender, EventArgs e)
        {
            frmManagePermissions permForm = new frmManagePermissions();
            permForm.ShowDialog(this);
        }

        // Call this inside CustomerListForm_Load after ApplyPermissions()
        private void InitStatusStrip()
        {
            lblStatusUser.Text =
                $"User: {AppSession.CurrentFullName}  |  Role: {AppSession.CurrentRole}";
            UpdateClock();
        }

        private void UpdateClock()
        {
            lblStatusTime.Text = DateTime.Now.ToString("dddd, MMMM dd yyyy   hh:mm:ss tt");
        }

        private void statusTimer_Tick(object sender, EventArgs e)
        {
            UpdateClock();
        }

        private void dgvCustomers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Skip the header row
            if (e.RowIndex < 0) return;


            // Read Status from the data behind this row (the DataTable from LoadCustomers)
            var rowData = dgvCustomers.Rows[e.RowIndex].DataBoundItem as DataRowView;
            string status = rowData?["Status"]?.ToString() ?? "";


            if (status == "Inactive")
            {
                // The whole row turns light red
                e.CellStyle.BackColor = AppTheme.InactiveRowColor;
            }
            else if (status == "Active" &&
                     dgvCustomers.Columns[e.ColumnIndex].DataPropertyName == "Status")
            {
                // Only the Status cell turns light green, so the
                // alternating row colors stay visible
                e.CellStyle.BackColor = AppTheme.ActiveRowColor;
            }
        }

        private void btnViewBilling_Click(object sender, EventArgs e)
        {
            // Popup 1: walang napiling customer
            if (dgvCustomers.SelectedRows.Count == 0 ||
                dgvCustomers.SelectedRows[0].IsNewRow)
            {
                MessageBox.Show("Please select a customer first before viewing billing history.",
                    "No Customer Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return; // hindi magbubukas ang form
            }

            int customerId = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells[0].Value);

            using (var frm = new frmBillingHistory(customerId))
            {
                frm.ShowDialog();
            }
        }
    }
}
