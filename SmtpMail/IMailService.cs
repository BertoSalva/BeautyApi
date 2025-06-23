using WebApplication1.RequestModels;

namespace WebApplication1.SmtpMail
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequest mailRequest);

        Task SendInvoiceEmailAsync(InvoiceEmailRequest mailRequest);
    }
}
