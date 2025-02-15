using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Invoice_Generator.Models
{
    public class ApplicationUser : IdentityUser 
    {

        public ApplicationUser()
        {
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
