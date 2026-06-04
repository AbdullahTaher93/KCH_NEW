using KCH_New.Models;
using KCH_New.Services;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace KCH_New.Views
{
    public partial class SearchWindow : Window
    {
        private readonly InvoiceService _svc = new();
        private Invoice? _selectedInvoice;
        public event Action<Invoice>? InvoiceSelected;

        public SearchWindow() => InitializeComponent();

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string txt = txtSearch.Text.Trim();
            List<Invoice> results;

            if (rbById.IsChecked == true)
            {
                if (int.TryParse(txt, out int id))
                {
                    var inv = await _svc.GetInvoiceByIdAsync(id);
                    results = inv != null ? new List<Invoice> { inv } : new List<Invoice>();
                }
                else results = new List<Invoice>();
            }
            else
            {
                results = await _svc.SearchByNameAsync(txt);
            }

            dgResults.ItemsSource = results;
            if (results.Count == 0)
                MessageBox.Show("لا توجد نتائج للبحث", "بحث", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtSearch.Text.Length < 1) return;
            if (rbByName.IsChecked == true)
            {
                var results = await _svc.SearchByNameAsync(txtSearch.Text.Trim());
                dgResults.ItemsSource = results;
            }
        }

        private async void DgResults_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgResults.SelectedItem is Invoice inv)
            {
                // Load with items
                _selectedInvoice = await _svc.GetInvoiceByIdAsync(inv.Id);
                if (_selectedInvoice == null) return;
                lblCustomer.Text = _selectedInvoice.CustomerName;
                lblDate.Text = _selectedInvoice.Date.ToString("yyyy/MM/dd");
                lblFinancial.Text = $"{_selectedInvoice.NetPrice:N0} / {_selectedInvoice.AmountPaid:N0} / {_selectedInvoice.Remaining:N0}";
                lblNotes.Text = _selectedInvoice.Notes;
                dgItems.ItemsSource = _selectedInvoice.Items;
            }
        }

        private void BtnOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInvoice == null)
            {
                MessageBox.Show("يرجى اختيار قائمة أولاً", "تنبيه");
                return;
            }
            InvoiceSelected?.Invoke(_selectedInvoice);
            Close();
        }

        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedInvoice == null)
            {
                MessageBox.Show("يرجى اختيار قائمة أولاً", "تنبيه");
                return;
            }
            new PrintPreviewWindow(_selectedInvoice) { Owner = this }.ShowDialog();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}
