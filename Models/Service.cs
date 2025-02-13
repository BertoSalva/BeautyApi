using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Service
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Stylist")]
    public int StylistId { get; set; }
    public Stylist Stylist { get; set; }

    public string? Name { get; set; }
    public decimal? Price { get; set; }
}
