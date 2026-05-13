using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models;

public class CreateOrderRequest
{
    [Required]
    public required Customer Kund { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Minst en produkt krävs")]
    [MaxLength(50, ErrorMessage = "Max 50 produkter per order")]
    public required List<Product> Produkter { get; set; }
}
