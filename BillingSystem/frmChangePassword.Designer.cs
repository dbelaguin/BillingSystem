namespace BillingSystem
{
    partial class frmChangePassword
    {
        private System.ComponentModel.IContainer components = null;
        private Label lblCurrentPassword;
        private Label lblNewPassword;
        private Label lblRetypePassword;
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private TextBox txtRetypePassword;
        private Button btnSavePassword;
        private Button btnCancel;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            lblCurrentPassword = new Label();
            lblNewPassword = new Label();
            lblRetypePassword = new Label();
            txtCurrentPassword = new TextBox();
            txtNewPassword = new TextBox();
            txtRetypePassword = new TextBox();
            btnSavePassword = new Button();
            btnCancel = new Button();
            SuspendLayout();

            lblCurrentPassword.AutoSize = true;
            lblCurrentPassword.Location = new Point(30, 30);
            lblCurrentPassword.Name = "lblCurrentPassword";
            lblCurrentPassword.Size = new Size(116, 20);
            lblCurrentPassword.Text = "Current Password:";

            txtCurrentPassword.Location = new Point(175, 27);
            txtCurrentPassword.Name = "txtCurrentPassword";
            txtCurrentPassword.Size = new Size(240, 27);
            txtCurrentPassword.UseSystemPasswordChar = true;
            txtCurrentPassword.TabIndex = 0;

            lblNewPassword.AutoSize = true;
            lblNewPassword.Location = new Point(30, 78);
            lblNewPassword.Name = "lblNewPassword";
            lblNewPassword.Size = new Size(105, 20);
            lblNewPassword.Text = "New Password:";

            txtNewPassword.Location = new Point(175, 75);
            txtNewPassword.Name = "txtNewPassword";
            txtNewPassword.Size = new Size(240, 27);
            txtNewPassword.UseSystemPasswordChar = true;
            txtNewPassword.TabIndex = 1;

            lblRetypePassword.AutoSize = true;
            lblRetypePassword.Location = new Point(30, 126);
            lblRetypePassword.Name = "lblRetypePassword";
            lblRetypePassword.Size = new Size(119, 20);
            lblRetypePassword.Text = "Retype Password:";

            txtRetypePassword.Location = new Point(175, 123);
            txtRetypePassword.Name = "txtRetypePassword";
            txtRetypePassword.Size = new Size(240, 27);
            txtRetypePassword.UseSystemPasswordChar = true;
            txtRetypePassword.TabIndex = 2;

            btnSavePassword.Location = new Point(175, 180);
            btnSavePassword.Name = "btnSavePassword";
            btnSavePassword.Size = new Size(135, 35);
            btnSavePassword.Text = "Save Password";
            btnSavePassword.UseVisualStyleBackColor = true;
            btnSavePassword.Click += btnSavePassword_Click;
            btnSavePassword.TabIndex = 3;

            btnCancel.Location = new Point(320, 180);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(95, 35);
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            btnCancel.TabIndex = 4;

            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(455, 245);
            Controls.Add(lblCurrentPassword);
            Controls.Add(txtCurrentPassword);
            Controls.Add(lblNewPassword);
            Controls.Add(txtNewPassword);
            Controls.Add(lblRetypePassword);
            Controls.Add(txtRetypePassword);
            Controls.Add(btnSavePassword);
            Controls.Add(btnCancel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "frmChangePassword";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Change Password";
            ResumeLayout(false);
            PerformLayout();
        }
    }
}
