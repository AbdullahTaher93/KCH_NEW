using KCH_New.Models;
using KCH_New.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KCH_New.Views
{
    public partial class InvoiceWindow : Window
    {
        private readonly InvoiceService _svc = new();
        private ObservableCollection<InvoiceItem> _items = new();
        private Invoice? _currentInvoice;

        public InvoiceWindow()
        {
            InitializeComponent();
            dpDate.SelectedDate = DateTime.Today;
            dgItems.ItemsSource = _items;
            SetNewMode();
        }

        // ─── Helpers ─────────────────────────────────────────────
        private void SetNewMode()
        {
            _items.Clear();
            _currentInvoice = null;
            txtCustomerName.Text = "";
            txtCustomerAddress.Text = "";
            txtNotes.Text = "";
            txtDiscount.Text = "0";
            dpDate.SelectedDate = DateTime.Today;
            lblTotal.Text = "0";
            lblNet.Text = "0";
            lblPaid.Text = "0";
            lblRemaining.Text = "0";
            txtInvoiceNumber.Text = "قائمة جديدة";
            txtCustomerName.IsReadOnly = false;
            txtCustomerAddress.IsReadOnly = false;
        }

        private void RecalcTotals()
        {
            int row = 1;
            foreach (var item in _items)
            {
                item.ItemNumber = row++;
                item.TotalPrice = item.Quantity * item.UnitPrice;
            }
            double total = _items.Sum(x => x.TotalPrice);
            double discount = double.TryParse(txtDiscount.Text, out var d) ? d : 0;
            double net = total - discount;
            lblTotal.Text = total.ToString("N0");
            lblNet.Text = net.ToString("N0");
        }

        private void LoadInvoice(Invoice inv)
        {
            _currentInvoice = inv;
            txtInvoiceNumber.Text = $"رقم القائمة: {inv.Id}";
            txtCustomerName.Text = inv.CustomerName;
            txtCustomerAddress.Text = inv.CustomerAddress;
            dpDate.SelectedDate = inv.Date;
            txtNotes.Text = inv.Notes;
            txtDiscount.Text = inv.Discount.ToString();
            lblTotal.Text = inv.TotalPrice.ToString("N0");
            lblNet.Text = inv.NetPrice.ToString("N0");
            lblPaid.Text = inv.AmountPaid.ToString("N0");
            lblRemaining.Text = inv.Remaining.ToString("N0");
            _items = new ObservableCollection<InvoiceItem>(inv.Items);
            dgItems.ItemsSource = _items;
        }

        // ─── Events ───────────────────────────────────────────────
        private void BtnNew_Click(object sender, RoutedEventArgs e) => SetNewMode();

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCustomerName.Text))
            {
                MessageBox.Show("يرجى إدخال اسم الزبون", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var realItems = _items.Where(i => !string.IsNullOrWhiteSpace(i.Name)).ToList();
            if (realItems.Count == 0)
            {
                MessageBox.Show("يرجى إضافة مادة واحدة على الأقل", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            double total = realItems.Sum(x => x.TotalPrice);
            double discount = double.TryParse(txtDiscount.Text, out var d) ? d : 0;
            double net = total - discount;

            // Ask for paid amount
            var payDlg = new PaymentDialog(net) { Owner = this };
            if (payDlg.ShowDialog() != true) return;
            double paid = payDlg.AmountPaid;
            double remaining = net - paid;

            var invoice = _currentInvoice ?? new Invoice();
            invoice.CustomerName = txtCustomerName.Text.Trim();
            invoice.CustomerAddress = txtCustomerAddress.Text.Trim();
            invoice.Date = dpDate.SelectedDate ?? DateTime.Today;
            invoice.Notes = txtNotes.Text.Trim();
            invoice.TotalPrice = total;
            invoice.Discount = discount;
            invoice.NetPrice = net;
            invoice.AmountPaid = paid;
            invoice.Remaining = remaining;
            invoice.Items = realItems.Select((it, idx) => new InvoiceItem
            {
                ItemNumber = idx + 1,
                Name = it.Name,
                Quantity = it.Quantity,
                UnitPrice = it.UnitPrice,
                TotalPrice = it.TotalPrice
            }).ToList();

            if (_currentInvoice == null)
                await _svc.SaveInvoiceAsync(invoice);
            else
                await _svc.UpdateInvoiceAsync(invoice);

            _currentInvoice = invoice;
            lblPaid.Text = paid.ToString("N0");
            lblRemaining.Text = remaining.ToString("N0");
            txtInvoiceNumber.Text = $"رقم القائمة: {invoice.Id}";

            MessageBox.Show("تم حفظ البيانات بنجاح ✔", "نجاح",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_currentInvoice == null)
            {
                MessageBox.Show("يرجى حفظ القائمة أولاً قبل الطباعة", "تنبيه",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            new PrintPreviewWindow(_currentInvoice) { Owner = this }.ShowDialog();
        }

        private async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox(
                "أدخل عدد القوائم المراد حذفها (سيتم حذف الأقدم أولاً):",
                "حذف قوائم", "10");
            if (!int.TryParse(input, out int count) || count <= 0)
            {
                MessageBox.Show("يرجى إدخال عدد صحيح أكبر من صفر", "تنبيه");
                return;
            }
            if (MessageBox.Show($"سيتم حذف {count} قائمة من الأقدم، هل أنت متأكد؟",
                "تأكيد الحذف", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _svc.DeleteOldestInvoicesAsync(count);
                MessageBox.Show("تم الحذف بنجاح ✔", "نجاح");
            }
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            var w = new SearchWindow();
            w.InvoiceSelected += inv => LoadInvoice(inv);
            w.Owner = this;
            w.ShowDialog();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }

        private void TxtDiscount_TextChanged(object sender, TextChangedEventArgs e) => RecalcTotals();

        private void DgItems_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e) => RecalcTotals();
    }
}
