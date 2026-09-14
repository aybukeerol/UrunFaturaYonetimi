using Microsoft.Data.SqlClient;

namespace UrunFaturaYonetimi.DataAccess
{
    public static class DatabaseInitializer
    {
        private const string DatabaseName = "UrunFaturaYonetimiDb";

        // Önce master veritabanına bağlanıp uygulama veritabanını oluşturur.
        private static readonly string MasterConnectionString =
            @"Server=(localdb)\MSSQLLocalDB;
              Database=master;
              Trusted_Connection=True;
              TrustServerCertificate=True;";

        // Uygulamanın normalde kullanacağı asıl bağlantı.
        public static readonly string ConnectionString =
            $@"Server=(localdb)\MSSQLLocalDB;
               Database={DatabaseName};
               Trusted_Connection=True;
               TrustServerCertificate=True;";

        public static void Initialize()
        {
            CreateDatabase();
            CreateTables();
        }

        private static void CreateDatabase()
        {
            using (var conn = new SqlConnection(MasterConnectionString))
            {
                conn.Open();

                string sql = $@"
                    IF DB_ID(N'{DatabaseName}') IS NULL
                    BEGIN
                        CREATE DATABASE [{DatabaseName}]
                    END;
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static void CreateTables()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                string sql = @"
                    IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Products
                        (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(200) NOT NULL,
                            Category NVARCHAR(150) NULL,
                            UnitPrice DECIMAL(18,2) NOT NULL,
                            StockQuantity INT NOT NULL,
                            Image VARBINARY(MAX) NULL
                        );
                    END;

                    IF OBJECT_ID(N'dbo.Invoices', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.Invoices
                        (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            InvoiceNumber NVARCHAR(100) NOT NULL,
                            InvoiceDate DATETIME2 NOT NULL,
                            CustomerTitle NVARCHAR(MAX) NOT NULL,
                            TotalAmount DECIMAL(18,2) NOT NULL
                        );
                    END;

                    IF OBJECT_ID(N'dbo.CurrentAccounts', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.CurrentAccounts
                        (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            AccountName NVARCHAR(250) NOT NULL,
                            AccountType NVARCHAR(50) NULL,
                            TaxOrIdNumber NVARCHAR(50) NULL,
                            Address NVARCHAR(MAX) NULL
                        );
                    END;

                    IF OBJECT_ID(N'dbo.InvoiceDetails', N'U') IS NULL
                    BEGIN
                        CREATE TABLE dbo.InvoiceDetails
                        (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            InvoiceId INT NOT NULL,
                            ProductId INT NOT NULL,
                            Quantity INT NOT NULL,
                            UnitPrice DECIMAL(18,2) NOT NULL,
                            LineTotal DECIMAL(18,2) NOT NULL,

                            CONSTRAINT FK_InvoiceDetails_Invoices
                                FOREIGN KEY (InvoiceId)
                                REFERENCES dbo.Invoices(Id)
                                ON DELETE CASCADE,

                            CONSTRAINT FK_InvoiceDetails_Products
                                FOREIGN KEY (ProductId)
                                REFERENCES dbo.Products(Id)
                        );
                    END;
                ";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}