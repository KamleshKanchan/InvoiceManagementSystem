-- Invoice Management System Database Schema
-- MS SQL Server 2022

-- Create Database
CREATE DATABASE InvoiceManagementDB;
GO

USE InvoiceManagementDB;
GO

-- Companies Table
CREATE TABLE Companies (
    CompanyId INT PRIMARY KEY IDENTITY(1,1),
    CompanyName NVARCHAR(200) NOT NULL,
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Country NVARCHAR(100),
    PostalCode NVARCHAR(20),
    Phone NVARCHAR(50),
    Email NVARCHAR(100),
    Website NVARCHAR(200),
    LogoUrl NVARCHAR(500),
    TaxNumber NVARCHAR(100),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE()
);

-- Users Table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(500) NOT NULL,
    FullName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Admin', 'InvoiceCreator', 'ViewOnly')),
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastLoginDate DATETIME,
    ModifiedDate DATETIME DEFAULT GETDATE()
);

-- Clients Table
CREATE TABLE Clients (
    ClientId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    ClientName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(200),
    Email NVARCHAR(100),
    Phone NVARCHAR(50),
    Address NVARCHAR(500),
    City NVARCHAR(100),
    State NVARCHAR(100),
    Country NVARCHAR(100),
    PostalCode NVARCHAR(20),
    TaxNumber NVARCHAR(100),
    Currency NVARCHAR(10) DEFAULT 'INR' CHECK (Currency IN ('INR', 'USD', 'EUR')),
    TaxRate DECIMAL(5,2) DEFAULT 0,
    TaxType NVARCHAR(50), -- GST, VAT, or NULL
    InvoiceNumberFormat NVARCHAR(100), -- e.g., "CLIENT-{YYYY}-{####}"
    InvoicePrefix NVARCHAR(50),
    LastInvoiceNumber INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

-- Bank Accounts Table
CREATE TABLE BankAccounts (
    BankAccountId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    AccountName NVARCHAR(200) NOT NULL,
    BankName NVARCHAR(200) NOT NULL,
    AccountNumber NVARCHAR(100) NOT NULL,
    IFSCCode NVARCHAR(50),
    SWIFTCode NVARCHAR(50),
    Branch NVARCHAR(200),
    Currency NVARCHAR(10) DEFAULT 'INR',
    AdditionalDetails NVARCHAR(MAX), -- JSON for future fields
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId)
);

-- Client Bank Mapping Table
CREATE TABLE ClientBankMapping (
    MappingId INT PRIMARY KEY IDENTITY(1,1),
    ClientId INT NOT NULL,
    BankAccountId INT NOT NULL,
    DisplayOrder INT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (BankAccountId) REFERENCES BankAccounts(BankAccountId)
);

-- Invoices Table
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    CompanyId INT NOT NULL,
    ClientId INT NOT NULL,
    InvoiceNumber NVARCHAR(100) NOT NULL UNIQUE,
    InvoiceDate DATE NOT NULL,
    DueDate DATE,
    Currency NVARCHAR(10) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
    TaxRate DECIMAL(5,2) DEFAULT 0,
    TaxAmount DECIMAL(18,2) DEFAULT 0,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Draft' CHECK (Status IN ('Draft', 'Sent', 'Paid', 'Cancelled')),
    Notes NVARCHAR(MAX),
    Terms NVARCHAR(MAX),
    CreatedBy INT NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    ModifiedBy INT,
    ModifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CompanyId) REFERENCES Companies(CompanyId),
    FOREIGN KEY (ClientId) REFERENCES Clients(ClientId),
    FOREIGN KEY (CreatedBy) REFERENCES Users(UserId),
    FOREIGN KEY (ModifiedBy) REFERENCES Users(UserId)
);

-- Invoice Items Table
CREATE TABLE InvoiceItems (
    InvoiceItemId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    LineNumber INT NOT NULL,
    Description NVARCHAR(500) NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Amount DECIMAL(18,2) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE
);

-- Invoice Bank Details Table (which bank account was selected for this invoice)
CREATE TABLE InvoiceBankDetails (
    InvoiceBankDetailId INT PRIMARY KEY IDENTITY(1,1),
    InvoiceId INT NOT NULL,
    BankAccountId INT NOT NULL,
    DisplayOrder INT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(InvoiceId) ON DELETE CASCADE,
    FOREIGN KEY (BankAccountId) REFERENCES BankAccounts(BankAccountId)
);

-- Audit Log Table
CREATE TABLE AuditLog (
    AuditId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT,
    Action NVARCHAR(100) NOT NULL,
    TableName NVARCHAR(100),
    RecordId INT,
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX),
    IPAddress NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- Create Indexes for Performance
CREATE INDEX IX_Clients_CompanyId ON Clients(CompanyId);
CREATE INDEX IX_Clients_ClientName ON Clients(ClientName);
CREATE INDEX IX_Invoices_CompanyId ON Invoices(CompanyId);
CREATE INDEX IX_Invoices_ClientId ON Invoices(ClientId);
CREATE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
CREATE INDEX IX_Invoices_InvoiceDate ON Invoices(InvoiceDate);
CREATE INDEX IX_Invoices_Status ON Invoices(Status);
CREATE INDEX IX_InvoiceItems_InvoiceId ON InvoiceItems(InvoiceId);
CREATE INDEX IX_BankAccounts_CompanyId ON BankAccounts(CompanyId);
CREATE INDEX IX_ClientBankMapping_ClientId ON ClientBankMapping(ClientId);
CREATE INDEX IX_InvoiceBankDetails_InvoiceId ON InvoiceBankDetails(InvoiceId);

