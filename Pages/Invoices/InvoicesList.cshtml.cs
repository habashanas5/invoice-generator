using Invoice_Generator.Data;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Invoice_Generator.Pages.Invoices
{
    [Authorize(Roles = "Admin,Analyzer")]
    public class InvoicesListModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InvoicesListModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Invoice> Invoices { get; set; }

        public async Task OnGetAsync()
        {
            Invoices = await _context.invoices
                .Include(i => i.Client)
                .Include(i => i.Company)
                .AsNoTracking()
                .ToListAsync();
            Log.Information("Invoices list fetched successfully.");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var invoice = await _context.invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _context.invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            Log.Information($"Invoice with ID {id} deleted successfully.");

            return RedirectToPage();
        }
    }
}
