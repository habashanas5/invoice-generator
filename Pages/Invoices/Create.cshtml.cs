using Invoice_Generator.Applications;
using Invoice_Generator.Data;
using Invoice_Generator.Infrastructures.Currencies;
using Invoice_Generator.Infrastructures.Pdfs;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Invoice_Generator.Pages.Invoices
{
    [Authorize(Roles = "Admin,User,Analyzer")]
    public class CreateModel : PageModel
    {
        private readonly ICurrencyService _currencyService;
        private readonly IInvoiceService _invoiceService;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.ApplicationUser> _userManager;
        private readonly IPdfService _pdfService;
        private readonly IEmailSender _emailSender;

        public CreateModel
            (
            ICurrencyService currencyService,
            IInvoiceService invoiceService,
            ApplicationDbContext context,
            UserManager<Models.ApplicationUser> userManager,
            IPdfService pdfService,
            IEmailSender emailSender
            )
        {
            _currencyService = currencyService;
            _context = context;
            _invoiceService = invoiceService;
            _userManager = userManager;
            _pdfService = pdfService;
            _emailSender = emailSender;
        }

        public ICollection<SelectListItem> CurrencyLookup { get; set; } = new List<SelectListItem>();

        public string SelectedCurrency { get; set; } = string.Empty;

        [BindProperty]
        public InputModel Input { get; set; }

        [TempData]
        public int? LastInvoiceId { get; set; }

        public void OnGet()
        {
            Input = new InputModel
            {
                Date = DateTime.Today
            };
            BindLookup();
        }

        private string GenerateInvoiceNumber()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
        }

        public class InputModel
        {

            [Required(ErrorMessage = "Invoice Number is required.")]
            [Display(Name = "Invoice Number")]
            [RegularExpression(@"^\d+$", ErrorMessage = "Please enter a valid number.")]
            public  string InvoiceNumber { get; set; }
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
            [RegularExpression(@"^[\d\w]+$", ErrorMessage = "Please enter a valid PO number (alphanumeric).")]
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
            [Display(Name = "ClientEmail")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            public string? ClientEmail { get; set; }

            [Display(Name = "SubTotal")]
            public decimal SubTotal { get; set; }

            [Display(Name = "Discount")]
            [Range(0, double.MaxValue, ErrorMessage = "Discount must be a positive number.")]
            public decimal Discount { get; set; }
            
            [Display(Name = "TaxAmount")]
            [Range(0, double.MaxValue, ErrorMessage = "Tax Amount must be a positive number.")]
            public decimal TaxAmount { get; set; }

            [Display(Name = "Shipping")]
            [Range(0, double.MaxValue, ErrorMessage = "Shipping must be a positive number.")]
            public decimal Shipping { get; set; }

            [Display(Name = "Amount Paid")]
            [Range(0, double.MaxValue, ErrorMessage = "Amount Paid must be a positive number.")]
            public decimal AmountPaid { get; set; }

            [Display(Name = "Total")]
            public decimal Total { get; set; }

            [Display(Name = "BalanceDue")]
            public decimal BalanceDue { get; set; }

            [Required]
            [Display(Name = "Currency")]
            public string Currency { get; set; } = "USD";

            public List<InvoiceItemInput> InvoiceItems { get; set; }

            public class InvoiceItemInput
            {
                public string Description { get; set; }
                public int Quantity { get; set; }
                public decimal Rate { get; set; }
                public decimal Total { get; set; }
            }
        }

        public async Task<IActionResult> OnPostSave()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(Input.InvoiceNumber))
            {
                Input.InvoiceNumber = GenerateInvoiceNumber();
            }

            var client = await _context.clients.FirstOrDefaultAsync(c => c.Email == Input.ClientEmail);
            if (client == null)
            {
                client = new Client
                {
                    Name = Input.ClientName,
                    Email = Input.ClientEmail,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                await _invoiceService.SaveClientAsync(client);
            }

            var company = await _context.companies.FirstOrDefaultAsync(c => c.Name == Input.CompanyName);
            if (company == null)
            {
                company = new Company
                {
                    Name = Input.CompanyName,
                    Address = Input.CompanyAddress,  
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files.FirstOrDefault();
                    if (file != null && file.Length > 0)
                    {
                        using (var dataStream = new MemoryStream())
                        {
                            await file.CopyToAsync(dataStream);
                            company.Logo = dataStream.ToArray();
                        }
                    }
                }

                await _invoiceService.SaveCompanyAsync(company);
            }

            var invoice = new Invoice
            {
                InvoiceNumber = Input.InvoiceNumber,
                Date = Input.Date,
                PaymentTerms = Input.PaymentTerms,
                DueDate = (DateTime)Input.DueDate,
                PONumber = Input.PONumber,
                SubTotal = Input.SubTotal,
                Discount = Input.Discount,
                TaxAmount = Input.TaxAmount,
                Shipping = Input.Shipping,
                AmountPaid = Input.AmountPaid,
                Total = Input.Total,
                BalanceDue = Input.BalanceDue,
                Currency = Input.Currency,
                Notes = Input.Notes,
                Terms = Input.Terms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ApplicationUserId = userId,
                ClientId = client.Id,
                CompanyId = company.Id
            };

            await _invoiceService.SaveInvoiceAsync(invoice);

            if (Input.InvoiceItems != null && Input.InvoiceItems.Any())
            {
                var invoiceItems = Input.InvoiceItems.Select(item => new InvoiceItem
                {
                    InvoiceId = invoice.Id,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }).ToList();

                await _invoiceService.SaveInvoiceItemsAsync(invoiceItems);
            }

            TempData["SuccessMessage"] = "Invoice saved successfully!";
            // return RedirectToPage("/Invoices/Create");
            //return RedirectToPage("/Invoices/Download", new { id = invoice.Id });
            TempData["LastInvoiceId"] = invoice.Id;
            //return Page();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDownload(int id)
        {
            Log.Information("Download request received for invoice with ID: {InvoiceId}", id);

            // Fetch the invoice data from the database
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);

            if (invoice == null)
            {
                Log.Warning("Invoice with ID {InvoiceId} not found.", id);
                return NotFound();
            }

            // Generate HTML content for the invoice
            string htmlContent = GenerateInvoiceHtml(invoice);

            // Generate the PDF
            byte[] pdfBytes = _pdfService.CreatePdfFromHtml(htmlContent, $"Invoice_{invoice.InvoiceNumber}");

            Log.Information("PDF generated successfully for invoice {InvoiceId}.", id);

            // Return the PDF as a file download
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }

        private string GenerateInvoiceHtml(Invoice invoice)
        {
            string logoHtml = "";
            if (invoice.Company.Logo != null)
            {
                string base64Image = Convert.ToBase64String(invoice.Company.Logo);
                logoHtml = $"<img src='data:image/png;base64,{base64Image}' alt='Company Logo' class='logo' />";
            }
            var htmlContent = $@"
           <html>
            <head>
                <title>Invoice {invoice.InvoiceNumber}</title>
                <style>
                    body {{
                        font-family: 'Roboto', sans-serif;
                        margin: 0;
                        padding: 0;
                        width: 100%;
                        height: 100%;
                        background-color: #f7f7f7;
                        display: flex;
                        justify-content: center;
                        align-items: center;
                    }}
                    .invoice-container {{
                        width: 95%;
                        max-width: 850px;
                        margin: 20px auto;
                        padding: 40px;
                        box-sizing: border-box;
                        background-color: #ffffff;
                        border-radius: 10px;
                        border: 1px solid #e0e0e0;
                        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
                        position: relative;
                    }}
                    .header {{
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        margin-bottom: 30px;
                    }}
                    .logo {{
                        max-width: 190px;
                        max-height: 135px;
                        object-fit: contain;
                        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
                    }}
                    .invoice-title {{
                        font-size: 36px;
                        font-weight: bold;
                        text-align: right;
                        color: #1e2a47;
                        margin: 0;
                    }}
                    .invoice-number-details {{
                        text-align: right;
                        margin-top: 15px;
                        color: #555;
                    }}
                    .invoice-number-details p {{
                        margin: 8px 0;
                        font-size: 16px;
                        color: #444;
                    }}
                    .invoice-details table {{
                        width: 100%;
                        margin-bottom: 20px;
                        border-collapse: collapse;
                    }}
                    .invoice-details td {{
                        padding: 12px;
                        vertical-align: top;
                        background-color: #fff;
                        border: none;
                     }}
                    .invoice-details td strong {{
                        color: #1e2a47;
                        font-size: 16px;
                    }}
                    .invoice-items {{
                        width: 100%;
                        margin-top: 30px;
                    }}
                    .invoice-items table {{
                        width: 100%;
                        border-collapse: collapse;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
                    }}
                    .invoice-items th, .invoice-items td {{
                        padding: 14px;
                        border: 1px solid #e0e0e0;
                        text-align: left;
                        font-size: 15px;
                    }}
                    .invoice-items th {{
                        background-color: #1e2a47;
                        color: #fff;
                        text-align: center;
                        font-weight: 600;
                    }}
                    .invoice-items td:first-child {{
                        width: 50%;
                        text-align: left;
                    }}
                    .invoice-items th, .invoice-items td {{
                        width: 12%;
                        text-align: center;
                    }}
                    .totals {{
                        text-align: right;
                        margin-top: 30px;
                    }}
                    .totals table {{
                        width: 100%;
                        border-collapse: collapse;
                        border-radius: 8px;
                        overflow: hidden;
                        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
                    }}
                    .totals td {{
                        padding: 14px;
                        border: 1px solid #e0e0e0;
                        background-color: #f9f9f9;
                    }}
                    .totals .total {{
                        font-size: 22px;
                        font-weight: bold;
                        color: #1e2a47;
                    }}
                    .footer {{
                        margin-top: 40px;
                        text-align: center;
                        font-size: 16px;
                        color: #777;
                        padding: 20px;
                        background-color: #f5f5f5;
                        border-radius: 8px;
                        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.1);
                    }}
                    @media print {{
                        body {{
                            font-size: 14px;
                        }}
                        .invoice-container {{
                            padding: 30px;
                            box-shadow: none;
                            border: none;
                        }}
                    }}
                </style>
            </head>
            <body>
                <div class='invoice-container'>
                    <div class='header'>
                        <div class='logo'>{logoHtml}</div>
                        <div class='invoice-title'>INVOICE</div>
                    </div>

                    <div class='invoice-number-details'>
                        <p><strong>Currency:</strong> {invoice.Currency}</p>
                        <p><strong>Total: {invoice.Currency} {invoice.Total.ToString("N2")}</strong></p>
                    </div>

                    <br><br>

                    <div class='invoice-details'>
                        <table>
                            <tr>
                                <td><strong>Client Information: </strong><br>Client Name: {invoice.Client.Name}<br>Client Email: {invoice.Client.Email}</td>
                                <td><strong>Company Information: </strong><br>Company Name: {invoice.Company.Name}<br>Company Address: {invoice.Company.Address}</td>
                                <td><strong>Invoice Number: {invoice.InvoiceNumber}<br>Date: {invoice.Date.ToShortDateString()}<br>Due Date: {invoice.DueDate.ToShortDateString()}<br>PONumber: {invoice.PONumber}</td>
                            </tr>
                        </table>
                    </div>

                    <hr>

                    <div class='invoice-items'>
                        <table>
                            <thead>
                                <tr>
                                    <th>Item</th>
                                    <th>Rate</th>
                                    <th>Quantity</th>
                                    <th>Total</th>
                                </tr>
                            </thead>
                            <tbody>";

            foreach (var item in invoice.InvoiceItems)
            {
                htmlContent += $@"
                                <tr>
                                    <td>{item.Description}</td>
                                    <td>{invoice.Currency} {item.Rate.ToString("N2")}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{invoice.Currency} {item.Total.ToString("N2")}</td>
                                </tr>";
            }

            htmlContent += $@"
                            </tbody>
                        </table>
                    </div>

                    <div class='totals'>
                        <table>
                            <tr>
                                <td>Payment Terms:</td>
                                <td>{invoice.PaymentTerms}</td>
                            </tr>
                             <tr>
                                <td>Terms:</td>
                                <td>{invoice.Terms}</td>
                            </tr>
                            <tr>
                                <td>Notes:</td>
                                <td>{invoice.Notes}</td>
                            </tr>
                            <tr>
                                <td>Subtotal:</td>
                                <td>{invoice.Currency} {invoice.SubTotal.ToString("N2")}</td>
                            </tr>
                            <tr>
                                <td>Discount:</td>
                                <td>{invoice.Currency} {invoice.Discount.ToString("N2")}</td>
                            </tr>
                            <tr>
                                <td>Tax:</td>
                                <td>{invoice.Currency} {invoice.TaxAmount.ToString("N2")}</td>
                            </tr>
                            <tr>
                                <td>Shipping:</td>
                                <td>{invoice.Currency} {invoice.Shipping.ToString("N2")}</td>
                            </tr>
                            <tr class='total'>
                                <td>Total:</td>
                                <td>{invoice.Currency} {invoice.Total.ToString("N2")}</td>
                            </tr>
                            <tr class='total'>
                                <td>Amount Paid:</td>
                                <td>{invoice.Currency} {invoice.AmountPaid.ToString("N2")}</td>
                            </tr>
                            <tr class='total'>
                                <td>Balance Due:</td>
                                <td>{invoice.Currency} {invoice.BalanceDue.ToString("N2")}</td>
                            </tr>           
                        </table>
                    </div>

                    <div class='footer'>
                        <p>Thank you for your business!</p>
                    </div>
                </div>
            </body>
        </html>";

            return htmlContent;
        }

        public async Task<IActionResult> OnPostDuplicate(int id)
        {
            Log.Information($"Attempting to duplicate invoice with ID {id}.");

            // Fetch the existing invoice from the database
            var existingInvoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (existingInvoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found!";
                Log.Warning($"Invoice with ID {id} not found for duplication.");
                return Page();
            }

            // Create a new invoice with the same details as the existing one
            var newInvoice = new Invoice
            {
                InvoiceNumber = GenerateInvoiceNumber(), 
                Date = existingInvoice.Date,
                PaymentTerms = existingInvoice.PaymentTerms,
                DueDate = existingInvoice.DueDate,
                PONumber = existingInvoice.PONumber,
                SubTotal = existingInvoice.SubTotal,
                Discount = existingInvoice.Discount,
                TaxAmount = existingInvoice.TaxAmount,
                Shipping = existingInvoice.Shipping,
                AmountPaid = existingInvoice.AmountPaid,
                Total = existingInvoice.Total,
                BalanceDue = existingInvoice.BalanceDue,
                Currency = existingInvoice.Currency,
                Notes = existingInvoice.Notes,
                Terms = existingInvoice.Terms,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                ApplicationUserId = existingInvoice.ApplicationUserId,
                ClientId = existingInvoice.ClientId,
                CompanyId = existingInvoice.CompanyId
            };

            // Save the new invoice to the database
            await _invoiceService.SaveInvoiceAsync(newInvoice);

            // Duplicate the invoice items if any
            var existingInvoiceItems = await _invoiceService.GetInvoiceItemsByInvoiceIdAsync(existingInvoice.Id);
            if (existingInvoiceItems != null && existingInvoiceItems.Any())
            {
                var newInvoiceItems = existingInvoiceItems.Select(item => new InvoiceItem
                {
                    InvoiceId = newInvoice.Id,
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                }).ToList();

                await _invoiceService.SaveInvoiceItemsAsync(newInvoiceItems);
            }
            Log.Information($"Invoice with ID {id} duplicated successfully.");

            TempData["SuccessMessage"] = "Invoice duplicated successfully!";
            TempData["LastInvoiceId"] = newInvoice.Id;
            return RedirectToPage();
            // return RedirectToPage("/Invoices/InvoiceHistory");
        }

        private void BindLookup()
        {
            CurrencyLookup = _currencyService.GetCurrencies() ?? new List<SelectListItem>(); // Null check if the service returns null
        }

        public async Task<IActionResult> OnPostSendEmail(string email, int invoiceId)
        {
            Log.Information("Starting to send email for invoice ID {InvoiceId} to {Email}", invoiceId, email);

            if (invoiceId == 0)
            {
                TempData["ErrorMessage"] = "No invoice found to send.";
                return RedirectToPage();
            }

            try
            {
                await _invoiceService.SendInvoiceEmailAsync(email, invoiceId);
                TempData["SuccessMessage"] = "Email sent successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error sending email: {ex.Message}";
            }

            return RedirectToPage(); 
        }
    }
}
