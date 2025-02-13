using Microsoft.AspNetCore.Mvc.ViewEngines;

public class Client : User
{
    public ICollection<Booking>? BookingHistory { get; set; }
    public ICollection<Stylist>? FavoriteStylists { get; set; }
    public decimal? WalletBalance { get; set; }
    public ICollection<Review>? ReviewsGiven { get; set; }
    public string? PaymentMethods { get; set; } // e.g., "Credit Card, PayPal"
    public string? NotificationPreferences { get; set; } // "Email, Push, SMS"
    public ICollection<ChatMessage>? ChatHistory { get; set; }
    public int? LoyaltyPoints { get; set; }
}
