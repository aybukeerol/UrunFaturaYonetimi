namespace UrunFaturaYonetimi
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        public System.Windows.Forms.DataGridView dgvFaturaHareketleri;
        public System.Windows.Forms.ComboBox cmbFaturaCariSec;
        public System.Windows.Forms.ComboBox cmbUrunler;
        public System.Windows.Forms.TextBox txtCariKodu;
        public System.Windows.Forms.TextBox txtCariAdi;
        public System.Windows.Forms.TextBox txtFaturaNo;
        public System.Windows.Forms.Button btnSepeteEkle;
        public System.Windows.Forms.Button btnFaturayiKaydet;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.dgvFaturaHareketleri = new System.Windows.Forms.DataGridView();
            this.cmbFaturaCariSec = new System.Windows.Forms.ComboBox();
            this.cmbUrunler = new System.Windows.Forms.ComboBox();
            this.txtCariKodu = new System.Windows.Forms.TextBox();
            this.txtCariAdi = new System.Windows.Forms.TextBox();
            this.txtFaturaNo = new System.Windows.Forms.TextBox();
            this.btnSepeteEkle = new System.Windows.Forms.Button();
            this.btnFaturayiKaydet = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)(this.dgvFaturaHareketleri)).BeginInit();
            this.SuspendLayout();

            // --- ÜST BİLGİ ALANI (Cari ve Fatura No) ---
            this.cmbFaturaCariSec.Location = new System.Drawing.Point(20, 25);
            this.cmbFaturaCariSec.Name = "cmbFaturaCariSec";
            this.cmbFaturaCariSec.Size = new System.Drawing.Size(220, 24);
            this.cmbFaturaCariSec.SelectedIndexChanged += new System.EventHandler(this.cmbFaturaCariSec_SelectedIndexChanged);

            this.txtCariKodu.Location = new System.Drawing.Point(255, 25);
            this.txtCariKodu.Name = "txtCariKodu";
            this.txtCariKodu.Size = new System.Drawing.Size(130, 22);
            this.txtCariKodu.Text = "Cari Kodu";

            this.txtCariAdi.Location = new System.Drawing.Point(395, 25);
            this.txtCariAdi.Name = "txtCariAdi";
            this.txtCariAdi.Size = new System.Drawing.Size(220, 22);
            this.txtCariAdi.Text = "Cari Adı";

            this.txtFaturaNo.Location = new System.Drawing.Point(830, 25);
            this.txtFaturaNo.Name = "txtFaturaNo";
            this.txtFaturaNo.Size = new System.Drawing.Size(150, 22);
            this.txtFaturaNo.Text = "Fatura No";

            // --- ÜRÜN SEÇİM VE SATIR EKLEME ALANI ---
            this.cmbUrunler.Location = new System.Drawing.Point(20, 65);
            this.cmbUrunler.Name = "cmbUrunler";
            this.cmbUrunler.Size = new System.Drawing.Size(220, 24);

            this.btnSepeteEkle.Location = new System.Drawing.Point(255, 63);
            this.btnSepeteEkle.Name = "btnSepeteEkle";
            this.btnSepeteEkle.Size = new System.Drawing.Size(130, 28);
            this.btnSepeteEkle.Text = "+ Satır Ekle";
            this.btnSepeteEkle.UseVisualStyleBackColor = true;
            this.btnSepeteEkle.Click += new System.EventHandler(this.btnSepeteEkle_Click);

            // --- ORTA KISIM: GENİŞ EXCEL TABLOSU ---
            this.dgvFaturaHareketleri.Location = new System.Drawing.Point(20, 110);
            this.dgvFaturaHareketleri.Name = "dgvFaturaHareketleri";
            this.dgvFaturaHareketleri.RowHeadersWidth = 51;
            this.dgvFaturaHareketleri.Size = new System.Drawing.Size(960, 380);
            this.dgvFaturaHareketleri.TabIndex = 0;

            // --- ALT KISIM: KAYDET BUTONU ---
            this.btnFaturayiKaydet.Location = new System.Drawing.Point(830, 510);
            this.btnFaturayiKaydet.Name = "btnFaturayiKaydet";
            this.btnFaturayiKaydet.Size = new System.Drawing.Size(150, 40);
            this.btnFaturayiKaydet.Text = "Faturayı Kaydet";
            this.btnFaturayiKaydet.UseVisualStyleBackColor = true;
            this.btnFaturayiKaydet.Click += new System.EventHandler(this.btnFaturayiKaydet_Click);

            // --- FORM GENEL BOYUTU VE AYARLARI ---
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1010, 570);
            this.Controls.Add(this.cmbFaturaCariSec);
            this.Controls.Add(this.txtCariKodu);
            this.Controls.Add(this.txtCariAdi);
            this.Controls.Add(this.txtFaturaNo);
            this.Controls.Add(this.cmbUrunler);
            this.Controls.Add(this.btnSepeteEkle);
            this.Controls.Add(this.dgvFaturaHareketleri);
            this.Controls.Add(this.btnFaturayiKaydet);
            this.Text = "Ürün & Fatura Yönetimi - Ön Muhasebe";

            ((System.ComponentModel.ISupportInitialize)(this.dgvFaturaHareketleri)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}