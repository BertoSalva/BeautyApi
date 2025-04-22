using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Booking
{
    public int Id { get; set; }
    public int ClientId { get; set; }          // FK to User
    public int StylistId { get; set; }         // FK to User (barber, nail tech, etc.)
    public string Service { get; set; }
    public DateTime Time { get; set; }
    public string Status { get; set; } = "Pending";

    public User Client { get; set; }
    public User Stylist { get; set; }
    public int? Rating { get; set; } // ✅ NEW: Rating out of 5

}
