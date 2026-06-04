using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KCH_New.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        [Required]
        public string CustomerName { get; set; } = string.Empty;

        public string CustomerAddress { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string Notes { get; set; } = string.Empty;

        public double TotalPrice { get; set; }

        public double Discount { get; set; }

        public double NetPrice { get; set; }

        public double AmountPaid { get; set; }

        public double Remaining { get; set; }

        public List<InvoiceItem> Items { get; set; } = new();
    }
}
