using System;
using System.Drawing;
using System.Windows.Forms;
using ATSCADA_WinForms.Services;

namespace ATSCADA_WinForms
{
    public partial class LoginForm : Form
    {
        private ApiService _apiService;

        public LoginForm(ApiService apiService)
        {
            _apiService = apiService;
            InitializeComponent();
            try { this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }
            btnLogin.Click += BtnLogin_Click;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            btnLogin.Enabled = false;
            lblMessage.Text = "Đang xác thực...";

            try
            {
                bool success = await _apiService.LoginAsync(txtUsername.Text, txtPassword.Text);
                if (success)
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblMessage.Text = "Đăng nhập thất bại. Vui lòng kiểm tra lại.";
                    btnLogin.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Lỗi kết nối API: " + ex.Message;
                btnLogin.Enabled = true;
            }
        }

        
    }
}
