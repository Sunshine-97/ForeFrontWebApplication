namespace ForeFrontWebApplication.Models.Warehouse
{
    public sealed class OrderVolumes
    {
        public required string ProduktId { get; init; }
        public required string Namn { get; init; }
        public int Antal { get; init; }
    }
}