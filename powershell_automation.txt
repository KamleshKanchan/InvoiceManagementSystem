# Invoice Management System - Automated Setup Script
# Save this file as: CreateInvoiceSystem.ps1
# Right-click and select "Run with PowerShell"

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Please run this script as Administrator!" -ForegroundColor Red
    Write-Host "Right-click the file and select 'Run as Administrator'" -ForegroundColor Yellow
    pause
    exit
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Invoice Management System - Auto Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Set project path
$projectPath = "C:\Projects\InvoiceManagementAPI"

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK installation..." -ForegroundColor Green
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found!" -ForegroundColor Red
    Write-Host "Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    pause
    exit
}

# Create Projects directory if it doesn't exist
Write-Host ""
Write-Host "Creating project directory..." -ForegroundColor Green
if (!(Test-Path "C:\Projects")) {
    New-Item -ItemType Directory -Path "C:\Projects" -Force | Out-Null
}

# Remove existing project if exists
if (Test-Path $projectPath) {
    Write-Host "Removing existing project..." -ForegroundColor Yellow
    Remove-Item -Path $projectPath -Recurse -Force
}

# Create new Web API project
Write-Host "Creating new ASP.NET Core Web API project..." -ForegroundColor Green
dotnet new webapi -n InvoiceManagementAPI -o $projectPath --force

# Navigate to project directory
Set-Location $projectPath

# Add NuGet packages
Write-Host ""
Write-Host "Installing NuGet packages..." -ForegroundColor Green
Write-Host "  - Microsoft.EntityFrameworkCore.SqlServer" -ForegroundColor Gray
dotnet add package Microsoft.EntityFrameworkCore.SqlServer --version 8.0.0

Write-Host "  - Microsoft.EntityFrameworkCore.Tools" -ForegroundColor Gray
dotnet add package Microsoft.EntityFrameworkCore.Tools --version 8.0.0

Write-Host "  - Microsoft.AspNetCore.Authentication.JwtBearer" -ForegroundColor Gray
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.0

Write-Host "  - System.IdentityModel.Tokens.Jwt" -ForegroundColor Gray
dotnet add package System.IdentityModel.Tokens.Jwt --version 7.0.0

# Create folder structure
Write-Host ""
Write-Host "Creating folder structure..." -ForegroundColor Green
New-Item -ItemType Directory -Path "Models" -Force | Out-Null
New-Item -ItemType Directory -Path "Data" -Force | Out-Null
New-Item -ItemType Directory -Path "Services" -Force | Out-Null

Write-Host "✓ Models folder created" -ForegroundColor Gray
Write-Host "✓ Data folder created" -ForegroundColor Gray
Write-Host "✓ Services folder created" -ForegroundColor Gray

# Create README file with next steps
$readmeContent = @"
# Invoice Management System

Project structure has been created automatically!

## Next Steps:

### 1. Copy Code Files
Copy the code from the artifacts into these files:

**Models folder:**
- User.cs
- Company.cs
- Client.cs
- BankAccount.cs
- Invoice.cs

**Data folder:**
- ApplicationDbContext.cs

**Services folder:**
- IAuthService.cs
- AuthService.cs
- IInvoiceService.cs
- InvoiceService.cs

**Controllers folder:**
- AuthController.cs
- UsersController.cs
- CompaniesController.cs
- ClientsController.cs
- BankAccountsController.cs
- InvoicesController.cs
- ReportsController.cs

**Root folder:**
- Replace Program.cs
- Replace appsettings.json

### 2. Update Connection String
Edit appsettings.json and update the connection string with your SQL Server name.

### 3. Build and Run
Open InvoiceManagementAPI.sln in Visual Studio and press F5.

### 4. Test
Navigate to: http://localhost:5000/swagger

## Project Location
$projectPath

## Created on: $(Get-Date)
"@

Set-Content -Path "README.md" -Value $readmeContent

