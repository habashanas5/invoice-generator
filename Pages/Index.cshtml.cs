using Invoice_Generator.Data;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Invoice_Generator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public int TotalCustomers { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalCompanies { get; set; }

        // Modify the constructor to accept ApplicationDbContext as a parameter
        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;
            _userManager = userManager;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            // Count total users in the Identity database.
            TotalCustomers = await Task.Run(() => _userManager.Users.Count());
            TotalInvoices = _context.invoices.Count();
            TotalCompanies = _context.companies.Count();

        }
    }
}