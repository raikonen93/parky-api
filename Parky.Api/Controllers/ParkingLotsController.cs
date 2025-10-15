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
    [Authorize(Roles = "Owner")]
    public class ParkingLotsController : ControllerBase
    {
        private readonly ParkyDbContext _context;
        private readonly IMapper _mapper;

        public ParkingLotsController(ParkyDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/ParkingLots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParkingLot>>> GetParkingLots()
        {
            return await _context.ParkingLots.AsNoTracking().ToListAsync();
        }

        // GET: api/ParkingLots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ParkingLot>> GetParkingLot(int id)
        {
            var parkingLot = await _context.ParkingLots.FindAsync(id);

            if (parkingLot == null)
            {
                return NotFound();
            }

            return parkingLot;
        }

        // PUT: api/ParkingLots/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutParkingLot(int id, ParkingLotDto parkingLot)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var existingParkingLot = await _context.ParkingLots.FindAsync(id);

            if (existingParkingLot == null)
                return NotFound($"Parking lot with Id={id} not found.");

            _mapper.Map(parkingLot, existingParkingLot);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/ParkingLots
        [HttpPost]
        public async Task<ActionResult<ParkingLot>> PostParkingLot(ParkingLotDto parkingLot)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var itemToadd = _mapper.Map<ParkingLot>(parkingLot);
            itemToadd.CreatedAt = DateTime.UtcNow;
            _context.ParkingLots.Add(itemToadd);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetParkingLot", new { id = itemToadd.Id }, parkingLot);
        }

        // DELETE: api/ParkingLots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteParkingLot(int id)
        {
            var parkingLot = await _context.ParkingLots.FindAsync(id);
            if (parkingLot == null)
            {
                return NotFound();
            }

            _context.ParkingLots.Remove(parkingLot);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
