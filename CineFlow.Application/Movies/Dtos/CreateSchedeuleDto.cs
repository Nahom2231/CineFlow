using System;
using CineFlow.Domain.Entities;

namespace CineFlow.Application.Schedules.Commands;

public class CreateScheduleDto
{
    public Guid MovieId { get; set; }
    public Guid CinemaHallId { get; set; }
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
}
