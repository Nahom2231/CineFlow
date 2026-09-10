using System;

namespace CineFlow.Application.Schedules.Dtos;

public class UpdateScheduleDto
{
    public Guid MovieId { get; set; }
    public Guid CinemaHallId { get; set; }
    public DateTime Showtime { get; set; }
    public decimal TicketPrice { get; set; }
}
