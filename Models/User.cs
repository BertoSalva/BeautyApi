using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; }

    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccountStatus { get; set; } = "Active"; // "Active", "Inactive", "Banned"
    public string? Role { get; set; } // "Client", "Stylist", "Admin"
    public string? LanguagePreference { get; set; }
    public string? Location { get; set; }
    public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }
}
