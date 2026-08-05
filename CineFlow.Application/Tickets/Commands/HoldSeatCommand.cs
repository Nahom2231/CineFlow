using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CineFlow.Application.Common.Interfaces;
using CineFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CineFlow.Application.Tickets.Commands;

public record HoldSeatCommand(
    Guid ScheduleId,
    string SeatNumber,
    string UserId,
    int HoldDurationMinutes = 10) : IRequest<Guid>;
    public class HoldSeatCommandHandler :IRequestHandler<HoldSeatCommand, Guid>
{
    private readonly ICineFlowDbContext _context;
    public HoldSeatCommandHandler(ICineFlowDbContext context)
    {
        _context = context;
    }
    public async Task <Guid> Handle (HoldSeatCommand request, CancellationToken cancellationToken)
    {
        var schedule = await _context.Schedules
        .Include(s=>s.Tickets)
        .FirstOrDefaultAsync(s=> s.Id==request.ScheduleId, cancellationToken );

        if(schedule==null)
        throw new Exception (" The requested showtime schedule does not exist.");

        if(schedule.AvailableSeats <= 0)
        throw new Exception("This showtime is completely sold out.");

        var targetSeat = request.SeatNumber.Trim().ToUpper();

        var isBooked = schedule.Tickets.Any(t=>t.SeatNumber.Trim().ToUpper()==targetSeat);
        if(isBooked)
        throw new Exception($"Seat {request.SeatNumber} is already booked");

        var isHeld= await _context.SeatReservations.AnyAsync(r=>
        r.ScheduleId==request.ScheduleId &&
        r.SeatNumber.Trim().ToUpper()==targetSeat&&
        !r.IsCompleted&& r.ExpiredAt> DateTime.UtcNow, cancellationToken);

        if(isHeld)
        throw new Exception($"Seat {request.SeatNumber }is currently held by another user completing payment.");

        var reservation = new SeatReservation
        {
            Id= Guid.NewGuid(),
            ScheduleId= request.ScheduleId,
            SeatNumber = targetSeat,
            UserId= request.UserId,
            ReservedAt= DateTime.UtcNow,

            ExpiredAt=DateTime.UtcNow.AddMinutes(request.HoldDurationMinutes),
            IsCompleted = false
        };
        schedule.AvailableSeats -=1;

        _context.SeatReservations.Add(reservation);
        await _context.SaveChangesAsync(cancellationToken);
        return reservation.Id;

    }
}
