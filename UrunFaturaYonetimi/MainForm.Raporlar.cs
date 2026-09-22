using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UrunFaturaYonetimi
{
    public partial class MainForm
    {
        private string AiSorgusunuCalistir(string kullaniciMesaji)
        {
            string q = TurkceKucult(kullaniciMesaji);


            DateTime simdi = DateTime.Now;
            DateTime buAyBaslangic = new DateTime(simdi.Year, simdi.Month, 1);
            DateTime sonrakiAyBaslangic = buAyBaslangic.AddMonths(1);
            DateTime gecenAyBaslangic = buAyBaslangic.AddMonths(-1);
            // Haftanın ilk günü pazartesi; bitiş sınırı hariçtir.
            DateTime buHaftaBaslangic = simdi.Date.AddDays(-((7 + (int)simdi.DayOfWeek - (int)DayOfWeek.Monday) % 7));
            DateTime sonrakiHaftaBaslangic = buHaftaBaslangic.AddDays(7);
            DateTime gecenHaftaBaslangic = buHaftaBaslangic.AddDays(-7);

            bool buAy = q.Contains("bu ay") || q.Contains("bu ayki") ||
                        q.Contains("bu ayın") || q.Contains("bu ayda");
            bool gecenAy = q.Contains("geçen ay") || q.Contains("gecen ay");
            bool bugun = q.Contains("bugün") || q.Contains("bugun");
            bool buHafta = q.Contains("bu hafta") || q.Contains("bu haftaki") ||
                           q.Contains("bu haftanın") || q.Contains("bu haftayı");
            bool gecenHafta = q.Contains("geçen hafta") || q.Contains("gecen hafta") ||
                              q.Contains("geçtiğimiz hafta") || q.Contains("gectigimiz hafta");

            string cariAdi = AiCariAdiniBul(kullaniciMesaji);

            DateTime? baslangic = null;
            DateTime? bitis = null;

            if (buAy)
            {
                baslangic = buAyBaslangic;
                bitis = sonrakiAyBaslangic;
            }
            else if (gecenAy)
            {
                baslangic = gecenAyBaslangic;
                bitis = buAyBaslangic;
            }
            else if (bugun)
            {
                baslangic = simdi.Date;
                bitis = simdi.Date.AddDays(1);
            }
            else if (buHafta)
            {
                baslangic = buHaftaBaslangic;
                bitis = sonrakiHaftaBaslangic;
            }
            else if (gecenHafta)
            {
                baslangic = gecenHaftaBaslangic;
                bitis = buHaftaBaslangic;
            }

            var temel = AppData.Faturalar.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(cariAdi))
            {
                temel = temel.Where(f =>
                    f.CariAdi.IndexOf(
                        cariAdi,
                        StringComparison.CurrentCultureIgnoreCase) >= 0);
            }

            if (baslangic.HasValue)
                temel = temel.Where(f => f.Tarih >= baslangic.Value);

            if (bitis.HasValue)
                temel = temel.Where(f => f.Tarih < bitis.Value);

            decimal tutarEsigi = AiTutarEsiginiBul(kullaniciMesaji);
            bool uzeri =
                q.Contains("üzeri") || q.Contains("üstü") ||
                q.Contains("fazla") || q.Contains("büyük");

            if (tutarEsigi > 0 && uzeri)
                temel = temel.Where(f => f.GenelToplam > tutarEsigi);

            var sonuc = temel
                .OrderByDescending(f => f.Tarih)
                .ToList();

            // -----------------------------------------------------
            // GÜNLÜK / AYLIK / GENEL YÖNETİCİ ÖZETİ
            // -----------------------------------------------------
            if (q.Contains("işler nasıl") ||
                q.Contains("isler nasil") ||
                q.Contains("işler nasıldı") ||
                q.Contains("isler nasildi") ||
                q.Contains("durum nasıl") ||
                q.Contains("durum nasil") ||
                q.Contains("durum nasıldı") ||
                q.Contains("durum nasildi") ||
                q.Contains("özetle") ||
                q.Contains("özet"))
            {
                // Dönem belirtilmemişse eski davranışı koru: bu ayın özeti.
                var donemFaturalari = (buAy || gecenAy || bugun || buHafta || gecenHafta)
                    ? sonuc
                    : AppData.Faturalar
                        .Where(f => f.Tarih >= buAyBaslangic &&
                                    f.Tarih < sonrakiAyBaslangic)
                        .ToList();

                string donemAdi = bugun ? "Bugünün" :
                                  gecenHafta ? "Geçen haftanın" :
                                  buHafta ? "Bu haftanın" :
                                  gecenAy ? "Geçen ayın" : "Bu ayın";

                decimal donemToplam = donemFaturalari.Sum(f => f.GenelToplam);

                string degisimSatiri = "";
                if (!bugun && !gecenAy && !buHafta && !gecenHafta)
                {
                    decimal gecenAyToplam = AppData.Faturalar
                        .Where(f => f.Tarih >= gecenAyBaslangic &&
                                    f.Tarih < buAyBaslangic)
                        .Sum(f => f.GenelToplam);

                    string degisim;
                    if (gecenAyToplam > 0)
                    {
                        decimal oran =
                            ((donemToplam - gecenAyToplam) / gecenAyToplam) * 100M;
                        degisim = oran >= 0
                            ? "%" + oran.ToString("N1") + " artış"
                            : "%" + Math.Abs(oran).ToString("N1") + " düşüş";
                    }
                    else
                    {
                        degisim = "geçen ay karşılaştırma verisi yok";
                    }

                    degisimSatiri = "• Geçen aya göre: " + degisim + "\r\n";
                }

                string enIyiCari = "-";
                decimal enIyiCariToplam = 0;

                var grup = donemFaturalari
                    .GroupBy(f => f.CariAdi)
                    .Select(g => new
                    {
                        Cari = g.Key,
                        Toplam = g.Sum(x => x.GenelToplam)
                    })
                    .OrderByDescending(x => x.Toplam)
                    .FirstOrDefault();

                if (grup != null)
                {
                    enIyiCari = grup.Cari;
                    enIyiCariToplam = grup.Toplam;
                }

                return
                    donemAdi + " kısa özeti:\r\n\r\n" +
                    "• Fatura sayısı: " + donemFaturalari.Count + "\r\n" +
                    "• Toplam fatura tutarı: " +
                    donemToplam.ToString("N2") + " TL\r\n" +
                    degisimSatiri +
                    "• En yüksek hacimli cari: " + enIyiCari +
                    (grup == null ? "" :
                        " (" + enIyiCariToplam.ToString("N2") + " TL)") +
                    "\r\n" +
                    "• Toplam cari: " + AppData.Cariler.Count + "\r\n" +
                    "• Toplam ürün/hizmet: " + AppData.Urunler.Count;
            }

            // -----------------------------------------------------
            // BU AY - GEÇEN AY KARŞILAŞTIR
            // -----------------------------------------------------
            if ((q.Contains("karşılaştır") || q.Contains("karsilastir")) &&
                q.Contains("ay"))
            {
                var b = AppData.Faturalar
                    .Where(f => f.Tarih >= buAyBaslangic &&
                                f.Tarih < sonrakiAyBaslangic)
                    .ToList();

                var g = AppData.Faturalar
                    .Where(f => f.Tarih >= gecenAyBaslangic &&
                                f.Tarih < buAyBaslangic)
                    .ToList();

                decimal bt = b.Sum(f => f.GenelToplam);
                decimal gt = g.Sum(f => f.GenelToplam);

                string yorum;
                if (gt == 0)
                    yorum = "Geçen ay karşılaştırılabilir tutar bulunmuyor.";
                else
                {
                    decimal oran = ((bt - gt) / gt) * 100M;
                    yorum = oran >= 0
                        ? "Tutar %" + oran.ToString("N1") + " arttı."
                        : "Tutar %" + Math.Abs(oran).ToString("N1") + " azaldı.";
                }

                return
                    "Aylık karşılaştırma:\r\n\r\n" +
                    "Bu ay: " + b.Count + " fatura • " +
                    bt.ToString("N2") + " TL\r\n" +
                    "Geçen ay: " + g.Count + " fatura • " +
                    gt.ToString("N2") + " TL\r\n\r\n" +
                    yorum;
            }

            // -----------------------------------------------------
            // EN ÇOK HANGİ MÜŞTERİ/CARİ?
            // -----------------------------------------------------
            if ((q.Contains("en çok") || q.Contains("en cok")) &&
                (q.Contains("müşteri") || q.Contains("musteri") ||
                 q.Contains("cari") || q.Contains("kime")))
            {
                var grup = sonuc
                    .GroupBy(f => f.CariAdi)
                    .Select(g => new
                    {
                        Cari = g.Key,
                        Toplam = g.Sum(x => x.GenelToplam),
                        Adet = g.Count()
                    })
                    .OrderByDescending(x => x.Toplam)
                    .FirstOrDefault();

                if (grup == null)
                    return "Bu dönem için fatura kaydı bulamadım.";

                return
                    "En yüksek fatura hacmine sahip cari:\r\n\r\n" +
                    grup.Cari + "\r\n" +
                    grup.Adet + " fatura\r\n" +
                    grup.Toplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // SON N FATURA
            // -----------------------------------------------------
            int sonAdet = AiSonAdediBul(q);
            if (sonAdet > 0 && q.Contains("fatura"))
            {
                var sonlar = sonuc.Take(sonAdet).ToList();

                if (sonlar.Count == 0)
                    return "Bu sorguya uygun fatura bulamadım.";

                string metin =
                    (string.IsNullOrWhiteSpace(cariAdi) ? "" : cariAdi + " • ") +
                    "Son " + sonlar.Count + " fatura:\r\n\r\n";

                foreach (FaturaKaydi f in sonlar)
                {
                    metin +=
                        "• " + f.FaturaNo + " | " +
                        f.Tarih.ToString("dd.MM.yyyy") + " | " +
                        f.GenelToplam.ToString("N2") + " TL\r\n";
                }

                FaturalarAc(cariAdi, baslangic, bitis);
                pnlAiAssistant.BringToFront();
                return metin.TrimEnd();
            }

            // -----------------------------------------------------
            // EN YÜKSEK FATURA
            // -----------------------------------------------------
            if ((q.Contains("en yüksek") || q.Contains("en yuksek") ||
                 q.Contains("en büyük") || q.Contains("en buyuk")) &&
                q.Contains("fatura"))
            {
                FaturaKaydi enYuksek = sonuc
                    .OrderByDescending(f => f.GenelToplam)
                    .FirstOrDefault();

                if (enYuksek == null)
                    return "Gösterilecek fatura bulunamadı.";

                FaturalarAc();
                FaturaSatiriniSec(enYuksek.FaturaNo);
                pnlAiAssistant.BringToFront();

                return
                    "En yüksek fatura:\r\n\r\n" +
                    enYuksek.FaturaNo + "\r\n" +
                    enYuksek.CariAdi + "\r\n" +
                    enYuksek.Tarih.ToString("dd.MM.yyyy") + "\r\n" +
                    enYuksek.GenelToplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // TOPLAM / NE KADAR?
            // -----------------------------------------------------
            if ((q.Contains("toplam") || q.Contains("ne kadar") ||
                 q.Contains("ciro")) &&
                (q.Contains("fatura") || q.Contains("kest") ||
                 q.Contains("ciro") || !string.IsNullOrWhiteSpace(cariAdi)))
            {
                decimal toplam = sonuc.Sum(f => f.GenelToplam);

                return
                    (string.IsNullOrWhiteSpace(cariAdi)
                        ? ""
                        : cariAdi + " için ") +
                    AiDonemAdi(buAy, gecenAy, bugun) +
                    sonuc.Count + " fatura var.\r\n" +
                    "Toplam tutar: " +
                    toplam.ToString("N2") + " TL";
            }

            // -----------------------------------------------------
            // KAÇ FATURA?
            // -----------------------------------------------------
            if (q.Contains("kaç") && q.Contains("fatura"))
            {
                return
                    AiDonemAdi(buAy, gecenAy, bugun) +
                    sonuc.Count + " fatura bulunuyor.";
            }

            // -----------------------------------------------------
            // FATURALARI GETİR / GÖSTER / LİSTELE
            // -----------------------------------------------------
            if (q.Contains("fatura") &&
                (q.Contains("getir") || q.Contains("göster") ||
                 q.Contains("goster") || q.Contains("listele") ||
                 !string.IsNullOrWhiteSpace(cariAdi) ||
                 tutarEsigi > 0))
            {
                if (sonuc.Count == 0)
                {
                    return
                        (string.IsNullOrWhiteSpace(cariAdi)
                            ? ""
                            : cariAdi + " için ") +
                        AiDonemAdi(buAy, gecenAy, bugun) +
                        "uygun fatura bulamadım.";
                }

                decimal toplam = sonuc.Sum(f => f.GenelToplam);

                FaturalarAc(cariAdi, baslangic, bitis);
                pnlAiAssistant.BringToFront();

                return
                    (string.IsNullOrWhiteSpace(cariAdi)
                        ? ""
                        : cariAdi + " için ") +
                    sonuc.Count + " fatura buldum.\r\n" +
                    "Toplam: " + toplam.ToString("N2") + " TL\r\n\r\n" +
                    "Faturalar ekranını açtım.";
            }

            if (q.Contains("yardım") || q.Contains("yardim") ||
                q.Contains("ne yapabilirsin"))
            {
                return
                    "Şunları deneyebilirsiniz:\r\n\r\n" +
                    "• Bu ay işler nasıl?\r\n" +
                    "• ABC Mobilya'nın bu ayki faturalarını getir\r\n" +
                    "• ABC'nin son 3 faturasını göster\r\n" +
                    "• Bu ay en çok hangi müşteriye fatura kestim?\r\n" +
                    "• Bu ay ile geçen ayı karşılaştır\r\n" +
                    "• 10.000 TL üzerindeki faturaları göster\r\n" +
                    "• En yüksek faturam hangisi?";
            }

            return
                "Bu cümleyi henüz güvenli bir veri sorgusuna çeviremedim.\r\n\r\n" +
                "Fatura, müşteri, dönem, toplam veya karşılaştırma içeren bir soru deneyin. " +
                "Örneğin “Bu ay işler nasıl?” yazabilirsiniz.";
        }

    }
}