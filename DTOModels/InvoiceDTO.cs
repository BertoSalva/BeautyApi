using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;

namespace WebApplication1.DTOModels
{
    public class InvoiceDTO
    {
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string? Description { get; set; }
        public bool IsPaid { get; set; }
        public string? InvoiceNumber { get; set; }

        [Required]
        public int UserId { get; set; }
        [Required]
        public int VendorId { get; set; }

        [Required]
        public ICollection<InvoiceItemDTO> InvoiceItems { get; set; } = null!;
    }
}
