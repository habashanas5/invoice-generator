using Invoice_Generator.Data;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Invoice_Generator.Pages.Admin.Dashboards
{
    [Authorize(Roles = "Admin,Analyzer")]
    public class DefaultDashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.ApplicationUser> _userManager;

        public DefaultDashboardModel(ApplicationDbContext context , UserManager<Models.ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public int TotalInvoices { get; set; }
        public decimal TotalInvoiceAmount { get; set; }
        public decimal TotalPayments { get; set; }
        public int UnpaidInvoices { get; set; }
        public int UpcomingDueInvoices { get; set; }
        public List<Models.Invoice> LatestInvoices { get; set; }
        public int TotalClients { get; set; }
        public int TotalCompanies { get; set; }
        public int TotalUser { get; set; }
        public int NewUsersThisMonth { get; set; }
        public Dictionary<string, int> CurrencyCounts { get; set; }
        public int PaidInvoices { get; set; } 

        public void OnGet()
        {
            TotalInvoices = _context.invoices.Count();
            TotalInvoiceAmount = _context.invoices.Sum(i => i.Total);
            TotalPayments = _context.invoices.Sum(i => i.AmountPaid);
            //UnpaidInvoices = _context.invoices.Count(i => i.BalanceDue > 0);
            UnpaidInvoices = _context.invoices.Count(i => !i.IsPaid); 
            PaidInvoices = _context.invoices.Count(i => i.IsPaid); 
            UpcomingDueInvoices = _context.invoices.Count(i => i.DueDate <= DateTime.UtcNow.AddDays(7));
            LatestInvoices = _context.invoices
                .Include(i => i.Client)  
                .Include(i => i.Company) 
                .OrderByDescending(i => i.Date)
                .Take(5)
                .ToList();

            TotalClients = _context.clients.Count();
            TotalCompanies = _context.companies.Count();
            TotalUser = TotalUser = _userManager.Users.Count();
            NewUsersThisMonth = _userManager.Users.Count(u => u.CreatedAt.Month == DateTime.UtcNow.Month);

            CurrencyCounts = _context.invoices.GroupBy(i => i.Currency).ToDictionary(g => g.Key, g => g.Count());
        }
    }
}
