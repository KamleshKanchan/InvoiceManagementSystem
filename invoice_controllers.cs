// ============================================
// MISSING CONTROLLERS PACKAGE
// Copy each controller to the Controllers folder
// ============================================

// ============================================
// File: Controllers/InvoicesController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementAPI.Data;
using InvoiceManagementAPI.Models;
using InvoiceManagementAPI.Services;
using System.Security.Claims;

namespace InvoiceManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(ApplicationDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetInvoices([FromQuery] int? companyId = null, [FromQuery] string status = null)
        {
            var query = _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Include(i => i.Creator)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            var invoices = await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
            
            foreach (var invoice in invoices)
            {
                if (invoice.Creator != null)
                {
                    invoice.Creator.PasswordHash = null;
                }
            }

            return invoices;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Include(i => i.Items.OrderBy(item => item.LineNumber))
                .Include(i => i.BankDetails)
                    .ThenInclude(bd => bd.BankAccount)
                .Include(i => i.Creator)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            if (invoice.Creator != null)
            {
                invoice.Creator.PasswordHash = null;
            }

            return invoice;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<ActionResult<Invoice>> PostInvoice(Invoice invoice)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var invoiceNumber = await _invoiceService.GenerateInvoiceNumber(invoice.ClientId);
            if (string.IsNullOrEmpty(invoiceNumber))
                return BadRequest("Unable to generate invoice number");

            invoice.InvoiceNumber = invoiceNumber;
            invoice.CreatedBy = userId;
            invoice.CreatedDate = DateTime.Now;
            invoice.ModifiedDate = DateTime.Now;

            invoice.SubTotal = invoice.Items.Sum(item => item.Amount);
            invoice.TaxAmount = invoice.SubTotal * (invoice.TaxRate / 100);
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            for (int i = 0; i < invoice.Items.Count; i++)
            {
                invoice.Items[i].InvoiceId = invoice.InvoiceId;
                invoice.Items[i].LineNumber = i + 1;
            }

            if (invoice.BankDetails != null)
            {
                foreach (var bankDetail in invoice.BankDetails)
                {
                    bankDetail.InvoiceId = invoice.InvoiceId;
                }
            }

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.InvoiceId }, invoice);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<IActionResult> PutInvoice(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceId)
                return BadRequest();

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var userId = int.Parse(userIdClaim);

            var existingInvoice = await _context.Invoices
                .Include(i => i.Items)
                .Include(i => i.BankDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (existingInvoice == null)
                return NotFound();

            existingInvoice.InvoiceDate = invoice.InvoiceDate;
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.Status = invoice.Status;
            existingInvoice.Notes = invoice.Notes;
            existingInvoice.Terms = invoice.Terms;
            existingInvoice.TaxRate = invoice.TaxRate;
            existingInvoice.ModifiedBy = userId;
            existingInvoice.ModifiedDate = DateTime.Now;

            _context.InvoiceItems.RemoveRange(existingInvoice.Items);
            
            for (int i = 0; i < invoice.Items.Count; i++)
            {
                invoice.Items[i].InvoiceId = id;
                invoice.Items[i].LineNumber = i + 1;
                _context.InvoiceItems.Add(invoice.Items[i]);
            }

            existingInvoice.SubTotal = invoice.Items.Sum(item => item.Amount);
            existingInvoice.TaxAmount = existingInvoice.SubTotal * (existingInvoice.TaxRate / 100);
            existingInvoice.TotalAmount = existingInvoice.SubTotal + existingInvoice.TaxAmount;

            _context.InvoiceBankDetails.RemoveRange(existingInvoice.BankDetails);
            
            if (invoice.BankDetails != null)
            {
                foreach (var bankDetail in invoice.BankDetails)
                {
                    bankDetail.InvoiceId = id;
                    _context.InvoiceBankDetails.Add(bankDetail);
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("generate-number/{clientId}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<ActionResult<string>> GenerateInvoiceNumber(int clientId)
        {
            var invoiceNumber = await _invoiceService.GenerateInvoiceNumber(clientId);
            
            if (string.IsNullOrEmpty(invoiceNumber))
                return BadRequest("Unable to generate invoice number");

            return Ok(new { invoiceNumber });
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<object>> GetDashboardStats([FromQuery] int? companyId = null)
        {
            var query = _context.Invoices.AsQueryable();
            
            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            var totalInvoices = await query.CountAsync();
            var draftInvoices = await query.CountAsync(i => i.Status == "Draft");
            var sentInvoices = await query.CountAsync(i => i.Status == "Sent");
            var paidInvoices = await query.CountAsync(i => i.Status == "Paid");

            var revenueByurrency = await query
                .Where(i => i.Status == "Paid")
                .GroupBy(i => i.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(i => i.TotalAmount) })
                .ToListAsync();

            var monthlyRevenue = await query
                .Where(i => i.Status == "Paid" && i.InvoiceDate >= DateTime.Now.AddMonths(-12))
                .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month, i.Currency })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Currency = g.Key.Currency,
                    Revenue = g.Sum(i => i.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToListAsync();

            return Ok(new
            {
                totalInvoices,
                draftInvoices,
                sentInvoices,
                paidInvoices,
                revenueByurrency,
                monthlyRevenue
            });
        }
    }
}

// ============================================
// File: Controllers/BankAccountsController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementAPI.Data;
using InvoiceManagementAPI.Models;

namespace InvoiceManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BankAccountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BankAccountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetBankAccounts([FromQuery] int? companyId = null)
        {
            var query = _context.BankAccounts
                .Include(b => b.Company)
                .Where(b => b.IsActive)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(b => b.CompanyId == companyId.Value);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BankAccount>> GetBankAccount(int id)
        {
            var bankAccount = await _context.BankAccounts
                .Include(b => b.Company)
                .FirstOrDefaultAsync(b => b.BankAccountId == id);

            if (bankAccount == null)
                return NotFound();

            return bankAccount;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutBankAccount(int id, BankAccount bankAccount)
        {
            if (id != bankAccount.BankAccountId)
                return BadRequest();

            bankAccount.ModifiedDate = DateTime.Now;
            _context.Entry(bankAccount).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BankAccount>> PostBankAccount(BankAccount bankAccount)
        {
            bankAccount.CreatedDate = DateTime.Now;
            bankAccount.ModifiedDate = DateTime.Now;
            _context.BankAccounts.Add(bankAccount);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBankAccount), new { id = bankAccount.BankAccountId }, bankAccount);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBankAccount(int id)
        {
            var bankAccount = await _context.BankAccounts.FindAsync(id);
            if (bankAccount == null)
                return NotFound();

            bankAccount.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("client-mapping")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<ActionResult> MapBankToClient(ClientBankMapping mapping)
        {
            mapping.CreatedDate = DateTime.Now;
            _context.ClientBankMappings.Add(mapping);
            await _context.SaveChangesAsync();

            return Ok(mapping);
        }

        [HttpDelete("client-mapping/{mappingId}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<IActionResult> RemoveBankFromClient(int mappingId)
        {
            var mapping = await _context.ClientBankMappings.FindAsync(mappingId);
            if (mapping == null)
                return NotFound();

            _context.ClientBankMappings.Remove(mapping);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("by-client/{clientId}")]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetBankAccountsByClient(int clientId)
        {
            var bankAccounts = await _context.ClientBankMappings
                .Where(m => m.ClientId == clientId && m.IsActive)
                .Include(m => m.BankAccount)
                    .ThenInclude(b => b.Company)
                .OrderBy(m => m.DisplayOrder)
                .Select(m => m.BankAccount)
                .ToListAsync();

            return bankAccounts;
        }
    }
}

// ============================================
// File: Controllers/ReportsController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagementAPI.Data;

namespace InvoiceManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("sales-by-client")]
        public async Task<ActionResult<object>> GetSalesByClient([FromQuery] int? companyId = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var query = _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.Status == "Paid")
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(i => i.InvoiceDate <= endDate.Value);
            }

            var salesByClient = await query
                .GroupBy(i => new { i.ClientId, i.Client.ClientName, i.Currency })
                .Select(g => new
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.ClientName,
                    Currency = g.Key.Currency,
                    TotalSales = g.Sum(i => i.TotalAmount),
                    InvoiceCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSales)
                .ToListAsync();

            return Ok(salesByClient);
        }

        [HttpGet("monthly-revenue")]
        public async Task<ActionResult<object>> GetMonthlyRevenue([FromQuery] int? companyId = null, [FromQuery] int months = 12)
        {
            var startDate = DateTime.Now.AddMonths(-months);
            
            var query = _context.Invoices
                .Where(i => i.Status == "Paid" && i.InvoiceDate >= startDate)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            var monthlyRevenue = await query
                .GroupBy(i => new
                {
                    Year = i.InvoiceDate.Year,
                    Month = i.InvoiceDate.Month,
                    Currency = i.Currency
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Currency = g.Key.Currency,
                    Revenue = g.Sum(i => i.TotalAmount),
                    InvoiceCount = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return Ok(monthlyRevenue);
        }

        [HttpGet("status-summary")]
        public async Task<ActionResult<object>> GetStatusSummary([FromQuery] int? companyId = null)
        {
            var query = _context.Invoices.AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            var statusSummary = await query
                .GroupBy(i => new { i.Status, i.Currency })
                .Select(g => new
                {
                    Status = g.Key.Status,
                    Currency = g.Key.Currency,
                    Count = g.Count(),
                    TotalAmount = g.Sum(i => i.TotalAmount)
                })
                .ToListAsync();

            return Ok(statusSummary);
        }

        [HttpGet("top-clients")]
        public async Task<ActionResult<object>> GetTopClients([FromQuery] int? companyId = null, [FromQuery] int top = 10)
        {
            var query = _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.Status == "Paid")
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            var topClients = await query
                .GroupBy(i => new { i.ClientId, i.Client.ClientName })
                .Select(g => new
                {
                    ClientId = g.Key.ClientId,
                    ClientName = g.Key.ClientName,
                    TotalRevenue = g.Sum(i => i.TotalAmount),
                    InvoiceCount = g.Count()
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(top)
                .ToListAsync();

            return Ok(topClients);
        }
    }
}