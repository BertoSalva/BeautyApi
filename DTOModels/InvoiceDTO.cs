namespace WebApplication1.DTOModels
{
    public class InvoiceDTO
    {
        public DateTime InvoiceDate { get; set; }
        public decimal Total { get; set; }
        public string Description { get; set; }
        public bool IsPaid { get; set; }
    }
}
