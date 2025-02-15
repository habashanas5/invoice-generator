using System.ComponentModel.DataAnnotations;

namespace Invoice_Generator.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        
        [EmailAddress]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
