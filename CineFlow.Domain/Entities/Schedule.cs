namespace CineFlow.Domain.Entities;

public class Schedule
{
    public Guid Id { get; set;}
    public DateTime Showtime { get; set; }



    public decimal TicketPrice { get; set; }
    public int AvailableSeats { get; set; }
     public uint RowVersion { get; set; }
    public Guid CinemaHallId {get; set;}
    public CinemaHall? CinemaHall {get; set;}

    public Guid MovieId { get; set; }
    public Movie? Movie { get; set; } 

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}