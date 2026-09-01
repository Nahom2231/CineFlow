using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CinemaHallController : ControllerBase
{
    private readonly ICineFlowDbContext _context;

    public CinemaHallController(ICineFlowDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var halls = await _context.CinemaHalls
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return Ok(halls);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(id, out var guidId))
        {
            var hall = await _context.CinemaHalls
                .AsNoTracking()
                .FirstOrDefaultAsync(h => h.Id == guidId, cancellationToken);

            if (hall != null) return Ok(hall);
        }

        var firstHall = await _context.CinemaHalls.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (firstHall != null) return Ok(firstHall);

        return NotFound(new { Message = "Cinema hall not found" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CinemaHall hall, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(hall.HallName))
        {
            return BadRequest(new { Message = "Hall name is required." });
        }

        hall.Id = Guid.NewGuid();
        _context.CinemaHalls.Add(hall);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { hall.Id, Message = "Cinema hall created successfully!" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(string id, [FromBody] CinemaHall hall, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId)) return NotFound(new { Message = "Cinema hall not found" });
        var existing = await _context.CinemaHalls.FirstOrDefaultAsync(h => h.Id == guidId, cancellationToken);
        if (existing == null) return NotFound(new { Message = "Cinema hall not found" });

        existing.BranchName = hall.BranchName;
        existing.HallName = hall.HallName;
        existing.TotalCapacity = hall.TotalCapacity;
        existing.SeatMapMatrixJson = hall.SeatMapMatrixJson;

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { Message = "Cinema hall updated successfully!" });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(id, out var guidId)) return NotFound(new { Message = "Cinema hall not found" });
        var hall = await _context.CinemaHalls
            .Include(h => h.Schedules)
            .FirstOrDefaultAsync(h => h.Id == guidId, cancellationToken);
        if (hall == null) return NotFound(new { Message = "Cinema hall not found" });

        if (hall.Schedules.Any())
        {
            var scheduleIds = hall.Schedules.Select(s => s.Id).ToList();

            var reservations = await _context.SeatReservations
                .Where(r => scheduleIds.Contains(r.ScheduleId))
                .ToListAsync(cancellationToken);
            if (reservations.Any())
            {
                _context.SeatReservations.RemoveRange(reservations);
            }

            var tickets = await _context.Tickets
                .Where(t => scheduleIds.Contains(t.ScheduleId))
                .ToListAsync(cancellationToken);
            if (tickets.Any())
            {
                _context.Tickets.RemoveRange(tickets);
            }

            _context.Schedules.RemoveRange(hall.Schedules);
        }

        _context.CinemaHalls.Remove(hall);
        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { Message = "Cinema hall deleted successfully!" });
    }
}