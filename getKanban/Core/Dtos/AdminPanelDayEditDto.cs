namespace Core.Dtos;

public class AdminPanelDayEditDto
{
	public string TeamName { get; set; } = null!;
	public int DayNumber { get; set; }
	public List<string> SubmittedTickets { get; set; } = null!;
	public List<string> TakenTickets { get; set; } = null!;
}