-- Insert Default Data

-- Insert Companies
INSERT INTO Companies (CompanyName, Address, City, State, Country, Phone, Email) VALUES
('Sahir Projects', '123 Business St', 'Mumbai', 'Maharashtra', 'India', '+91-1234567890', 'info@sahirprojects.com'),
('Configurator Solutions', '456 Tech Park', 'Pune', 'Maharashtra', 'India', '+91-0987654321', 'info@configuratorsolutions.com');

-- Insert Default Admin User (Password: Admin@123 - must be hashed in application)
INSERT INTO Users (Username, Email, PasswordHash, FullName, Role) VALUES
('admin', 'admin@company.com', 'HASH_TO_BE_GENERATED', 'System Administrator', 'Admin');

-- Insert Sample Bank Accounts for Sahir Projects
INSERT INTO BankAccounts (CompanyId, AccountName, BankName, AccountNumber, IFSCCode, Branch, Currency) VALUES
(1, 'Sahir Projects Current Account', 'HDFC Bank', '12345678901234', 'HDFC0001234', 'Mumbai Branch', 'INR'),
(1, 'Sahir Projects USD Account', 'ICICI Bank', '98765432109876', 'ICIC0004567', 'Mumbai International', 'USD');

-- Insert Sample Bank Accounts for Configurator Solutions
INSERT INTO BankAccounts (CompanyId, AccountName, BankName, AccountNumber, IFSCCode, SWIFTCode, Branch, Currency) VALUES
(2, 'Configurator Solutions INR Account', 'State Bank of India', '11112222333344', 'SBIN0001111', NULL, 'Pune Branch', 'INR'),
(2, 'Configurator Solutions EUR Account', 'Axis Bank', '55556666777788', 'UTIB0002222', 'AXISINBB123', 'Pune International', 'EUR');

GO

-- Stored Procedures

-- Get Next Invoice Number for Client
CREATE PROCEDURE sp_GetNextInvoiceNumber
    @ClientId INT,
    @NextInvoiceNumber NVARCHAR(100) OUTPUT
AS
BEGIN
    DECLARE @Format NVARCHAR(100);
    DECLARE @Prefix NVARCHAR(50);
    DECLARE @LastNumber INT;
    DECLARE @Year NVARCHAR(4) = CAST(YEAR(GETDATE()) AS NVARCHAR(4));
    DECLARE @Month NVARCHAR(2) = RIGHT('0' + CAST(MONTH(GETDATE()) AS NVARCHAR(2)), 2);
    
    SELECT 
        @Format = InvoiceNumberFormat,
        @Prefix = InvoicePrefix,
        @LastNumber = LastInvoiceNumber
    FROM Clients
    WHERE ClientId = @ClientId;
    
    SET @LastNumber = @LastNumber + 1;
    
    -- Replace placeholders in format
    SET @NextInvoiceNumber = @Format;
    SET @NextInvoiceNumber = REPLACE(@NextInvoiceNumber, '{YYYY}', @Year);
    SET @NextInvoiceNumber = REPLACE(@NextInvoiceNumber, '{MM}', @Month);
    SET @NextInvoiceNumber = REPLACE(@NextInvoiceNumber, '{####}', RIGHT('0000' + CAST(@LastNumber AS NVARCHAR(10)), 4));
    SET @NextInvoiceNumber = REPLACE(@NextInvoiceNumber, '{###}', RIGHT('000' + CAST(@LastNumber AS NVARCHAR(10)), 3));
    
    -- Update last invoice number
    UPDATE Clients 
    SET LastInvoiceNumber = @LastNumber,
        ModifiedDate = GETDATE()
    WHERE ClientId = @ClientId;
END;
GO

-- Get Dashboard Statistics
CREATE PROCEDURE sp_GetDashboardStats
    @CompanyId INT = NULL
AS
BEGIN
    -- Total Clients
    SELECT COUNT(*) AS TotalClients
    FROM Clients
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND IsActive = 1;
    
    -- Total Invoices
    SELECT COUNT(*) AS TotalInvoices
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId);
    
    -- Draft Invoices
    SELECT COUNT(*) AS DraftInvoices
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND Status = 'Draft';
    
    -- Sent Invoices
    SELECT COUNT(*) AS SentInvoices
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND Status = 'Sent';
    
    -- Paid Invoices
    SELECT COUNT(*) AS PaidInvoices
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND Status = 'Paid';
    
    -- Total Revenue by Currency
    SELECT 
        Currency,
        SUM(TotalAmount) AS TotalRevenue
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND Status = 'Paid'
    GROUP BY Currency;
    
    -- Monthly Revenue (Last 12 Months)
    SELECT 
        FORMAT(InvoiceDate, 'yyyy-MM') AS Month,
        Currency,
        SUM(TotalAmount) AS Revenue
    FROM Invoices
    WHERE (@CompanyId IS NULL OR CompanyId = @CompanyId)
    AND InvoiceDate >= DATEADD(MONTH, -12, GETDATE())
    AND Status = 'Paid'
    GROUP BY FORMAT(InvoiceDate, 'yyyy-MM'), Currency
    ORDER BY Month DESC;
END;
GO

PRINT 'Database schema created successfully!';
PRINT 'Default admin user created: username=admin, password=Admin@123 (must be hashed in application)';
PRINT 'Two companies created: Sahir Projects and Configurator Solutions';
PRINT 'Sample bank accounts created for both companies';
