using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Invoice_Generator.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Invoice_Generator.Infrastructures.Pdfs;
using Invoice_Generator.Applications;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Drawing;
using Serilog;

namespace Invoice_Generator.Pages.Invoices
{
    [Authorize(Roles = "Admin,User,Analyzer")]
    public class InvoiceHistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IPdfService _pdfService;
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailSender _emailSender;

        public InvoiceHistoryModel(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IPdfService pdfService,
            IEmailSender emailSender,
            IInvoiceService invoiceService
         )
        {
            _userManager = userManager;
            _context = context;
            _pdfService = pdfService;
            _emailSender = emailSender;
            _invoiceService = invoiceService;
            Invoices = new List<Invoice>();
        }

        public List<Invoice> Invoices { get; set; }

        public async Task OnGetAsync()
        {
            Invoices = await _invoiceService.GetInvoicesAsync();

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                Log.Information($"Fetching invoices for user {user.UserName}.");
                Invoices = await _context.invoices
                    .Include(i => i.Client)
                    .Include(i => i.Company)
                    .Include(i => i.InvoiceItems)
                    .Where(i => i.ApplicationUserId == user.Id)
                    .OrderByDescending(i => i.CreatedAt)
                    .ToListAsync();
                Log.Information($"Loaded {Invoices.Count} invoices for user {user.UserName}.");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            Log.Information($"Attempting to delete invoice with ID {id}.");
            var invoice = await _context.invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                Log.Information($"Invoice with ID {id} deleted successfully.");
            }

            TempData["SuccessMessage"] = "Invoice deleted successfully!";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDuplicateAsync(int id)
        {
            Log.Information($"Attempting to duplicate invoice with ID {id}.");
            var invoice = await _context.invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == id);
            if (invoice == null)
            {
                Log.Warning($"Invoice with ID {id} not found for duplication.");
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);


            var duplicateInvoice = new Invoice
            {
                InvoiceNumber = "Copy-" + invoice.InvoiceNumber,
                Date = DateTime.UtcNow,
                DueDate = invoice.DueDate,
                ClientId = invoice.ClientId,
                CompanyId = invoice.CompanyId,
                PaymentTerms = invoice.PaymentTerms,
                PONumber = invoice.PONumber,
                Notes = invoice.Notes,
                Terms = invoice.Terms,
                SubTotal = invoice.SubTotal,
                Discount = invoice.Discount,
                Shipping = invoice.Shipping,
                TaxAmount = invoice.TaxAmount,
                Total = invoice.Total,
                AmountPaid = invoice.AmountPaid,
                BalanceDue = invoice.BalanceDue,
                Currency = invoice.Currency,
                ApplicationUserId = userId,
            };

            _context.invoices.Add(duplicateInvoice);
            await _context.SaveChangesAsync();

            foreach (var item in invoice.InvoiceItems)
            {
                var duplicateItem = new InvoiceItem
                {
                    Description = item.Description,
                    Quantity = item.Quantity,
                    Rate = item.Rate,
                    InvoiceId = duplicateInvoice.Id 
                };

                _context.invoiceItems.Add(duplicateItem);
            }

            await _context.SaveChangesAsync();

            Log.Information($"Invoice with ID {id} duplicated successfully.");

            TempData["SuccessMessage"] = "Invoice duplicated successfully!";

