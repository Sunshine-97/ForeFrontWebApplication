using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models.Order
{
    public class UpdateStatusRequest
    {
        [Required]
        public required OrderStatus Status { get; init; }
    }
}