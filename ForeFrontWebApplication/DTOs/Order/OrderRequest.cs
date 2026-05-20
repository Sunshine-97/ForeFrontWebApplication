using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.DTOs.Order
{
    public class OrderRequest
    {
        [Required]
        public string KundId { get; set; } = string.Empty;

        [Required]
        [MinLength(1, ErrorMessage = "Minst en produkt krävs")]
        [MaxLength(50, ErrorMessage = "Max 50 produkter per order")]
        public List<OrderItemRequest> Produkter { get; set; } = [];
    }
}