            return RedirectToPage("/Invoices/InvoiceHistory");
        }

        public async Task<IActionResult> OnPostSendEmailAsync(int invoiceId, string email)
        {
            Log.Information("Starting to send email for invoice ID {InvoiceId} to {Email}", invoiceId, email);
            var invoice = await _context.invoices
                .Include(i => i.Client)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            var client = invoice.Client;
            var company = invoice.Company;

            string logoBase64 = string.Empty;
            if (company?.Logo != null)
            {
                logoBase64 = Convert.ToBase64String(company.Logo);
            }

            string currency = invoice.Currency ?? "USD"; 

            string FormatCurrency(decimal value)
            {
                return $"{value:N2} {currency}"; 
            }

            // Email-specific HTML content
            var subject = $"Invoice {invoice.InvoiceNumber} Details";
            var htmlMessage = $@"
                <html>
                <head>
                <style>
                .content {{ margin: 20px; }}
                .header {{ background: linear-gradient(to right, #4a67b3, #293b8b); color: white; padding: 10px; text-align: center; font-size: 20px; font-weight: bold; }}
                h3 {{ color: #333; }}
                p {{ font-size: 16px; color: #555; }}
                table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                table, th, td {{ border: 1px solid #ddd; }}
                th, td {{ padding: 8px; text-align: left; }}
                th {{ background-color: #f2f2f2; }}
                .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 0.9em; color: #555; }}
                .logo {{ width: 150px; height: auto; margin-bottom: 20px; }}
                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                </style>
                </head>
                <body>
                <div class='header'>
                <h2>Invoice Details</h2>
                {(string.IsNullOrEmpty(logoBase64) ? "" : $"<img class='logo' src='data:image/png;base64,{logoBase64}' alt='Company Logo'>")}
                </div>

                <div class='content'>
                <h3>Invoice Information</h3>
                <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                <p><strong>PONumber:</strong> {invoice.PONumber}</p>
                <p><strong>Date:</strong> {invoice.Date.ToShortDateString()}</p>
                <p><strong>Due Date:</strong> {invoice.DueDate.ToShortDateString()}</p>
                <p><strong>Subtotal:</strong> {FormatCurrency(invoice.SubTotal)}</p>
                <p><strong>Discount:</strong> {FormatCurrency(invoice.Discount)}</p>
                <p><strong>Tax:</strong> {FormatCurrency(invoice.TaxAmount)}</p>
                <p><strong>Shipping:</strong> {FormatCurrency(invoice.Shipping)}</p>
                <p><strong>Amount Paid:</strong> {FormatCurrency(invoice.AmountPaid)}</p>
                <p><strong>Total:</strong> {FormatCurrency(invoice.Total)}</p>
                <p><strong>Balance Due:</strong> {FormatCurrency(invoice.BalanceDue)}</p>
                <p><strong>Client:</strong> {client.Name} ({client.Email})</p>
                <p><strong>Company:</strong> {company.Name} - {company.Address}</p>
                <p><strong>Notes:</strong> {invoice.Notes}</p>
                <p><strong>Terms:</strong> {invoice.Terms}</p>
                <p><strong>Payment Terms:</strong> {invoice.PaymentTerms}</p>

                <h3>Invoice Items</h3>
                <table>
                <thead>
                    <tr>
                        <th>Description</th>
                        <th>Quantity</th>
                        <th>Rate</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", invoice.InvoiceItems.Select(item => $@"
                        <tr>
                            <td>{item.Description}</td>
                            <td>{item.Quantity}</td>
                            <td>{FormatCurrency(item.Rate)}</td>
                            <td>{FormatCurrency(item.Total)}</td>
                        </tr>
                    "))}
                </tbody>
                </table>
                </div>
    
                <div class='footer'>
                <p>Thank you for using our services. If you have any questions, please contact us at {company.Name}.</p>
                <p>&copy; {DateTime.Now.Year} {company.Name}. All rights reserved.</p>
                </div>
                </body>
                </html>
                ";

            // Send the email
            await _emailSender.SendEmailAsync(email, subject, htmlMessage);

            TempData["SuccessMessage"] = "Email sent successfully!";

            Log.Information("Sending email to {Email} for invoice ID {InvoiceId}", email, invoiceId);

            return RedirectToPage("/Invoices/InvoiceHistory");


            // Return a response (e.g., JSON) without including the logo in the web page
            //return new JsonResult(new { success = true });
        }
        public async Task<IActionResult> OnPostDownloadAsync(int id)
        {
            Log.Information("Download request received for invoice with ID: {InvoiceId}", id);

            if (Invoices == null || !Invoices.Any())
            {
                Log.Information("Invoices list is empty, loading invoices from service.");
                Invoices = await _invoiceService.GetInvoicesAsync();
            }

            var invoice = Invoices.FirstOrDefault(i => i.Id == id);
            if (invoice == null)
            {
                Log.Warning("Invoice with ID {InvoiceId} not found.", id);
                return NotFound();
            }

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

            var pdfBytes = _pdfService.CreatePdfFromHtml(htmlContent, $"Invoice_{invoice.InvoiceNumber}");
            Log.Information("PDF generated successfully for invoice {InvoiceId}.", id);
            return File(pdfBytes, "application/pdf", $"Invoice_{invoice.InvoiceNumber}.pdf");
        }
    }
}
