using System;
namespace CineFlow.Domain.Entities;

public class SeatReservation
{
    public Guid Id {get; set;}
    public Guid ScheduleId {get; set;}
    public Schedule Schedule {get; set;} = null!;
    
    public string SeatNumber {get; set;}= string.Empty;
    public string UserId {get; set;}= string.Empty;
    public DateTime ReservedAt {get; set;} =DateTime.UtcNow;
    public DateTime ExpiredAt {get; set;}

    public bool IsCompleted { get; set;}= false;
}