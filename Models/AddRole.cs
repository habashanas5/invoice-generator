using System.ComponentModel.DataAnnotations;

namespace Invoice_Generator.Models
{
    public class AddRole
    {
        [Required, StringLength(256)]
        public string Name { get; set; }
    }
}
