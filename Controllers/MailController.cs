using Microsoft.AspNetCore.Mvc;
using WebApplication1.Mailkit;
using WebApplication1.RequestModels;
using WebApplication1.SmtpMail;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private readonly IWebHostEnvironment _env;
        private readonly BeautyShopDbContext _db;
        public MailController(IConfiguration configuration, IMailService mailService, IWebHostEnvironment env, BeautyShopDbContext db   )
        {
            _mailService = mailService;
            _env = env;
            _configuration = configuration;
            _db = db;
        }

        [HttpPost]
        [Route("sendInvoice/{invoiceId}")]
        public async Task<IActionResult> SendInvoiceEmail(int invoiceId, string invoiceLink)
        {
            var invoice = await _db.Invoices.FindAsync(invoiceId);
            if (invoice == null) 
            {
                return NotFound(new {message = "Failed to find invoice,"});
            }

            var recipient = await _db.Users.FindAsync(invoice.UserId);
            if (recipient == null)
            {
                return NotFound(new { message = "Failed to find client." });
            }

            string templatePath = Path.Combine(_env.ContentRootPath, "SmtpMail/EmailTemplates/Invoice.html");

            var tokens = new Dictionary<string, string>
            {
                { "InvoiceNumber", invoice.InvoiceNumber },
                { "Recipient", recipient.FullName },
                { "DueDate", invoice.InvoiceDate.AddDays(10).ToString("dd/MM/yyyy") },
                { "Total", invoice.Total.ToString() },
                { "Year", DateTime.Now.ToString("yyyy")},
                { "InvoiceLink", invoiceLink }
            };

            string body = EmailTemplateHelper.GetTemplate(templatePath, tokens);

            string invoicesFolder = Path.Combine(_env.ContentRootPath, "BeautyShop_Invoices");
            string fileName = $"MyBeautyShop - {invoice.InvoiceNumber}.pdf";
            string fullPath = Path.Combine(invoicesFolder, fileName);

            byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

            var request = new MailRequest
            {
                ToEmail = recipient.Email,
                Subject = "myBeautyShop Invoice Notification",
                Body = body,
                Attachments = new List<AttachmentFile>
                {
                    new AttachmentFile
                    {
                        Content = fileBytes,
                        FileName = fileName,
                        ContentType = "application/pdf"
                    }
                }
            };

            try
            {
                await _mailService.SendEmailAsync(request);
            }
            catch (Exception ex)
            {           
                return StatusCode(500, new { message = "Failed to send email." });
            }
            return Ok(new { message = "Invoice email sent." });
        }
    }
}
