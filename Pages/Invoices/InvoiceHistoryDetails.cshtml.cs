using Invoice_Generator.Data;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Invoice_Generator.Pages.Invoices
{
    [Authorize(Roles = "Admin,User,Analyzer")]
    public class InvoiceHistoryDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public InvoiceHistoryDetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Invoice Invoice { get; set; }
        public string CompanyLogoBase64 { get; set; }
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                Log.Warning("Invoice ID is null");
                return NotFound();
            }

            Invoice = await _context.invoices
                .Include(i => i.Client)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Invoice == null)
            {
                Log.Error($"Invoice with ID {id} not found");
                return NotFound();
            }

            if (Invoice.Company?.Logo != null && Invoice.Company.Logo.Length > 0)
            {
                CompanyLogoBase64 = $"data:image/png;base64,{Convert.ToBase64String(Invoice.Company.Logo)}";
            }

            Log.Information($"Invoice {id} details loaded successfully");

            return Page();
        }
    }
}