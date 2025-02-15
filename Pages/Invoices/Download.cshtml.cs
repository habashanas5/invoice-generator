using Humanizer;
using Invoice_Generator.Applications;
using Invoice_Generator.Infrastructures.Pdfs;
using Invoice_Generator.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing.Printing;
using System.Drawing;

namespace Invoice_Generator.Pages.Invoices
{
    public class DownloadModel : PageModel
    {
        private readonly IPdfService _pdfService;
        private readonly IInvoiceService _invoiceService;

        public DownloadModel(IPdfService pdfService, IInvoiceService invoiceService)
        {
            _pdfService = pdfService;
            _invoiceService = invoiceService;
        }

        public Invoice Invoice { get; set; }

        public async Task OnGetAsync(int id)
        {
            // Fetch the invoice data from the database
            // Assuming you have a method to get the invoice by ID
            Invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        }

        public async Task<IActionResult> OnPostDownloadAsync(int id)
        {
            // Fetch the invoice data from the database
            Invoice = await _invoiceService.GetInvoiceByIdAsync(id);

            // Generate HTML content for the invoice
            string htmlContent = GenerateInvoiceHtml(Invoice);

            // Generate the PDF
            byte[] pdfBytes = _pdfService.CreatePdfFromHtml(htmlContent, $"Invoice_{Invoice.InvoiceNumber}");

            // Return the PDF as a file download
            return File(pdfBytes, "application/pdf", $"Invoice_{Invoice.InvoiceNumber}.pdf");
        }

        private string GenerateInvoiceHtml(Invoice invoice)
        {
            string html = $@"
        <html>
            <head>
                <title>Invoice {invoice.InvoiceNumber}</title>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        font-size: 12px;
                        margin: 0;
                        padding: 0;
                    }}

                    .invoice-container {{
                        width: 100%;
                        max-width: 800px;
                        height: 100%
                        margin: 0 auto;
                        padding: 20px;
                        border: 1px solid #ddd;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}

                    .header {{
                        display: flex;
                        justify-content: space-between;
                        align-items: flex-start; /* Align items to the top */
                        margin-bottom: 20px;
                    }}

                    .header img {{
                        max-width: 100%;
                        height: 100%;
                    }}

                    .header .invoice-info {{
                        text-align: right; /* Align text to the right */
                    }}

                    .header h1 {{
                        font-size: 24px;
                        color: #333;
                        margin: 0 0 10px 0;
                    }}

                    .header p {{
                        font-size: 14px;
                        color: #777;
                        margin: 5px 0;
                    }}

                    .company-info, .client-info {{
                        margin-bottom: 20px;
                    }}

                    .company-info h2, .client-info h2 {{
                        font-size: 18px;
                        color: #333;
                        margin-bottom: 10px;
                    }}

                    .info-table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }}

                    .info-table th, .info-table td {{
                        border: 1px solid #ddd;
                        padding: 8px;
                        text-align: left;
                    }}

                    .info-table th {{
                        background-color: #f9f9f9;
                        font-weight: bold;
                    }}

                    .invoice-details {{
                        margin-bottom: 20px;
                    }}

                    .invoice-details table {{
                        width: 100%;
                        border-collapse: collapse;
                    }}

                    .invoice-details th, .invoice-details td {{
                        border: 1px solid #ddd;
                        padding: 8px;
                        text-align: left;
                    }}

                    .invoice-details th {{
                        background-color: #f9f9f9;
                        font-weight: bold;
                    }}

                    .items-table {{
                        width: 100%;
                        border-collapse: collapse;
                        margin-bottom: 20px;
                    }}

                    .items-table th, .items-table td {{
                        border: 1px solid #ddd;
                        padding: 8px;
                        text-align: left;
                    }}

                    .items-table th {{
                        background-color: #f9f9f9;
                        font-weight: bold;
                    }}

                    .total-section {{
                        text-align: right;
                        margin-top: 20px;
                    }}

                    .total-section p {{
                        margin: 5px 0;
                    }}

                    .footer {{
                        margin-top: 30px;
                        padding-top: 10px;
                        border-top: 1px solid #ddd;
                        text-align: center;
                        font-size: 10px;
                        color: #777;
                    }}
                </style>
            </head>
            <body>
                <div class='invoice-container'>
                    <!-- Header -->
                    <div class='header'>
                        <!-- Logo on the left -->
                        <div>
                            {(invoice.Company.Logo != null ? $"<img src='data:image/png;base64,{Convert.ToBase64String(invoice.Company.Logo)}' alt='Company Logo' />" : "")}
                        </div>

                        <!-- Invoice info on the right -->
                        <div class='invoice-info'>
                            <h1>Invoice</h1>
                            <p>Invoice Number: {invoice.InvoiceNumber}</p>
                            <p>Date: {invoice.Date.ToShortDateString()}</p>
                            <p>Due Date: {invoice.DueDate.ToShortDateString()}</p>
                        </div>
                    </div>

                    <!-- Company Information -->
                    <div class='company-info'>
                        <h2>From:</h2>
                        <table class='info-table'>
                            <tr>
                                <th>Name</th>
                                <td>{invoice.Company.Name}</td>
                            </tr>
                            <tr>
                                <th>Address</th>
                                <td>{invoice.Company.Address}</td>
                            </tr>
                        </table>
                    </div>

                    <!-- Client Information -->
                    <div class='client-info'>
                        <h2>To:</h2>
                        <table class='info-table'>
                            <tr>
                                <th>Name</th>
                                <td>{invoice.Client.Name}</td>
                            </tr>
                            <tr>
                                <th>Email</th>
                                <td>{invoice.Client.Email}</td>
                            </tr>
                        </table>
                    </div>

                    <!-- Invoice Details -->
                    <div class='invoice-details'>
                        <table>
                            <tr>
                                <th>Payment Terms</th>
                                <td>{invoice.PaymentTerms}</td>
                            </tr>
                            <tr>
                                <th>PO Number</th>
                                <td>{invoice.PONumber}</td>
                            </tr>
                            <tr>
                                <th>Currency</th>
                                <td>{invoice.Currency}</td>
                            </tr>
                        </table>
                    </div>

                    <!-- Invoice Items -->
                    <table class='items-table'>
                        <thead>
                            <tr>
                                <th>Description</th>
                                <th>Quantity</th>
                                <th>Rate</th>
                                <th>Total</th>
                            </tr>
                        </thead>
                        <tbody>";

            foreach (var item in invoice.InvoiceItems)
            {
                html += $@"
            <tr>
                <td>{item.Description}</td>
                <td>{item.Quantity}</td>
                <td>{item.Rate}</td>
                <td>{item.Total}</td>
            </tr>";
            }

            html += $@"
                        </tbody>
                    </table>

                    <!-- Totals -->
                    <div class='total-section'>
                        <p>SubTotal: {invoice.SubTotal}</p>
                        <p>Discount: {invoice.Discount}</p>
                        <p>Shipping: {invoice.Shipping}</p>
                        <p>Tax: {invoice.TaxAmount}</p>
                        <p>Total: {invoice.Total}</p>
                        <p>Amount Paid: {invoice.AmountPaid}</p>
                        <p>Balance Due: {invoice.BalanceDue}</p>
                    </div>

                    <!-- Footer -->
                    <div class='footer'>
                        <p>Thank you for your business!</p>
                        <p>Terms: {invoice.Terms}</p>
                        <p>Notes: {invoice.Notes}</p>
                    </div>
                </div>
            </body>
        </html>";

            return html;
        }
    }
}