# Create a batch file to open VS
$openVSBat = @"
@echo off
start InvoiceManagementAPI.sln
"@
Set-Content -Path "OpenInVisualStudio.bat" -Value $openVSBat

# Create template files with TODO comments
Write-Host ""
Write-Host "Creating template files..." -ForegroundColor Green

# Create Models templates
@"
// TODO: Copy User model code from artifact here
namespace InvoiceManagementAPI.Models
{
    // User class code goes here
}
"@ | Set-Content -Path "Models\User.cs"

@"
// TODO: Copy Company model code from artifact here
namespace InvoiceManagementAPI.Models
{
    // Company class code goes here
}
"@ | Set-Content -Path "Models\Company.cs"

@"
// TODO: Copy Client model code from artifact here
namespace InvoiceManagementAPI.Models
{
    // Client class code goes here
}
"@ | Set-Content -Path "Models\Client.cs"

@"
// TODO: Copy BankAccount model code from artifact here
namespace InvoiceManagementAPI.Models
{
    // BankAccount class code goes here
}
"@ | Set-Content -Path "Models\BankAccount.cs"

@"
// TODO: Copy Invoice model code from artifact here
namespace InvoiceManagementAPI.Models
{
    // Invoice class code goes here
}
"@ | Set-Content -Path "Models\Invoice.cs"

# Create Data template
@"
// TODO: Copy ApplicationDbContext code from artifact here
using Microsoft.EntityFrameworkCore;
using InvoiceManagementAPI.Models;

namespace InvoiceManagementAPI.Data
{
    // ApplicationDbContext class code goes here
}
"@ | Set-Content -Path "Data\ApplicationDbContext.cs"

# Create Services templates
@"
// TODO: Copy IAuthService code from artifact here
namespace InvoiceManagementAPI.Services
{
    // IAuthService interface code goes here
}
"@ | Set-Content -Path "Services\IAuthService.cs"

@"
// TODO: Copy AuthService code from artifact here
namespace InvoiceManagementAPI.Services
{
    // AuthService class code goes here
}
"@ | Set-Content -Path "Services\AuthService.cs"

@"
// TODO: Copy IInvoiceService code from artifact here
namespace InvoiceManagementAPI.Services
{
    // IInvoiceService interface code goes here
}
"@ | Set-Content -Path "Services\IInvoiceService.cs"

@"
// TODO: Copy InvoiceService code from artifact here
namespace InvoiceManagementAPI.Services
{
    // InvoiceService class code goes here
}
"@ | Set-Content -Path "Services\InvoiceService.cs"

# Create Controllers templates
@"
// TODO: Copy AuthController code from artifact here
using Microsoft.AspNetCore.Mvc;
using InvoiceManagementAPI.Services;

namespace InvoiceManagementAPI.Controllers
{
    // AuthController class code goes here
}
"@ | Set-Content -Path "Controllers\AuthController.cs"

# Update appsettings.json
$appsettingsContent = @"
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=InvoiceManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyMinimum32CharactersLong123456789012345",
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
  "Urls": "http://localhost:5000",
  "CorsOrigins": ["http://localhost:3000", "http://localhost:80", "http://localhost"]
}
"@
Set-Content -Path "appsettings.json" -Value $appsettingsContent -Force

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✓ Setup completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Project created at: $projectPath" -ForegroundColor Cyan
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host "1. Open the project folder: C:\Projects\InvoiceManagementAPI" -ForegroundColor White
Write-Host "2. Read the README.md file for instructions" -ForegroundColor White
Write-Host "3. Copy all code files from the artifacts" -ForegroundColor White
Write-Host "4. Double-click 'OpenInVisualStudio.bat' to open in Visual Studio" -ForegroundColor White
Write-Host "5. Build and run the project (press F5)" -ForegroundColor White
Write-Host ""
Write-Host "Opening project folder..." -ForegroundColor Green
Start-Sleep -Seconds 2
explorer $projectPath

Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
