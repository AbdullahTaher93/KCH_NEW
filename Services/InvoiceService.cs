using KCH_New.Data;
using KCH_New.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KCH_New.Services
{
    public class InvoiceService
    {
        public async Task<Invoice> SaveInvoiceAsync(Invoice invoice)
        {
            using var db = new AppDbContext();
            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice?> UpdateInvoiceAsync(Invoice invoice)
        {
            using var db = new AppDbContext();
            // Delete old items then re-add
            var oldItems = db.InvoiceItems.Where(x => x.InvoiceId == invoice.Id);
            db.InvoiceItems.RemoveRange(oldItems);
            db.Invoices.Update(invoice);
            await db.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            using var db = new AppDbContext();
            return await db.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Invoice>> SearchByNameAsync(string name)
        {
            using var db = new AppDbContext();
            return await db.Invoices
                .Where(i => i.CustomerName.Contains(name))
                .OrderByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            using var db = new AppDbContext();
            return await db.Invoices.OrderByDescending(i => i.Id).ToListAsync();
        }

        public async Task DeleteOldestInvoicesAsync(int count)
        {
            using var db = new AppDbContext();
            var toDelete = await db.Invoices
                .OrderBy(i => i.Date)
                .Take(count)
                .Include(i => i.Items)
                .ToListAsync();
            db.Invoices.RemoveRange(toDelete);
            await db.SaveChangesAsync();
        }
    }
}
