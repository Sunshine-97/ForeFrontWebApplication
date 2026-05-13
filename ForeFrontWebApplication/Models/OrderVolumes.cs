namespace ForeFrontWebApplication.Models;

public class OrderVolumes
{
    public required string ProduktId { get; set; }
    public required string Namn { get; set; }
    public int Antal { get; set; }
    public decimal Pris { get; set; }
}
