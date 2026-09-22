using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace UrunFaturaYonetimi.DataAccess
{
    public class SqlFaturaKalemi
    {
        public string UrunAdi { get; set; }
        public int Miktar { get; set; }
        public decimal BirimFiyat { get; set; }
        public decimal SatirToplam { get; set; }
    }

    public static class InvoiceSqlRepository
    {
        public static int Kaydet(
            string faturaNo,
            DateTime tarih,
            string alici,
            decimal toplam,
            IList<SqlFaturaKalemi> kalemler)
        {
            if (string.IsNullOrWhiteSpace(faturaNo))
                throw new Exception("Fatura numarası boş olamaz.");

            if (string.IsNullOrWhiteSpace(alici))
                throw new Exception("Alıcı adı boş olamaz.");

            if (kalemler == null || kalemler.Count == 0)
                throw new Exception("Faturada en az bir ürün olmalıdır.");

            using (var conn = new SqlConnection(
                DatabaseInitializer.ConnectionString))
            {
                conn.Open();

                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Aynı fatura numarasının tekrar kaydedilmesini önle.
                        using (var kontrol = new SqlCommand(
                            @"SELECT COUNT(*)
                              FROM dbo.Invoices
                              WHERE InvoiceNumber = @no",
                            conn, transaction))
                        {
                            kontrol.Parameters.Add(
                                "@no", SqlDbType.NVarChar, 100).Value = faturaNo;

                            int adet = Convert.ToInt32(
                                kontrol.ExecuteScalar());

                            if (adet > 0)
                                throw new Exception(
                                    "Bu fatura numarası zaten kayıtlı: " + faturaNo);
                        }

                        // Önce ürünleri kontrol et.
                        var urunIdleri = new List<int>();

                        foreach (SqlFaturaKalemi kalem in kalemler)
                        {
                            if (string.IsNullOrWhiteSpace(kalem.UrunAdi))
                                throw new Exception(
                                    "Faturadaki ürün adı boş olamaz.");

                            if (kalem.Miktar <= 0)
                                throw new Exception(
                                    "Ürün miktarı sıfırdan büyük olmalıdır.");

                            if (kalem.BirimFiyat < 0 || kalem.SatirToplam < 0)
                                throw new Exception(
                                    "Ürün fiyatı veya toplamı negatif olamaz.");

                            using (var urunKomutu = new SqlCommand(
                                @"SELECT Id
                                  FROM dbo.Products
                                  WHERE Name = @name",
                                conn, transaction))
                            {
                                urunKomutu.Parameters.Add(
                                    "@name", SqlDbType.NVarChar, 200
                                ).Value = kalem.UrunAdi;

                                int urunId = 0;

                                using (var reader = urunKomutu.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        urunId = reader.GetInt32(0);

                                        if (reader.Read())
                                            throw new Exception(
                                                "SQL'de aynı adla birden fazla ürün var: "
                                                + kalem.UrunAdi);
                                    }
                                }

                                if (urunId == 0)
                                    throw new Exception(
                                        "Ürün SQL'de bulunamadı: "
                                        + kalem.UrunAdi
                                        + "\nÖnce ürün kartını oluşturmalısınız.");

                                urunIdleri.Add(urunId);
                            }
                        }

                        // Fatura başlığını kaydet.
                        int faturaId;

                        using (var komut = new SqlCommand(
                            @"INSERT INTO dbo.Invoices
                              (
                                  InvoiceNumber,
                                  InvoiceDate,
                                  CustomerTitle,
                                  TotalAmount
                              )
                              OUTPUT INSERTED.Id
                              VALUES
                              (
                                  @no,
                                  @date,
                                  @customer,
                                  @total
                              )",
                            conn, transaction))
                        {
                            komut.Parameters.Add(
                                "@no", SqlDbType.NVarChar, 100
                            ).Value = faturaNo;

                            komut.Parameters.Add(
                                "@date", SqlDbType.DateTime2
                            ).Value = tarih;

                            komut.Parameters.Add(
                                "@customer", SqlDbType.NVarChar, -1
                            ).Value = alici;

                            var toplamParametre = komut.Parameters.Add(
                                "@total", SqlDbType.Decimal);

                            toplamParametre.Precision = 18;
                            toplamParametre.Scale = 2;
                            toplamParametre.Value = toplam;

                            faturaId = Convert.ToInt32(
                                komut.ExecuteScalar());
                        }

                        // Faturanın ürün kalemlerini kaydet.
                        for (int i = 0; i < kalemler.Count; i++)
                        {
                            SqlFaturaKalemi kalem = kalemler[i];

                            using (var komut = new SqlCommand(
                                @"INSERT INTO dbo.InvoiceDetails
                                  (
                                      InvoiceId,
                                      ProductId,
                                      Quantity,
                                      UnitPrice,
                                      LineTotal
                                  )
                                  VALUES
                                  (
                                      @invoice,
                                      @product,
                                      @quantity,
                                      @price,
                                      @line
                                  )",
                                conn, transaction))
                            {
                                komut.Parameters.Add(
                                    "@invoice", SqlDbType.Int
                                ).Value = faturaId;

                                komut.Parameters.Add(
                                    "@product", SqlDbType.Int
                                ).Value = urunIdleri[i];

                                komut.Parameters.Add(
                                    "@quantity", SqlDbType.Int
                                ).Value = kalem.Miktar;

                                var fiyatParametre = komut.Parameters.Add(
                                    "@price", SqlDbType.Decimal);

                                fiyatParametre.Precision = 18;
                                fiyatParametre.Scale = 2;
                                fiyatParametre.Value = kalem.BirimFiyat;

                                var satirParametre = komut.Parameters.Add(
                                    "@line", SqlDbType.Decimal);

                                satirParametre.Precision = 18;
                                satirParametre.Scale = 2;
                                satirParametre.Value = kalem.SatirToplam;

                                komut.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();

                        return faturaId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}