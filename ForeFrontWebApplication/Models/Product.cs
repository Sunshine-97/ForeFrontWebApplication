using System.ComponentModel.DataAnnotations;

namespace ForeFrontWebApplication.Models;

public class Product
{
    [Required]
    public required string ProduktId { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public required string Namn { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Antal måste vara minst 1")]
    public int Antal { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Pris måste vara större än 0")]
    public decimal Pris { get; set; }
}
