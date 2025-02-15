using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice_Generator.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public string? InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public string? PaymentTerms { get; set; }
        public DateTime DueDate { get; set; }
        public string? PONumber { get; set; }
        public string? Notes { get; set; }
        public string? Terms { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Shipping { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public string? Currency { get; set; }
        public bool IsPaid { get; set; } = false;

        // Relationships
        public string ApplicationUserId { get; set; }
        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        public int ClientId { get; set; }
        [ForeignKey("ClientId")]
        public Client Client { get; set; }

        [Required]
        public int CompanyId { get; set; }
        [ForeignKey("CompanyId")]
        public Company Company { get; set; }

        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    }
}
