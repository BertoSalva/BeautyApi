using WebApplication1.RequestModels;

namespace WebApplication1.SmtpMail
{
    public class MailRequest
    {
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty; // HTML
        public List<AttachmentFile>? Attachments { get; set; } // Optional
    }
}
