namespace Core.Dtos.DayStatistics;

public class DayStatisticDto
{
	public int DayNumber { get; init; }
	
	public int ProfitGained { get; init; }
	
	public int ClientsGained { get; init; }
	
	public int ProfitPerClient { get; init; }
}