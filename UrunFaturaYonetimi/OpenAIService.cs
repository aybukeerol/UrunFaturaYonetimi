using System;
using OpenAI.Chat;

namespace UrunFaturaYonetimi
{
    internal class OpenAIService
    {
        // =========================================================
        // API KEY KONTROLÜ
        // =========================================================

        public static bool ApiKeyVarMi()
        {
            string apiKey =
                Environment.GetEnvironmentVariable("OPENAI_API_KEY");

            return !string.IsNullOrWhiteSpace(apiKey);
        }


        // =========================================================
        // YÖNETİCİ RAPORU OLUŞTUR
        // =========================================================

        public static string YoneticiRaporuOlustur(string raporVerisi)
        {
            try
            {
                string apiKey =
                    Environment.GetEnvironmentVariable("OPENAI_API_KEY");

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    return
                        "OPENAI_API_KEY bulunamadı.\n\n" +
                        "OpenAI API anahtarını ortam değişkenlerine " +
                        "ekledikten sonra tekrar deneyin.";
                }


                // -------------------------------------------------
                // OpenAI istemcisi
                // -------------------------------------------------

                ChatClient client =
                    new ChatClient(
                        model: "gpt-5.1",
                        apiKey: apiKey);


                // -------------------------------------------------
                // AI'YA GÖNDERİLECEK İSTEK
                // -------------------------------------------------

                string prompt =
                    @"Sen NEXORA İşletme Yönetim Platformu içinde çalışan
bir yönetici raporlama asistanısın.

Görevin sana verilen işletme verilerini analiz ederek
Türkçe, kısa, anlaşılır ve profesyonel bir yönetici raporu
hazırlamaktır.

Raporu aşağıdaki yapıda oluştur:

1. YÖNETİCİ ÖZETİ
Dönemin genel durumunu birkaç cümleyle açıkla.

2. FATURA ANALİZİ
Fatura sayısı, toplam tutar, ortalama fatura ve
yüksek tutarlı işlemler hakkında yorum yap.

3. CARİ / MÜŞTERİ ANALİZİ
Mevcut verilerden çıkarılabilecek müşteri ve cari
hareketlerini değerlendir.

4. ÜRÜN / HİZMET ANALİZİ
Mevcut ürün verilerini değerlendir.

5. DİKKAT EDİLMESİ GEREKENLER
Verilerde dikkat çeken durumları maddeler halinde belirt.

6. YÖNETİM İÇİN NOTLAR
Yöneticinin incelemesinin faydalı olabileceği noktaları
belirt.

ÖNEMLİ KURALLAR:

- Sana verilmeyen bir rakamı uydurma.
- Olmayan satış veya müşteri bilgisi üretme.
- Yalnızca verilen verileri değerlendir.
- Veri yetersizse bunu açıkça belirt.
- Para değerlerini Türk Lirası biçiminde yorumla.
- Gereksiz uzun açıklamalardan kaçın.
- Rapor profesyonel bir yönetim raporu gibi görünmeli.

NEXORA'DAN GELEN RAPOR VERİLERİ:

" + raporVerisi;


                // -------------------------------------------------
                // OPENAI ÇAĞRISI
                // -------------------------------------------------

                ChatCompletion completion =
                    client.CompleteChat(prompt);


                // -------------------------------------------------
                // CEVAP KONTROLÜ
                // -------------------------------------------------

                if (completion == null ||
                    completion.Content == null ||
                    completion.Content.Count == 0)
                {
                    return
                        "OpenAI rapor oluşturdu ancak " +
                        "geçerli bir metin döndürmedi.";
                }


                string cevap =
                    completion.Content[0].Text;


                if (string.IsNullOrWhiteSpace(cevap))
                {
                    return
                        "OpenAI'dan boş bir rapor döndü.";
                }


                return cevap;
            }
            catch (Exception ex)
            {
                return
                    "NEXORA AI raporu oluşturulurken bir hata oluştu.\n\n" +
                    "Hata: " +
                    ex.Message;
            }
        }
    }
}