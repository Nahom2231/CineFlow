namespace CineFlow.Domain.Entities;

public class Schedule
{
    public Guid Id { get; set;}
    public DateTime ShowTime { get; set; }
    public string CinemaBranch {get; set;} =string.Empty;

    public string HallName {get; set; } =string.Empty;
    public decimal TicketPrice { get; set; }
    public int AvailableSeats { get; set; }

    public Guid MovieId { get; set; }
    public Movie? Movie { get; set; } 

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}