using Invoice_Generator.Models;
using Invoice_Generator.Pages.Invoices;

namespace Invoice_Generator.Applications
{
    public interface IInvoiceService
    {
        Task SaveInvoiceAsync(Invoice invoice);
        Task SaveClientAsync(Client client);
        Task SaveCompanyAsync(Company company);
        Task SaveInvoiceItemsAsync(List<InvoiceItem> invoiceItems);
        Task<Invoice> GetInvoiceByIdAsync(int id);
        Task<List<Invoice>> GetInvoicesAsync();
        Task<IEnumerable<InvoiceItem>> GetInvoiceItemsByInvoiceIdAsync(int invoiceId);
        Task SendInvoiceEmailAsync(string email, int invoiceId);

    }
}
