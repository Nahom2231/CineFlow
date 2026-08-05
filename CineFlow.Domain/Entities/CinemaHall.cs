namespace CineFlow.Domain.Entities;

public class CinemaHall
{
public Guid Id {get; set;}
public string BranchName {get; set;}= string.Empty;
public string HallName {get; set; }=string.Empty;
public int TotalCapacity {get; set;}

public string  SeatMapMatrixJson {get; set; }=string.Empty;

public ICollection<Schedule> Schedule {get; set;}= new List<Schedule>();
}