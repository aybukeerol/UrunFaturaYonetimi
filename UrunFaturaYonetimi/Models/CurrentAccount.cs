namespace UrunFaturaYonetimi.Models
{
    public class CurrentAccount
    {
        public int Id { get; set; }
        public string AccountCode { get; set; }        // Cari Kodu
        public string AccountName { get; set; }        // Cari Adı / Unvanı
        public string AccountType { get; set; }        // Müşteri veya Tedarikçi
        public string IdentifierNumber { get; set; }   // T.C. / Vergi No
        public string Phone { get; set; }              // Telefon
        public string Email { get; set; }              // E-posta
        public string Neighborhood { get; set; }       // Mahalle
        public string District { get; set; }           // İlçe
        public string City { get; set; }               // Şehir
    }
}
