namespace Core.Dtos.DayStatistics;

public class TeamStatisticDto
{
	public Guid TeamId { get; init; }
	
	public IReadOnlyList<DayStatisticDto> DayStatistics { get; init; }

	public int TotalProfitGained => DayStatistics.Select(t => t.ProfitGained).Sum() - Penalty + BonusProfit;
	public int TotalClientsGained => DayStatistics.Select(t => t.ClientsGained).Sum();
	
	public int Penalty { get; init; }
	public int BonusProfit { get; init; }
}