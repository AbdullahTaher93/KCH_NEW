using System.Windows;
using System.Windows.Input;

namespace KCH_New.Views
{
    public partial class PaymentDialog : Window
    {
        public double AmountPaid { get; private set; }

        public PaymentDialog(double netAmount)
        {
            InitializeComponent();
            lblNetAmount.Text = $"المبلغ الصافي المطلوب: {netAmount:N0} د.ع";
            txtPaid.Text = netAmount.ToString("N0").Replace(",", "");
            txtPaid.SelectAll();
            txtPaid.Focus();
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(txtPaid.Text, out double val) || val < 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح", "تنبيه");
                return;
            }
            AmountPaid = val;
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        private void TxtPaid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) BtnConfirm_Click(sender, e);
        }
    }
}
