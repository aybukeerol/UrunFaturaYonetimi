using System;

namespace UrunFaturaYonetimi.Models
{
    public class Invoice
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; }
        public DateTime Date { get; set; }
        public int CurrentAccountId { get; set; }
    }
}
