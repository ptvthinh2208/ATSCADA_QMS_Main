namespace ATSCADA_WinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lblCounterInfo = new Label();
            lblCurrentNumber = new Label();
            btnNext = new Button();
            btnCallAny = new Button();
            pnlCallAny = new Panel();
            txtAnyNumber = new TextBox();
            btnSendAny = new Button();
            tblButtons = new TableLayoutPanel();
            btnPrevious = new Button();
            btnReset = new Button();
            pnlCallAny.SuspendLayout();
            tblButtons.SuspendLayout();
            SuspendLayout();
            // 
            // lblCounterInfo
            // 
            lblCounterInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCounterInfo.ForeColor = Color.FromArgb(33, 37, 41);
            lblCounterInfo.Location = new Point(6, 7);
            lblCounterInfo.Name = "lblCounterInfo";
            lblCounterInfo.Size = new Size(389, 40);
            lblCounterInfo.TabIndex = 0;
            lblCounterInfo.Text = "Cargando...";
            lblCounterInfo.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // lblCurrentNumber
            // 
            lblCurrentNumber.Font = new Font("Segoe UI", 64F, FontStyle.Bold);
            lblCurrentNumber.ForeColor = Color.FromArgb(0, 123, 255);
            lblCurrentNumber.Location = new Point(6, 53);
            lblCurrentNumber.Name = "lblCurrentNumber";
            lblCurrentNumber.Size = new Size(389, 133);
            lblCurrentNumber.TabIndex = 1;
            lblCurrentNumber.Text = "000";
            lblCurrentNumber.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // btnNext
            // 
            btnNext.BackColor = Color.FromArgb(40, 167, 69);
            btnNext.Dock = DockStyle.Fill;
            btnNext.FlatAppearance.BorderSize = 0;
            btnNext.FlatStyle = FlatStyle.Flat;
            btnNext.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnNext.ForeColor = Color.White;
            btnNext.Location = new Point(3, 4);
            btnNext.Margin = new Padding(3, 4, 3, 4);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(188, 85);
            btnNext.TabIndex = 0;
            btnNext.Text = "TIẾP THEO (F1)";
            btnNext.UseVisualStyleBackColor = false;
            // 
            // btnCallAny
            // 
            btnCallAny.BackColor = Color.FromArgb(108, 117, 125);
            btnCallAny.Dock = DockStyle.Fill;
            btnCallAny.FlatAppearance.BorderSize = 0;
            btnCallAny.FlatStyle = FlatStyle.Flat;
            btnCallAny.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnCallAny.ForeColor = Color.White;
            btnCallAny.Location = new Point(197, 4);
            btnCallAny.Margin = new Padding(3, 4, 3, 4);
            btnCallAny.Name = "btnCallAny";
            btnCallAny.Size = new Size(189, 85);
            btnCallAny.TabIndex = 1;
            btnCallAny.Text = "GỌI BẤT KỲ (F4)";
            btnCallAny.UseVisualStyleBackColor = false;
            // 
            // btnPrevious
            // 
            btnPrevious.BackColor = Color.FromArgb(255, 193, 7);
            btnPrevious.Dock = DockStyle.Fill;
            btnPrevious.FlatAppearance.BorderSize = 0;
            btnPrevious.FlatStyle = FlatStyle.Flat;
            btnPrevious.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnPrevious.ForeColor = Color.White;
            btnPrevious.Location = new Point(3, 97);
            btnPrevious.Margin = new Padding(3, 4, 3, 4);
            btnPrevious.Name = "btnPrevious";
            btnPrevious.Size = new Size(188, 85);
            btnPrevious.TabIndex = 2;
            btnPrevious.Text = "GỌI LÙI (F2)";
            btnPrevious.UseVisualStyleBackColor = false;
            // 
            // btnReset
            // 
            btnReset.BackColor = Color.FromArgb(220, 53, 69);
            btnReset.Dock = DockStyle.Fill;
            btnReset.FlatAppearance.BorderSize = 0;
            btnReset.FlatStyle = FlatStyle.Flat;
            btnReset.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnReset.ForeColor = Color.White;
            btnReset.Location = new Point(197, 97);
            btnReset.Margin = new Padding(3, 4, 3, 4);
            btnReset.Name = "btnReset";
            btnReset.Size = new Size(189, 85);
            btnReset.TabIndex = 3;
            btnReset.Text = "RESET (F3)";
            btnReset.UseVisualStyleBackColor = false;
            // 
            // pnlCallAny
            // 
            pnlCallAny.BackColor = Color.White;
            pnlCallAny.Controls.Add(txtAnyNumber);
            pnlCallAny.Controls.Add(btnSendAny);
            pnlCallAny.Location = new Point(6, 400);
            pnlCallAny.Margin = new Padding(3, 4, 3, 4);
            pnlCallAny.Name = "pnlCallAny";
            pnlCallAny.Size = new Size(389, 67);
            pnlCallAny.TabIndex = 3;
            pnlCallAny.Visible = false;
            // 
            // txtAnyNumber
            // 
            txtAnyNumber.Font = new Font("Segoe UI", 14F);
            txtAnyNumber.Location = new Point(11, 13);
            txtAnyNumber.Margin = new Padding(3, 4, 3, 4);
            txtAnyNumber.Name = "txtAnyNumber";
            txtAnyNumber.PlaceholderText = "Nhập số...";
            txtAnyNumber.Size = new Size(228, 39);
            txtAnyNumber.TabIndex = 0;
            // 
            // btnSendAny
            // 
            btnSendAny.BackColor = Color.FromArgb(0, 123, 255);
            btnSendAny.FlatStyle = FlatStyle.Flat;
            btnSendAny.ForeColor = Color.White;
            btnSendAny.Location = new Point(251, 13);
            btnSendAny.Margin = new Padding(3, 4, 3, 4);
            btnSendAny.Name = "btnSendAny";
            btnSendAny.Size = new Size(137, 40);
            btnSendAny.TabIndex = 1;
            btnSendAny.Text = "Gửi";
            btnSendAny.UseVisualStyleBackColor = false;
            // 
            // tblButtons
            // 
            tblButtons.ColumnCount = 2;
            tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblButtons.Controls.Add(btnNext, 0, 0);
            tblButtons.Controls.Add(btnCallAny, 1, 0);
            tblButtons.Controls.Add(btnPrevious, 0, 1);
            tblButtons.Controls.Add(btnReset, 1, 1);
            tblButtons.Location = new Point(6, 200);
            tblButtons.Margin = new Padding(3, 4, 3, 4);
            tblButtons.Name = "tblButtons";
            tblButtons.RowCount = 2;
            tblButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblButtons.Size = new Size(389, 186);
            tblButtons.TabIndex = 2;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(245, 245, 245);
            ClientSize = new Size(400, 480);
            Controls.Add(tblButtons);
            Controls.Add(pnlCallAny);
            Controls.Add(lblCurrentNumber);
            Controls.Add(lblCounterInfo);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            KeyPreview = true;
            Margin = new Padding(3, 4, 3, 4);
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "QMS Counter Dashboard";
            TopMost = true;
            pnlCallAny.ResumeLayout(false);
            pnlCallAny.PerformLayout();
            tblButtons.ResumeLayout(false);
            ResumeLayout(false);
        }

        private System.Windows.Forms.Label lblCurrentNumber;
        private System.Windows.Forms.Label lblCounterInfo;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.Button btnCallAny;
        private System.Windows.Forms.Button btnPrevious;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Panel pnlCallAny;
        private System.Windows.Forms.TextBox txtAnyNumber;
        private System.Windows.Forms.Button btnSendAny;
        private System.Windows.Forms.TableLayoutPanel tblButtons;
    }
}
