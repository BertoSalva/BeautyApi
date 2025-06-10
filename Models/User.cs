using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class User
{
    [Key]
    public int Id { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public string? PasswordHash { get; set; }

    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public string? AccountStatus { get; set; } = "Active"; // "Active", "Inactive", "Banned"
    public string? Role { get; set; } // "Client", "Stylist", "Admin"
    public string? LanguagePreference { get; set; }
    public string? Location { get; set; }
    public DateTime? DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime? LastLogin { get; set; }

    public string? Bio { get; set; }
    public int? YearsOfExperience { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? AvailableWorkingHours { get; set; } // e.g., "Mon-Fri 9AM-6PM"
    public decimal? TravelRadius { get; set; }
    public string? BusinessLocation { get; set; }
    public decimal? Earnings { get; set; }
    public decimal? Rating { get; set; } = 4;
    public decimal? Visits { get; set; } = 0;
    public decimal? ServiceCost { get; set; }

    public string? CancellationPolicy { get; set; }
    public string? PaymentDetails { get; set; } // e.g., "Bank Account, PayPal"
    public string? Certifications { get; set; }
}
