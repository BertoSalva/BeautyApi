using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly BeautyShopDbContext _dbContext;
        private readonly IConfiguration _config;
        private readonly LocalFileStorageService _fileStorageService;

        public AuthController(BeautyShopDbContext dbContext, IConfiguration config, LocalFileStorageService fileStorageService)
        {
            _dbContext = dbContext;
            _config = config;
            _fileStorageService = fileStorageService;
        }


        // ✅ REGISTER USER WITH EXTENDED ATTRIBUTES AND BLOB SUPPORT
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterRequest request)
        {
            if (_dbContext.Users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already exists." });
            }

            // Use the URL provided or upload the file if one was submitted.
            string profilePictureUrl = request.ProfilePictureUrl;
            if (request.ProfilePictureFile != null && request.ProfilePictureFile.Length > 0)
            {
                profilePictureUrl = await _fileStorageService.UploadFileAsync(request.ProfilePictureFile);
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = HashPassword(request.Password),
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                ProfilePictureUrl = profilePictureUrl,
                LanguagePreference = request.LanguagePreference,
                Location = request.Location,
                AccountStatus = "Active", // Default status
                Role = string.IsNullOrEmpty(request.Role) ? "Client" : request.Role,
                DateCreated = DateTime.UtcNow,

                // Extended stylist properties (optional)
                Bio = request.Bio,
                YearsOfExperience = request.YearsOfExperience,
                PortfolioUrl = request.PortfolioUrl,
                AvailableWorkingHours = request.AvailableWorkingHours,
                TravelRadius = request.TravelRadius,
                BusinessLocation = request.BusinessLocation,
                CancellationPolicy = request.CancellationPolicy,
                PaymentDetails = request.PaymentDetails,
                Certifications = request.Certifications
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
            // Fallback URL if no file is provided.
            public string? ProfilePictureUrl { get; set; }
            // File upload for profile picture
            public IFormFile? ProfilePictureFile { get; set; }
            public string? LanguagePreference { get; set; }
            public string? Location { get; set; }
            public string? Role { get; set; }

            // Extended stylist properties (optional)
            public string? Bio { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? PortfolioUrl { get; set; }
            public string? AvailableWorkingHours { get; set; }
            public decimal? TravelRadius { get; set; }
            public string? BusinessLocation { get; set; }
            public string? CancellationPolicy { get; set; }
            public string? PaymentDetails { get; set; }
            public string? Certifications { get; set; }
        }

        // ✅ LOGIN ENDPOINT (unchanged)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (user == null || user.PasswordHash != HashPassword(request.Password))
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                string token = GenerateJwtToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Login error: " + ex.Message);
                return StatusCode(500, new { message = "An error occurred during login." });
            }
        }

        // DTO for Login Request
        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        // ✅ UPDATE USER DETAILS WITH OPTIONAL EXTENDED ATTRIBUTES AND BLOB UPLOAD
        [HttpPut("update/{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateUser(int id, [FromForm] UpdateUserDto updatedUser)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Update base user fields
            user.FullName = !string.IsNullOrEmpty(updatedUser.FullName) ? updatedUser.FullName : user.FullName;
            user.PhoneNumber = !string.IsNullOrEmpty(updatedUser.PhoneNumber) ? updatedUser.PhoneNumber : user.PhoneNumber;

            // Update profile picture: if a file is provided, upload it; otherwise, use the provided URL if any.
            if (updatedUser.ProfilePictureFile != null && updatedUser.ProfilePictureFile.Length > 0)
            {
                user.ProfilePictureUrl = await _fileStorageService.UploadFileAsync(updatedUser.ProfilePictureFile);
            }
            else if (!string.IsNullOrEmpty(updatedUser.ProfilePictureUrl))
            {
                user.ProfilePictureUrl = updatedUser.ProfilePictureUrl;
            }

            user.LanguagePreference = !string.IsNullOrEmpty(updatedUser.LanguagePreference) ? updatedUser.LanguagePreference : user.LanguagePreference;
            user.Location = !string.IsNullOrEmpty(updatedUser.Location) ? updatedUser.Location : user.Location;
            user.AccountStatus = !string.IsNullOrEmpty(updatedUser.AccountStatus) ? updatedUser.AccountStatus : user.AccountStatus;

            if (!string.IsNullOrWhiteSpace(updatedUser.Password))
            {
                user.PasswordHash = HashPassword(updatedUser.Password);
            }

            // Update extended stylist fields if provided
            user.Bio = !string.IsNullOrEmpty(updatedUser.Bio) ? updatedUser.Bio : user.Bio;
            user.YearsOfExperience = updatedUser.YearsOfExperience.HasValue ? updatedUser.YearsOfExperience : user.YearsOfExperience;
            user.PortfolioUrl = !string.IsNullOrEmpty(updatedUser.PortfolioUrl) ? updatedUser.PortfolioUrl : user.PortfolioUrl;
            user.AvailableWorkingHours = !string.IsNullOrEmpty(updatedUser.AvailableWorkingHours) ? updatedUser.AvailableWorkingHours : user.AvailableWorkingHours;
            user.TravelRadius = updatedUser.TravelRadius.HasValue ? updatedUser.TravelRadius : user.TravelRadius;
            user.BusinessLocation = !string.IsNullOrEmpty(updatedUser.BusinessLocation) ? updatedUser.BusinessLocation : user.BusinessLocation;
            user.CancellationPolicy = !string.IsNullOrEmpty(updatedUser.CancellationPolicy) ? updatedUser.CancellationPolicy : user.CancellationPolicy;
            user.PaymentDetails = !string.IsNullOrEmpty(updatedUser.PaymentDetails) ? updatedUser.PaymentDetails : user.PaymentDetails;
            user.Certifications = !string.IsNullOrEmpty(updatedUser.Certifications) ? updatedUser.Certifications : user.Certifications;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "User updated successfully." });
        }

        // DTO for Updating User Details
        public class UpdateUserDto
        {
            public string? FullName { get; set; }
            public string? PhoneNumber { get; set; }
            public string? ProfilePictureUrl { get; set; }
            public IFormFile? ProfilePictureFile { get; set; }
            public string? AccountStatus { get; set; }
            public string? LanguagePreference { get; set; }
            public string? Location { get; set; }
            public string? Password { get; set; } // Optional; update only if provided

            // Extended stylist properties (optional)
            public string? Bio { get; set; }
            public int? YearsOfExperience { get; set; }
            public string? PortfolioUrl { get; set; }
            public string? AvailableWorkingHours { get; set; }
            public decimal? TravelRadius { get; set; }
            public string? BusinessLocation { get; set; }
            public string? CancellationPolicy { get; set; }
            public string? PaymentDetails { get; set; }
            public string? Certifications { get; set; }
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

        // ✅ GET USER BY ID (including extended attributes)
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    ProfilePictureUrl = u.ProfilePictureUrl,

                    u.LanguagePreference,
                    u.Location,
                    u.Role,
                    u.AccountStatus,
                    // Extended stylist properties
                    u.Bio,
                    u.YearsOfExperience,
                    u.PortfolioUrl,
                    u.AvailableWorkingHours,
                    u.TravelRadius,
                    u.BusinessLocation,
                    u.CancellationPolicy,
                    u.PaymentDetails,
                    u.Certifications
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            return Ok(user);
        }

        // ✅ GET USERS BY ROLE (including extended attributes)
        [HttpGet("users/role/{role}")]
        public async Task<IActionResult> GetUsersByRole(string role)
        {
            var users = await _dbContext.Users
                .Where(u => u.Role.ToLower() == role.ToLower())
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.ProfilePictureUrl,
                    u.LanguagePreference,
                    u.Location,
                    u.Role,
                    u.AccountStatus,
                    // Extended stylist properties
                    u.Bio,
                    u.YearsOfExperience,
                    u.PortfolioUrl,
                    u.AvailableWorkingHours,
                    u.TravelRadius,
                    u.BusinessLocation,
                    u.CancellationPolicy,
                    u.PaymentDetails,
                    u.Certifications
                })
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound(new { message = "No users found with the specified role." });
            }
            return Ok(users);
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

    // DTO for Login Request if defined outside of the controller (optional)
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
