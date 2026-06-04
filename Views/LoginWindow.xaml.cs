using KCH_New.Services;
using System.Windows;
using System.Windows.Input;

namespace KCH_New.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AuthService _auth = new();

        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;
            var user = await _auth.LoginAsync(txtUsername.Text.Trim(), txtPassword.Password);
            if (user != null)
            {
                new InvoiceWindow().Show();
                Close();
            }
            else
            {
                txtError.Text = "اسم المستخدم أو كلمة المرور غير صحيحة";
                txtError.Visibility = Visibility.Visible;
                txtPassword.Clear();
            }
        }

        private async void BtnChangePassword_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new ChangePasswordDialog { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                bool ok = await _auth.ChangePasswordAsync(
                    dlg.OldUsername, dlg.OldPassword,
                    dlg.NewUsername, dlg.NewPassword);
                MessageBox.Show(ok ? "تم تغيير بيانات الدخول بنجاح ✔" : "البيانات الحالية غير صحيحة!",
                    "تغيير كلمة المرور", MessageBoxButton.OK,
                    ok ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
        }

        private void TxtPassword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnLogin_Click(sender, e);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
