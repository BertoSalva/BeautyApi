using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class InvoiceItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }     

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int InvoiceId { get; set; }

        public Invoice Invoice { get; set; }
    }
}
