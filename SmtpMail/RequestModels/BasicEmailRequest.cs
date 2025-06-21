namespace WebApplication1.RequestModels
{
    public class BasicEmailRequest
    {
        public string? RecipientEmail { get; set; }
        public string? RecipientName { get; set; }
        public string? Message { get; set; } = string.Empty;
       
    }
}
