using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.DTOModels
{
    public class InvoiceDTO
    {
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string Description { get; set; } = null!;
        public bool IsPaid { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = null!;

        [Required]
        public ICollection<InvoiceItem> InvoiceItems { get; set; } = null!;
    }
}
