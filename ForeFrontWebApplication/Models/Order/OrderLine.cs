namespace ForeFrontWebApplication.Models.Order
{
    public class OrderLine
    {
        public required string OrderLineId { get; set; }
        public required string OrderId     { get; set; }
        public required string ProduktId   { get; set; }
        public required string Namn        { get; set; }
        public int     Antal               { get; set; }
        public decimal Pris                { get; set; }
    }
}
