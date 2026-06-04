using System.Windows;

namespace KCH_New.Views
{
    public partial class ChangePasswordDialog : Window
    {
        public string OldUsername { get; private set; } = "";
        public string OldPassword { get; private set; } = "";
        public string NewUsername { get; private set; } = "";
        public string NewPassword { get; private set; } = "";

        public ChangePasswordDialog() => InitializeComponent();

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtOldUser.Text) ||
                string.IsNullOrWhiteSpace(txtOldPass.Password) ||
                string.IsNullOrWhiteSpace(txtNewUser.Text) ||
                string.IsNullOrWhiteSpace(txtNewPass.Password))
            {
                MessageBox.Show("يرجى ملء جميع الحقول", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            OldUsername = txtOldUser.Text.Trim();
            OldPassword = txtOldPass.Password;
            NewUsername = txtNewUser.Text.Trim();
            NewPassword = txtNewPass.Password;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
