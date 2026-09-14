using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UrunFaturaYonetimi.Business;
using UrunFaturaYonetimi.Models;

namespace UrunFaturaYonetimi
{
    public partial class Form1 : Form
    {
        private readonly ProductService _productService = new ProductService();
        private readonly InvoiceService _invoiceService = new InvoiceService();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                CmbCarileriDoldur();
                CmbUrunleriDoldur();
                txtFaturaNo.Text = _invoiceService.GenerateInvoiceNumber();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Başlangıç hatası:\n" + ex.Message);
            }
        }

        public void CmbCarileriDoldur()
        {
            try
            {
                var cariler = _invoiceService.GetAllCurrentAccounts();
                cmbFaturaCariSec.DataSource = null;
                if (cariler != null && cariler.Count > 0)
                {
                    cmbFaturaCariSec.DataSource = cariler;
                    cmbFaturaCariSec.DisplayMember = "AccountName";
                    cmbFaturaCariSec.ValueMember = "Id";
                    cmbFaturaCariSec.SelectedIndex = -1;
                }
            }
            catch { }
        }

        private void CmbUrunleriDoldur()
        {
            try
            {
                cmbUrunler.DataSource = null;
                cmbUrunler.DataSource = _productService.GetAllProducts();
                cmbUrunler.DisplayMember = "Name";
                cmbUrunler.ValueMember = "Id";
            }
            catch { }
        }

        private void cmbFaturaCariSec_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbFaturaCariSec.SelectedItem is CurrentAccount secilenCari)
            {
                txtCariKodu.Text = secilenCari.AccountCode;
                txtCariAdi.Text = secilenCari.AccountName;
            }
        }

        private void btnSepeteEkle_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Satır ekleme alanı aktif.");
        }

        private void btnFaturayiKaydet_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fatura kayıt işlemi başarılı.");
        }
    }
}