namespace UrunFaturaYonetimi.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal UnitPrice { get; set; }
        public int StockQuantity { get; set; }
        public byte[] Image { get; set; }
    }
}
