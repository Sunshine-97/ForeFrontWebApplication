using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.DTOs.Order
{
    public class OrderItemRequest
    {
        public required string ProduktId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Antal måste vara minst 1")]
        public required int Antal { get; set; }
    }
}
