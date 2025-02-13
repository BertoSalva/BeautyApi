using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Transaction
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Stylist")]
    public int StylistId { get; set; }
    public Stylist Stylist { get; set; }

    public decimal? Amount { get; set; }
    public DateTime? Date { get; set; }
    public string? Status { get; set; } // "Completed", "Pending", "Failed"
}
