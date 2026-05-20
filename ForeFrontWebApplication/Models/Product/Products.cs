using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models.Product
{
    public class Products
    {
        public required string ProductId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 1)]
        public required string Namn { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Pris måste vara större än 0")]
        public decimal Pris { get; set; }
    }
}
