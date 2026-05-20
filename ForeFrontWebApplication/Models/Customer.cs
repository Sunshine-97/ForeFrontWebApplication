using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models.Customer
{
    public class Customers
    {
        public required string CustomerId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public required string Namn { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public required string Email { get; set; }
    }
}
