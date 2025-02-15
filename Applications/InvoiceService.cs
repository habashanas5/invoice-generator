using Invoice_Generator.Data;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Invoice_Generator.Applications
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public InvoiceService(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task SaveCompanyAsync(Company company)
        {
            _context.companies.Add(company);
            await _context.SaveChangesAsync();
        }

        public async Task SaveClientAsync(Client client)
        {
            _context.clients.Add(client);
            await _context.SaveChangesAsync();
        }

        public async Task SaveInvoiceAsync(Invoice invoice)
        {
            if (invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
            {
                foreach (var item in invoice.InvoiceItems)
                {
                    _context.invoiceItems.Add(item);
                }
            }

            _context.invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        public async Task SaveInvoiceItemsAsync(List<InvoiceItem> invoiceItems)
        {
            // Ensure the Total for each InvoiceItem is calculated before saving
            foreach (var item in invoiceItems)
            {
                item.CreatedAt = DateTime.UtcNow;
                item.UpdatedAt = DateTime.UtcNow;
            }

            // Add the InvoiceItems to the context
            _context.invoiceItems.AddRange(invoiceItems);

            // Save changes to the database
            await _context.SaveChangesAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.invoices
                .Include(i => i.Client)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<Invoice>> GetInvoicesAsync()
        {
            return await _context.invoices
                .Include(i => i.Client)
                .Include(i => i.Company)
                .Include(i => i.InvoiceItems)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<InvoiceItem>> GetInvoiceItemsByInvoiceIdAsync(int invoiceId)
        {
            return await _context.invoiceItems
                .Where(item => item.InvoiceId == invoiceId)
                .ToListAsync();
        }


        public async Task SendInvoiceEmailAsync(string email, int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found.");
            }

            var client = await _context.clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId);
            var company = await _context.companies.FirstOrDefaultAsync(c => c.Id == invoice.CompanyId);

            if (client == null || company == null)
            {
                throw new ArgumentException("Client or Company not found.");
            }

            string currency = invoice.Currency ?? "USD";
            decimal Discount = invoice.Discount == 0 ? 0.0M : invoice.Discount;
            decimal Tax = invoice.TaxAmount == 0 ? 0.0M : invoice.TaxAmount;
            decimal Shipping = invoice.Shipping == 0 ? 0.0M : invoice.Shipping;
            decimal AmountPaid = invoice.AmountPaid == 0 ? 0.0M : invoice.AmountPaid;


            string FormatCurrency(decimal value)
            {
                return $"{value:N2} {currency}";
            }

            var emailSubject = $"Invoice {invoice.InvoiceNumber} Details";

            // Convert the logo to Base64 if it exists
            var logoBase64 = company.Logo != null ? Convert.ToBase64String(company.Logo) : "";
            var logoHtml = !string.IsNullOrEmpty(logoBase64)
                ? $"<img src='data:image/png;base64,{logoBase64}' alt='Company Logo' style='max-width: 150px; display: block; margin: 0 auto;'>"
                : "<p>No logo available</p>";

            // Build the email body with HTML structure
            var emailBody = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                .header {{ background: linear-gradient(to right, #4a67b3, #293b8b); color: white; padding: 10px; text-align: center; font-size: 20px; font-weight: bold; }}
                    .content {{ padding: 20px; }}
                    .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 0.9em; color: #555; }}
                    table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                    table, th, td {{ border: 1px solid #ddd; }}
                    th, td {{ padding: 8px; text-align: left; }}
                    th {{ background-color: #f2f2f2; }}
                </style>
            </head>
            <body>
                <div class='header'>
                    <h2>Invoice Details</h2>
                    {logoHtml}
                </div>
                <div class='content'>
                    <h3>Invoice Information</h3>
                    <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
                    <p><strong>PONumber:</strong> {invoice.PONumber}</p>
                    <p><strong>Date:</strong> {invoice.Date.ToShortDateString()}</p>
                    <p><strong>Due Date:</strong> {invoice.DueDate.ToShortDateString()}</p>
                    <p><strong>Subtotal:</strong> {FormatCurrency(invoice.SubTotal)}</p>
                    <p><strong>Discount:</strong> {FormatCurrency(Discount)}</p>
                    <p><strong>Tax:</strong> {FormatCurrency(Tax)}</p>
                    <p><strong>Shipping:</strong> {FormatCurrency(Shipping)}</p>
                    <p><strong>Amount Paid:</strong> {FormatCurrency(AmountPaid)}</p>
                    <p><strong>Total:</strong> {FormatCurrency(invoice.Total)}</p>
                    <p><strong>Balance Due:</strong> {FormatCurrency(invoice.BalanceDue)}</p>
                    <p><strong>Client:</strong> {client.Name} ({client.Email})</p>
                    <p><strong>Company:</strong> {company.Name} - {company.Address}</p>
                    <p><strong>Payment Terms:</strong> {invoice.PaymentTerms}</p>
                    <p><strong>Notes:</strong> {invoice.Notes}</p>
                    <p><strong>Terms:</strong> {invoice.Terms}</p>

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
            await _emailSender.SendEmailAsync(email, emailSubject, emailBody);
            Log.Information("Sending email to {Email} for invoice ID {InvoiceId}", email, invoiceId);
        }

        /*public async Task SendInvoiceEmailAsync(string email, int invoiceId)
        {
            var invoice = await GetInvoiceByIdAsync(invoiceId);
            if (invoice == null)
            {
                throw new ArgumentException("Invoice not found.");
            }

            var client = await _context.clients.FirstOrDefaultAsync(c => c.Id == invoice.ClientId);
            var company = await _context.companies.FirstOrDefaultAsync(c => c.Id == invoice.CompanyId);

            if (client == null || company == null)
            {
                throw new ArgumentException("Client or Company not found.");
            }

            var emailSubject = $"Invoice Details - {invoice.InvoiceNumber}";

            // Convert the logo to Base64 if it exists
            var logoBase64 = company.Logo != null ? Convert.ToBase64String(company.Logo) : "";
            Console.WriteLine($"Logo Base64: {logoBase64}");

            var logoHtml = !string.IsNullOrEmpty(logoBase64)
                ? $"<img src='data:image/png;base64,{logoBase64}' alt='Company Logo' style='max-width: 150px; display: block; margin: 0 auto;'>"
                : "<p>No logo available</p>";

            // Build the email body with HTML structure
            var emailBody = $@"
    <html>
    <head>
        <style>
            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
            .header {{ background-color: #4CAF50; color: white; padding: 10px; text-align: center; }}
            .content {{ padding: 20px; }}
            .footer {{ background-color: #f1f1f1; padding: 10px; text-align: center; font-size: 0.9em; color: #555; }}
            table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
            table, th, td {{ border: 1px solid #ddd; }}
            th, td {{ padding: 8px; text-align: left; }}
            th {{ background-color: #f2f2f2; }}
        </style>
    </head>
    <body>
        <div class='header'>
            <h2>Invoice Details</h2>
            {logoHtml}
        </div>
        <div class='content'>
            <h3>Invoice Information</h3>
            <p><strong>Invoice Number:</strong> {invoice.InvoiceNumber}</p>
            <p><strong>Date:</strong> {invoice.Date.ToShortDateString()}</p>
            <p><strong>Total:</strong> {invoice.Total.ToString("C")}</p>
            <p><strong>Client:</strong> {client.Name} ({client.Email})</p>
            <p><strong>Company:</strong> {company.Name} - {company.Address}</p>
            <p><strong>Notes:</strong> {invoice.Notes}</p>
            <p><strong>Terms:</strong> {invoice.Terms}</p>

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
                            <td>{item.Rate.ToString("C")}</td>
                            <td>{item.Total.ToString("C")}</td>
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

            // Save the email body to a file for testing
            File.WriteAllText("test_email.html", emailBody);

            // Send the email
            await _emailSender.SendEmailAsync(email, emailSubject, emailBody);
        }*/
    }
}
