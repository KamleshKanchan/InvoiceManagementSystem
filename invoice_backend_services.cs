// ============================================
// File: Services/IAuthService.cs
// ============================================
using InvoiceManagement.Models;

namespace InvoiceManagement.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginRequest request);
        string GenerateJwtToken(User user);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}

// ============================================
// File: Services/AuthService.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InvoiceManagement.Data;
using InvoiceManagement.Models;

namespace InvoiceManagement.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
            {
                return null;
            }

            // Update last login date
            user.LastLoginDate = DateTime.Now;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Token = token,
                UserId = user.UserId,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Email = user.Email
            };
        }

        public string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("FullName", user.FullName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}

// ============================================
// File: Services/IInvoiceService.cs
// ============================================
using InvoiceManagement.Models;

namespace InvoiceManagement.Services
{
    public interface IInvoiceService
    {
        Task<string> GenerateInvoiceNumber(int clientId);
        Task<Invoice> CreateInvoice(Invoice invoice);
        Task<Invoice> UpdateInvoice(Invoice invoice);
        Task<bool> DeleteInvoice(int invoiceId);
        Task<Invoice> GetInvoiceById(int invoiceId);
        Task<List<Invoice>> GetAllInvoices(int? companyId = null);
    }
}

// ============================================
// File: Services/InvoiceService.cs
// ============================================
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;

namespace InvoiceManagement.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateInvoiceNumber(int clientId)
        {
            var client = await _context.Clients.FindAsync(clientId);
            if (client == null) return null;

            var nextNumber = client.LastInvoiceNumber + 1;
            var year = DateTime.Now.Year.ToString();
            var month = DateTime.Now.Month.ToString("00");

            var invoiceNumber = client.InvoiceNumberFormat
                .Replace("{YYYY}", year)
                .Replace("{MM}", month)
                .Replace("{####}", nextNumber.ToString("0000"))
                .Replace("{###}", nextNumber.ToString("000"));

            client.LastInvoiceNumber = nextNumber;
            client.ModifiedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return invoiceNumber;
        }

        public async Task<Invoice> CreateInvoice(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<Invoice> UpdateInvoice(Invoice invoice)
        {
            _context.Entry(invoice).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> DeleteInvoice(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Invoice> GetInvoiceById(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .Include(i => i.Items)
                .Include(i => i.BankDetails)
                    .ThenInclude(bd => bd.BankAccount)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<List<Invoice>> GetAllInvoices(int? companyId = null)
        {
            var query = _context.Invoices
                .Include(i => i.Company)
                .Include(i => i.Client)
                .AsQueryable();

            if (companyId.HasValue)
            {
                query = query.Where(i => i.CompanyId == companyId.Value);
            }

            return await query.OrderByDescending(i => i.CreatedDate).ToListAsync();
        }
    }
}

// ============================================
// File: Controllers/AuthController.cs
// ============================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;
using InvoiceManagement.Services;

namespace InvoiceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ApplicationDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
        {
            var response = await _authService.Login(request);
            
            if (response == null)
                return Unauthorized(new { message = "Invalid username or password" });

            return Ok(response);
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return BadRequest(new { message = "Username already exists" });

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return BadRequest(new { message = "Email already exists" });

            user.PasswordHash = _authService.HashPassword(user.PasswordHash); // PasswordHash contains plain password on input
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Remove password hash from response
            user.PasswordHash = null;
            return CreatedAtAction(nameof(Register), new { id = user.UserId }, user);
        }
    }
}

// ============================================
// File: Controllers/UsersController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;
using InvoiceManagement.Services;

namespace InvoiceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthService _authService;

        public UsersController(ApplicationDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            foreach (var user in users)
            {
                user.PasswordHash = null; // Don't send password hashes
            }
            return users;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return NotFound();

            user.PasswordHash = null;
            return user;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.UserId)
                return BadRequest();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;
            existingUser.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                existingUser.PasswordHash = _authService.HashPassword(user.PasswordHash);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            user.PasswordHash = _authService.HashPassword(user.PasswordHash);
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            user.PasswordHash = null;
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            user.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

// ============================================
// File: Controllers/CompaniesController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;

namespace InvoiceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompaniesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            return await _context.Companies.Where(c => c.IsActive).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            if (company == null)
                return NotFound();

            return company;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> PutCompany(int id, Company company)
        {
            if (id != company.CompanyId)
                return BadRequest();

            company.ModifiedDate = DateTime.Now;
            _context.Entry(company).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Company>> PostCompany(Company company)
        {
            company.CreatedDate = DateTime.Now;
            company.ModifiedDate = DateTime.Now;
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCompany), new { id = company.CompanyId }, company);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            company.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.CompanyId == id);
        }
    }
}

// ============================================
// File: Controllers/ClientsController.cs
// ============================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceManagement.Data;
using InvoiceManagement.Models;

namespace InvoiceManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients([FromQuery] int? companyId = null)
        {
            var query = _context.Clients.Include(c => c.Company).Where(c => c.IsActive);
            
            if (companyId.HasValue)
            {
                query = query.Where(c => c.CompanyId == companyId.Value);
            }

            return await query.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(int id)
        {
            var client = await _context.Clients
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.ClientId == id);

            if (client == null)
                return NotFound();

            return client;
        }

        [HttpGet("{id}/banks")]
        public async Task<ActionResult<IEnumerable<BankAccount>>> GetClientBanks(int id)
        {
            var banks = await _context.ClientBankMappings
                .Where(m => m.ClientId == id && m.IsActive)
                .Include(m => m.BankAccount)
                .Select(m => m.BankAccount)
                .ToListAsync();

            return banks;
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<IActionResult> PutClient(int id, Client client)
        {
            if (id != client.ClientId)
                return BadRequest();

            client.ModifiedDate = DateTime.Now;
            _context.Entry(client).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<ActionResult<Client>> PostClient(Client client)
        {
            client.CreatedDate = DateTime.Now;
            client.ModifiedDate = DateTime.Now;
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClient), new { id = client.ClientId }, client);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,InvoiceCreator")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null)
                return NotFound();

            client.IsActive = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

// Continue in next file with InvoicesController and BankAccountsController...
