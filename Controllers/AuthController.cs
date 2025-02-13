using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;

namespace WebApplication1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BeautyShopDbContext _dbContext;
        private readonly IConfiguration _config;

        public AuthController(BeautyShopDbContext dbContext, IConfiguration config)
        {
            _dbContext = dbContext;
            _config = config;
        }

        // ✅ REGISTER USER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (_dbContext.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                ProfilePictureUrl = request.ProfilePictureUrl,
                LanguagePreference = request.LanguagePreference,
                Location = request.Location,
                AccountStatus = "Active", // Default status
                Role = request.Role ?? "Client", // Default role
                DateCreated = DateTime.UtcNow
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User registered successfully." });
        }

        // DTO for Register Request
        public class RegisterRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string? FullName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? ProfilePictureUrl { get; set; }
            public string? LanguagePreference { get; set; }
            public string? Location { get; set; }
            public string? Role { get; set; } // Optional
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != HashPassword(request.Password))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            string token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // DTO for login request
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // ✅ UPDATE USER DETAILS
        [Authorize] // Requires authentication
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User updatedUser)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Only update non-null fields
            user.FullName = updatedUser.FullName ?? user.FullName;
            user.PhoneNumber = updatedUser.PhoneNumber ?? user.PhoneNumber;
            user.ProfilePictureUrl = updatedUser.ProfilePictureUrl ?? user.ProfilePictureUrl;
            user.LanguagePreference = updatedUser.LanguagePreference ?? user.LanguagePreference;
            user.Location = updatedUser.Location ?? user.Location;
            user.AccountStatus = updatedUser.AccountStatus ?? user.AccountStatus;

            // Hash password only if it's changed
            if (!string.IsNullOrWhiteSpace(updatedUser.PasswordHash))
            {
                user.PasswordHash = HashPassword(updatedUser.PasswordHash);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "User updated successfully." });
        }

        // ✅ GET LOGGED-IN USER DETAILS (Using JWT)
        [Authorize]
        [HttpGet("user")]
        public async Task<IActionResult> GetUserByToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized(new { message = "Invalid token" });

            var user = await _dbContext.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(user);
        }

        // ✅ DELETE USER BY ID
        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }

        // ✅ GET USER BY ID
        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(user);
        }

        // 🔒 Hash Password using SHA256
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // 🔐 Generate JWT Token
        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:SecretKey"]);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
