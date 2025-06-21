namespace WebApplication1.RequestModels
{
    public class InvoiceEmailRequest
    {
        public int InvoiceId { get; set; }  
        public string? InvoiceNumber { get; set; }
        public string? RecipientEmail { get; set; }
        public string? RecipientName { get; set; }
        public string? DueDate { get; set; }
        public string? Total { get; set; }
        public string? Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public List<AttachmentFile> Attachments { get; set; } = new();
    }

    public class AttachmentFile
    {
        public byte[] Content { get; set; }
        public string? FileName { get; set; }
        public string? ContentType { get; set; }
    }
}
