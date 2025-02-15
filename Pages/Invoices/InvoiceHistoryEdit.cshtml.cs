using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Invoice_Generator.Data;
using Invoice_Generator.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Invoice_Generator.Infrastructures.Currencies;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Invoice_Generator.Pages.Invoices
{
    [Authorize(Roles = "Admin,User,Analyzer")]
    public class InvoiceHistoryEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrencyService _currencyService;

        public InvoiceHistoryEditModel(ApplicationDbContext context, ICurrencyService currencyService)
        {
            _context = context;
            _currencyService = currencyService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public ICollection<SelectListItem> CurrencyLookup { get; set; } = new List<SelectListItem>();

        private void BindLookup()
        {
            CurrencyLookup = _currencyService.GetCurrencies() ?? new List<SelectListItem>();
        }
        public class InputModel
        {
            [Required]
            [Display(Name = "Invoice Number")]
            public string InvoiceNumber { get; set; }

            [Required]
            [Display(Name = "Date")]
            public DateTime Date { get; set; } = DateTime.Today;

            public byte[]? Logo { get; set; }

            [Required]
            [Display(Name = "Payment Terms")]
            public string? PaymentTerms { get; set; }

            [Required]
            [Display(Name = "Due Date")]
            public DateTime? DueDate { get; set; }

            [Required]
            [Display(Name = "PONumber")]
            public string? PONumber { get; set; }

            [Display(Name = "Notes")]
            public string? Notes { get; set; }

            [Display(Name = "Terms")]
            public string? Terms { get; set; }

            [Required]
            [Display(Name = "Company Name")]
            public string? CompanyName { get; set; }

            [Required]
            [Display(Name = "Company Address")]
            public string? CompanyAddress { get; set; }

            [Required]
            [Display(Name = "Client Name")]
            public string? ClientName { get; set; }

            [Required]
            [Display(Name = "Client Email")]
            public string? ClientEmail { get; set; }

            [Display(Name = "SubTotal")]
            public decimal SubTotal { get; set; }

            [Display(Name = "Discount")]
            public decimal Discount { get; set; }

            [Display(Name = "TaxAmount")]
            public decimal TaxAmount { get; set; }

            [Display(Name = "Shipping")]
            public decimal Shipping { get; set; }

            [Display(Name = "Amount Paid")]
            public decimal AmountPaid { get; set; }

            [Display(Name = "Total")]
            public decimal Total { get; set; }

            [Display(Name = "Balance Due")]
            public decimal BalanceDue { get; set; }

            [Required]
            [Display(Name = "Currency")]
            public string Currency { get; set; } = "USD";

            [Display(Name = "IsPaid")]
            public bool IsPaid { get; set; } = false;

            [Display(Name = "Items")]
            public List<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

            public byte[]? ExistingCompanyLogo { get; set; }  
            
            [BindProperty]
            public IFormFile? CompanyLogoFile { get; set; }  

        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var invoice = await _context.invoices
                .Where(i => i.Id == id)
                .Include(i => i.Client)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems) 
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                Log.Warning("Invoice with ID {InvoiceId} not found.", id);
                return NotFound();
            }

            Input = new InputModel
            {
                InvoiceNumber = invoice.InvoiceNumber,
                Date = invoice.Date,
                DueDate = invoice.DueDate,
                PONumber = invoice.PONumber,
                PaymentTerms = invoice.PaymentTerms,
                CompanyName = invoice.Company.Name,
                CompanyAddress = invoice.Company.Address,
                ClientName = invoice.Client.Name,
                ClientEmail = invoice.Client.Email,
                SubTotal = invoice.SubTotal,
                Discount = invoice.Discount,
                TaxAmount = invoice.TaxAmount,
                Shipping = invoice.Shipping,
                Total = invoice.Total,
                AmountPaid = invoice.AmountPaid,
                BalanceDue = invoice.BalanceDue,
                Currency = invoice.Currency,
                Notes = invoice.Notes,
                Terms = invoice.Terms,
                Items = invoice.InvoiceItems.ToList(),
                ExistingCompanyLogo = invoice.Company.Logo,
                IsPaid = invoice.IsPaid
            };

            BindLookup();

            Log.Information("Invoice details loaded successfully for Invoice ID {InvoiceId}.", id);

            return Page();
        }

        public IActionResult OnGetCompanyLogo(int id)
        {
            var invoice = _context.invoices.Find(id);
            if (invoice == null || invoice.Company.Logo == null)
            {
                Log.Warning("Logo for invoice with ID {InvoiceId} not found.", id);
                return NotFound();
            }

            Log.Information("Returning logo for Invoice ID {InvoiceId}.", id);
            return File(invoice.Company.Logo, "image/png"); 
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                Log.Warning("Invoice ID is null for post request.");
                return NotFound();
            }

            var invoice = await _context.invoices
                .Include(i => i.InvoiceItems)
                .Include(i => i.Company) 
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            invoice.InvoiceNumber = Input.InvoiceNumber;
            invoice.Date = Input.Date;
            invoice.PaymentTerms = Input.PaymentTerms;
            invoice.DueDate = (DateTime)Input.DueDate;
            invoice.PONumber = Input.PONumber;
            invoice.Notes = Input.Notes;
            invoice.Terms = Input.Terms;
            invoice.SubTotal = Input.SubTotal;
            invoice.Discount = Input.Discount;
            invoice.TaxAmount = Input.TaxAmount;
            invoice.Shipping = Input.Shipping;
            invoice.AmountPaid = Input.AmountPaid;
            invoice.Total = Input.Total;
            invoice.BalanceDue = Input.BalanceDue;
            invoice.Currency = Input.Currency;
            invoice.IsPaid = Input.IsPaid;
            invoice.InvoiceItems = Input.Items;

            if (invoice.Company == null)
            {
                invoice.Company = new Company
                {
                    Name = Input.CompanyName,
                    Address = Input.CompanyAddress,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                invoice.Company.Name = Input.CompanyName;
                invoice.Company.Address = Input.CompanyAddress;
                invoice.Company.UpdatedAt = DateTime.UtcNow;

                if (Input.CompanyLogoFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Input.CompanyLogoFile.CopyToAsync(memoryStream);
                        invoice.Company.Logo = memoryStream.ToArray();
                    }
                }
            }

            if (invoice.Client == null)
            {
                invoice.Client = new Client
                {
                    Name = Input.ClientName,
                    Email = Input.ClientEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                invoice.Client.Name = Input.ClientName;
                invoice.Client.Email = Input.ClientEmail;
                invoice.Client.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var item in Input.Items)
            {
                var existingItem = invoice.InvoiceItems
                    .FirstOrDefault(i => i.Description == item.Description);

                if (existingItem != null)
                {
                    existingItem.Quantity = item.Quantity;
                    existingItem.Rate = item.Rate;
                    existingItem.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newItem = new InvoiceItem
                    {
                        Description = item.Description,
                        Quantity = item.Quantity,
                        Rate = item.Rate,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    invoice.InvoiceItems.Add(newItem);
                    _context.invoiceItems.Add(newItem);
                }
            }

            try
            {
                Log.Information("Invoice with ID {InvoiceId} updated successfully.", id);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id.Value))
                {
                    Log.Warning("Invoice with ID {InvoiceId} does not exist.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = "Edit invoice successfully!";

            Log.Information("Invoice with ID {InvoiceId} update operation completed.", id);
            return RedirectToPage("/Invoices/InvoiceHistory");
        }

        private bool InvoiceExists(int id)
        {
            return _context.invoices.Any(e => e.Id == id);
        }
    }
}