using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Controllers
{
    [Route("api/bookings")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly BeautyShopDbContext _db;
        public BookingsController(BeautyShopDbContext db)
        {
            _db = db;
        }

        // ✅ Client books a stylist (no token required)
        [HttpPost("create")]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto request)
        {
            var client = await _db.Users.FindAsync(request.ClientId);
            var stylist = await _db.Users.FindAsync(request.StylistId);

            if (client == null || stylist == null)
                return BadRequest(new { message = "Client or stylist does not exist." });

            if (string.IsNullOrEmpty(stylist.Role))
                return BadRequest(new { message = "Stylist role not defined." });

            var booking = new Booking
            {
                ClientId = request.ClientId,
                StylistId = request.StylistId,
                Service = request.Service,
                Time = request.Time,
                Status = "Pending",
                ServiceCost = request.ServiceCost
            };

            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Booking created successfully." });
        }

        // ✅ Stylist manually provides their ID to view bookings
        [HttpGet("stylist/{stylistId}")]
        public async Task<IActionResult> GetBookingsForStylist(int stylistId)
        {
            var bookings = await _db.Bookings
                .Where(b => b.StylistId == stylistId)
                .Include(b => b.Client)
                .ToListAsync();

            return Ok(bookings.Select(b => new {
                b.Id,
                b.Service,
                b.Time,
                b.Status,
                ClientName = b.Client.FullName,
                ClientEmail = b.Client.Email,
                b.ServiceCost
            }));
        }
        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = "Booking not found." });

            booking.Status = status;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Status updated to '{status}'." });
        }

        [HttpPut("rate/{id}")]
        public async Task<IActionResult> RateBooking(int id, [FromBody] int rating)
        {
            if (rating < 1 || rating > 5)
                return BadRequest(new { message = "Rating must be between 1 and 5." });

            var booking = await _db.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound(new { message = "Booking not found." });

            booking.Rating = rating;
            await _db.SaveChangesAsync();

            return Ok(new { message = $"Booking rated {rating} stars." });
        }

        // ✅ Client manually provides their ID to view bookings
        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetBookingsForClient(int clientId)
        {
            var bookings = await _db.Bookings
                .Where(b => b.ClientId == clientId)
                .Include(b => b.Stylist)
                .ToListAsync();

            return Ok(bookings.Select(b => new {
                b.Id,
                b.Service,
                b.Time,
                b.Status,
                StylistName = b.Stylist.FullName,
                StylistRole = b.Stylist.Role,
                b.ServiceCost
            }));
        }

        // 📦 DTO
        public class CreateBookingDto
        {
            public int ClientId { get; set; }
            public int StylistId { get; set; }
            public string Service { get; set; }
            public DateTime Time { get; set; }
            public decimal ServiceCost { get; set; }
        }
    }
}
