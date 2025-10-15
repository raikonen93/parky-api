using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parky.Application.Dtos;
using Parky.Domain.Entities;
using Parky.Infrastructure.Context;

namespace Parky.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Driver")]
    public class BookingsController : ControllerBase
    {
        private readonly ParkyDbContext _context;
        private readonly IMapper _mapper;

        public BookingsController(ParkyDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: /api/bookings?lotId=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings([FromQuery] int? lotId)
        {
            IQueryable<Booking> query = _context.Bookings.Include(t => t.Lot).AsNoTracking();

            if (lotId.HasValue)
                query = query.Where(b => b.LotId == lotId.Value);

            return await query.ToListAsync();
        }

        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
            {
                return NotFound();
            }

            await _context.Entry(booking)
                  .Reference(b => b.Lot)
                  .LoadAsync();

            return booking;
        }

        // PUT: api/Bookings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(int id, BookingDto booking)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existingBooking = await _context.Bookings.FindAsync(id);

            if (existingBooking == null)
                return NotFound($"Booking with Id={id} not found.");
            bool overlaps = await _context.Bookings
                                .AnyAsync(b =>
                                    b.Id != id &&
                                    b.LotId == booking.LotId &&
                                    b.Status == "Active" &&
                                    booking.From < b.To && booking.To > b.From);

            if (overlaps)
                return Conflict("This time slot is already booked.");

            _mapper.Map(booking, existingBooking);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("This record was modified by another user. Please reload the booking and try again.");
            }

            return Ok(existingBooking);
        }

        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(BookingDto booking)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Start transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Check for overlaps for the same LotId
                bool overlaps = await _context.Bookings
                    .AnyAsync(b =>
                        b.LotId == booking.LotId &&
                        b.Status == "Active" && // consider only active bookings
                        booking.From < b.To && booking.To > b.From);

                if (overlaps)
                {
                    return Conflict("This parking lot is already booked for the selected time range.");
                }

                var bookingToAdd = _mapper.Map<Booking>(booking);

                _context.Bookings.Add(bookingToAdd);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return CreatedAtAction(nameof(GetBooking), new { id = bookingToAdd.Id }, booking);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{id}/cancel")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);

            if (booking == null)
                return NotFound($"Booking with Id={id} not found.");

            if (booking.Status == "Cancelled")
                return BadRequest("Booking is already cancelled.");

            booking.Status = "Cancelled";

            try
            {
                await _context.SaveChangesAsync();
                return Ok(booking);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("This record was modified by another user. Please reload and try again.");
            }
        }
    }
}
