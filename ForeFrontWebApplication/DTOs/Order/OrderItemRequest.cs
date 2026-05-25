using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.DTOs.Order
{
    public class OrderItemRequest
    {
        [Required]
        [StringLength(100)]
        public required string ProduktId { get; set; }

        [Range(1, 10_000, ErrorMessage = "Antal måste vara mellan 1 och 10 000")]
        public required int Antal { get; set; }
    }
}
