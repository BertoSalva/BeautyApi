using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DTOModels;
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
            if (_db is null)
            {
                return NotFound(new { message = "Faild to find Database to execute" });
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
                    x.UserId,
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
                return BadRequest(new { message = "There was an errors while trying to fetch invoices. Error: ** " + e });
            }
        }


        [HttpGet("getInvoice/{Id}")]
        public async Task<IActionResult> GetInvoiceById(int Id)
        {
            if (_db is null)
            {
                return NotFound(new { message = "Faild to find Database to execute" });
            }

            try
            {
                var Invoice = await _db.Invoices
                  .Where(x => x.Id == Id)
                  .Include(x => x.InvoiceItems)
                  .SingleOrDefaultAsync(x => x.Id == Id);


                if (Invoice == null)
                {
                    return NotFound(new { message = $"Invoice with Id {Id} not found." });
                }

                return Ok(new
                {
                    Invoice = new
                    {
                        Invoice.Id,
                        Invoice.InvoiceNumber,
                        Invoice.InvoiceDate,
                        Invoice.Total,
                        Invoice.Description,
                        Invoice.IsPaid,
                        Invoice.UserId,
                        Items = Invoice.InvoiceItems.Select(item => new
                        {
                            item.Id,
                            item.Name,
                            item.Quantity,
                            item.Price,
                            item.Description,
                            item.CreatedDate
                        }),
                    }
                });
            }
            catch (Exception e)
            {
                return BadRequest(new { message = "There was an errors while trying to fetch invoices. Error: ** " + e });
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
                if (addItems is OkObjectResult)
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

            return Ok(new { message = "Invoice items added successfully." });
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

        //Update Statement
        [HttpPut("updateInvoice/{id}")]
        public async Task<IActionResult> UpdateInvoice(int id, [FromBody] InvoiceDTO request)
        {
            var invoice = await _db.Invoices.Where(x => x.Id == id).FirstOrDefaultAsync();

            if (invoice == null)
                return NotFound(new { message = "Invoice not found." });

            invoice.InvoiceDate = request.InvoiceDate;
            invoice.Total = request.Total;
            invoice.Description = request.Description;
            invoice.IsPaid = request.IsPaid;

            try
            {
                _db.Invoices.Update(invoice);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Invoice updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update invoice.", error = ex.Message });
            }
        }

        [HttpPut("updateInvoiceItem/{id}")]
        public async Task<IActionResult> UpdateInvoiceItem(int id, [FromBody] InvoiceItem request)
        {
            var item = await _db.InvoiceItem.FindAsync(id);
            if (item == null)
                return NotFound(new { message = "Invoice item not found." });

            item.Name = request.Name;
            item.Quantity = request.Quantity;
            item.Price = request.Price;
            item.Description = request.Description;

            try
            {
                _db.InvoiceItem.Update(item);
                await _db.SaveChangesAsync();
                return Ok(new { message = "Invoice item updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to update invoice item.", error = ex.Message });
            }
        }

        [HttpDelete("deleteInvoice/{id}")]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _db.Invoices
                .Include(i => i.InvoiceItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
                return NotFound(new { message = "Invoice not found." });

            try
            {
                // Manually remove related invoice items if cascade delete is not configured
                if (invoice.InvoiceItems != null && invoice.InvoiceItems.Any())
                {
                    _db.InvoiceItem.RemoveRange(invoice.InvoiceItems);
                }

                _db.Invoices.Remove(invoice);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Invoice and related items deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Failed to delete invoice.", error = ex.Message });
            }
        }

    }
}
