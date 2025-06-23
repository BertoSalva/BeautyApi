using System.ComponentModel.DataAnnotations;

namespace WebApplication1.DTOModels
{
    public class InvoiceItemDTO
    {
        public int InvoiceId { get; set; }
        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? Description { get; set; }
}
}
