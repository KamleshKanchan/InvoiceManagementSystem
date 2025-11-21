// ============================================
// File: appsettings.json
// ============================================
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMinimum32CharactersLong123456",
    "Issuer": "InvoiceManagementAPI",
    "Audience": "InvoiceManagementClient",
    "ExpiryMinutes": 480
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CorsOrigins": ["http://localhost:3000", "http://localhost:5173"]
}

// ============================================
// File: Models/User.cs
// ============================================
using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models
{
    public class User
    {
        public int UserId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Username { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [Required]
        public string PasswordHash { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }
        
        [Required]
        public string Role { get; set; } // Admin, InvoiceCreator, ViewOnly
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastLoginDate { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }

    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        public string Password { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
    }
}

// ============================================
// File: Models/Company.cs
// ============================================
using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models
{
    public class Company
    {
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; }
        
        [StringLength(500)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(100)]
        public string State { get; set; }
        
        [StringLength(100)]
        public string Country { get; set; }
        
        [StringLength(20)]
        public string PostalCode { get; set; }
        
        [StringLength(50)]
        public string Phone { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [StringLength(200)]
        public string Website { get; set; }
        
        [StringLength(500)]
        public string LogoUrl { get; set; }
        
        [StringLength(100)]
        public string TaxNumber { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
    }
}

// ============================================
// File: Models/Client.cs
// ============================================
using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models
{
    public class Client
    {
        public int ClientId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string ClientName { get; set; }
        
        [StringLength(200)]
        public string ContactPerson { get; set; }
        
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }
        
        [StringLength(50)]
        public string Phone { get; set; }
        
        [StringLength(500)]
        public string Address { get; set; }
        
        [StringLength(100)]
        public string City { get; set; }
        
        [StringLength(100)]
        public string State { get; set; }
        
        [StringLength(100)]
        public string Country { get; set; }
        
        [StringLength(20)]
        public string PostalCode { get; set; }
        
        [StringLength(100)]
        public string TaxNumber { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "INR"; // INR, USD, EUR
        
        public decimal TaxRate { get; set; } = 0;
        
        [StringLength(50)]
        public string TaxType { get; set; } // GST, VAT, or NULL
        
        [StringLength(100)]
        public string InvoiceNumberFormat { get; set; }
        
        [StringLength(50)]
        public string InvoicePrefix { get; set; }
        
        public int LastInvoiceNumber { get; set; } = 0;
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Company Company { get; set; }
    }
}

// ============================================
// File: Models/BankAccount.cs
// ============================================
using System;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models
{
    public class BankAccount
    {
        public int BankAccountId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string AccountName { get; set; }
        
        [Required]
        [StringLength(200)]
        public string BankName { get; set; }
        
        [Required]
        [StringLength(100)]
        public string AccountNumber { get; set; }
        
        [StringLength(50)]
        public string IFSCCode { get; set; }
        
        [StringLength(50)]
        public string SWIFTCode { get; set; }
        
        [StringLength(200)]
        public string Branch { get; set; }
        
        [StringLength(10)]
        public string Currency { get; set; } = "INR";
        
        public string AdditionalDetails { get; set; }
        
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Company Company { get; set; }
    }

    public class ClientBankMapping
    {
        public int MappingId { get; set; }
        
        [Required]
        public int ClientId { get; set; }
        
        [Required]
        public int BankAccountId { get; set; }
        
        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Client Client { get; set; }
        public BankAccount BankAccount { get; set; }
    }
}

// ============================================
// File: Models/Invoice.cs
// ============================================
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace InvoiceManagement.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        public int ClientId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string InvoiceNumber { get; set; }
        
        [Required]
        public DateTime InvoiceDate { get; set; }
        
        public DateTime? DueDate { get; set; }
        
        [Required]
        [StringLength(10)]
        public string Currency { get; set; }
        
        public decimal SubTotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        
        [StringLength(50)]
        public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Cancelled
        
        public string Notes { get; set; }
        public string Terms { get; set; }
        
        [Required]
        public int CreatedBy { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int? ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Company Company { get; set; }
        public Client Client { get; set; }
        public User Creator { get; set; }
        public List<InvoiceItem> Items { get; set; }
        public List<InvoiceBankDetail> BankDetails { get; set; }
    }

    public class InvoiceItem
    {
        public int InvoiceItemId { get; set; }
        
        [Required]
        public int InvoiceId { get; set; }
        
        [Required]
        public int LineNumber { get; set; }
        
        [Required]
        [StringLength(500)]
        public string Description { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [Required]
        public decimal UnitPrice { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Invoice Invoice { get; set; }
    }

    public class InvoiceBankDetail
    {
        public int InvoiceBankDetailId { get; set; }
        
        [Required]
        public int InvoiceId { get; set; }
        
        [Required]
        public int BankAccountId { get; set; }
        
        public int DisplayOrder { get; set; } = 1;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Navigation
        public Invoice Invoice { get; set; }
        public BankAccount BankAccount { get; set; }
    }
}

// ============================================
// File: Data/ApplicationDbContext.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Models;

namespace InvoiceManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<ClientBankMapping> ClientBankMappings { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<InvoiceBankDetail> InvoiceBankDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure precision for decimal columns
            modelBuilder.Entity<Invoice>()
                .Property(i => i.SubTotal).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TaxRate).HasPrecision(5, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount).HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.UnitPrice).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>()
                .Property(i => i.Amount).HasPrecision(18, 2);

            modelBuilder.Entity<Client>()
                .Property(c => c.TaxRate).HasPrecision(5, 2);

            // Configure relationships
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Company)
                .WithMany()
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Client)
                .WithMany()
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Creator)
                .WithMany()
                .HasForeignKey(i => i.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

// ============================================
// File: Program.cs
// ============================================
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using InvoiceManagement.Data;
using InvoiceManagement.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("CorsOrigins").Get<string[]>())
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
