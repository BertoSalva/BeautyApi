using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/Invoices")]
    [ApiController]

    public class InvoiceController : Controller
    {
        private readonly BeautyShopDbContext _db;
        public InvoiceController(BeautyShopDbContext db)
        {
            _db = db;
        }

        [HttpGet("getVendorInvoices/{vendorUserId}")]
        public async Task<IActionResult> GetInvoicesByVendo(int vendorUserId)
        {
            if(_db is null)
            {
                return NotFound( new {message = "Faild to find Database to execute"});
            }

            try
            {
                var Invoices = await _db.Invoices
                  .Where(x => x.UserId == vendorUserId)
                  .Include(x => x.InvoiceItems)
                  .ToListAsync();

                return Ok(Invoices.Select(x => new
                {
                    x.Id,
                    x.InvoiceNumber,
                    x.InvoiceDate,
                    x.Total,
                    x.Description,
                    x.IsPaid,
                    Items = x.InvoiceItems.Select(item => new
                    {
                        item.Id,
                        item.Name,
                        item.Quantity,
                        item.Price,
                        item.Description,
                        item.CreatedDate
                    })
                }));
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "There was an errors while trying to fetch invoices. Error: ** " + e});
            }
          
        }

        [HttpPost("addInvoice")]
        public async Task<IActionResult> AddInvoice([FromBody] Invoice request)
        {
            var vendor = await _db.Users.FindAsync(request.UserId);

            if (vendor == null)
                return BadRequest(new { message = "Vendor or customer not found." });

            var invoice = new Invoice
            {
                UserId = request.UserId,
                InvoiceNumber = request.InvoiceNumber,
                InvoiceDate = request.InvoiceDate,
                Total = request.Total,
                Description = request.Description,
                IsPaid = false
            };

            try
            {
                var addItems = await AddInvoiceItems(request);
                if(addItems is OkObjectResult)
                {
                    try
                    {
                        _db.Invoices.Add(invoice);
                        await _db.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        return Ok(new { message = "Failed to save invoice.", InvoiceId = invoice.Id });
                    }
                }
            }
            catch (Exception)
            {
                return Ok(new { message = "Failed to create invoice.", InvoiceId = invoice.Id });
            }

            return Ok(new { message = "Invoice created successfully.", InvoiceId = invoice.Id });
        }

        [HttpPost("addInvoiceItemsRelated")]
        public async Task<IActionResult> AddInvoiceItems([FromBody] Invoice request)
        {
            var invoice = await _db.Invoices.FindAsync(request.Id);

            if (invoice == null)
                return NotFound(new { message = "Invoice not found." });

            var items = request.InvoiceItems.Select(item => new InvoiceItem
            {
                InvoiceId = request.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                Price = item.Price,
                Description = item.Description,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            _db.InvoiceItem.AddRange(items);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Invoice items added successfully."});
        }


        [HttpPost("addInvoiceItem")]
        public async Task<IActionResult> AddInvoiceItems([FromBody] InvoiceItem request)
        {
            var invoice = await _db.Invoices.FindAsync(request.InvoiceId);

            if (invoice == null)
                return NotFound(new { message = "Invoice not found." });

            var item = new InvoiceItem
            {
                InvoiceId = request.InvoiceId,
                Name = request.Name,
                Quantity = request.Quantity,
                Price = request.Price,
                Description = request.Description,
                CreatedDate = DateTime.UtcNow
            };

            _db.InvoiceItem.Add(item);

            await _db.SaveChangesAsync();

            return Ok(new { message = "Invoice items added successfully." });
        }

    }
}
