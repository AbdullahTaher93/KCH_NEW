using KCH_New.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace KCH_New.Views
{
    public partial class PrintPreviewWindow : Window
    {
        private readonly Invoice _invoice;

        public PrintPreviewWindow(Invoice invoice)
        {
            InitializeComponent();
            _invoice = invoice;
            LoadData();
        }

        private void LoadData()
        {
            lblInvNo.Text = $"رقم القائمة: {_invoice.Id}";
            lblCustomer.Text = $"اسم الزبون: {_invoice.CustomerName}";
            lblAddress.Text = $"العنوان: {_invoice.CustomerAddress}";
            lblDate.Text = $"التاريخ: {_invoice.Date:yyyy/MM/dd}";
            lblNotes.Text = string.IsNullOrWhiteSpace(_invoice.Notes) ? "" : $"ملاحظات: {_invoice.Notes}";
            dgItems.ItemsSource = _invoice.Items;
            lblTotal.Text = $"{_invoice.TotalPrice:N0} د.ع";
            lblDiscount.Text = $"{_invoice.Discount:N0} د.ع";
            lblNet.Text = $"{_invoice.NetPrice:N0} د.ع";
            lblPaid.Text = $"{_invoice.AmountPaid:N0} د.ع";
            lblRemaining.Text = $"{_invoice.Remaining:N0} د.ع";
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            var pd = new PrintDialog();
            if (pd.ShowDialog() == true)
            {
                // Make the printArea a Visual for printing
                printArea.Measure(new System.Windows.Size(pd.PrintableAreaWidth, double.PositiveInfinity));
                printArea.Arrange(new System.Windows.Rect(new System.Windows.Size(pd.PrintableAreaWidth, printArea.DesiredSize.Height)));
                pd.PrintVisual(printArea, $"فاتورة رقم {_invoice.Id}");
            }
        }
    }
}
