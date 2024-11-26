namespace Core.Dtos;

public class AdminPanelDaysDto
{
	public string TeamName { get; set; } = null!;
	
	public List<DayDto> Days { get; init; } = null!;
}