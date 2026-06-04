using System.ComponentModel.DataAnnotations;

namespace KCH_New.Models
{
    public class InvoiceItem
    {
        public int Id { get; set; }

        public int InvoiceId { get; set; }

        public int ItemNumber { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public double Quantity { get; set; }

        public double UnitPrice { get; set; }

        public double TotalPrice { get; set; }

        public Invoice Invoice { get; set; } = null!;
    }
}
