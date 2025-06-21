using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace WebApplication1.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = null!;

        [Required]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        public decimal Total { get; set; }

        public string? Description { get; set; }

        public bool IsPaid { get; set; }

        [Required]
        public int VendorId { get; set; }

        //FK Relationships
        [Required]
        public int UserId { get; set; }

        public User? User { get; set; }

        public ICollection<InvoiceItem>? InvoiceItems { get; set; }
    }
}
