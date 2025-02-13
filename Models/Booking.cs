using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Booking
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }
    public Client Client { get; set; }

    [ForeignKey("Stylist")]
    public int StylistId { get; set; }
    public Stylist Stylist { get; set; }

    public DateTime AppointmentDate { get; set; }
    public string? Status { get; set; } = "Pending"; // "Confirmed", "Cancelled"
}
