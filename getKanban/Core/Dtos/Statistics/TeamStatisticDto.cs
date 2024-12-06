namespace Core.Dtos.DayStatistics;

public class TeamStatisticDto
{
	public Guid TeamId { get; }

	public IReadOnlyList<DayStatisticDto> DayStatistics { get; }

	public int Penalty { get; }
	public int BonusProfit { get; }

	public int TotalProfitGained => DayStatistics.Select(t => t.ProfitGained).Sum() - Penalty + BonusProfit;
	public int TotalClientsGained => DayStatistics.Select(t => t.ClientsGained).Sum();

	private TeamStatisticDto(
		Guid teamId,
		IReadOnlyList<DayStatisticDto> dayStatistics,
		int penalty,
		int bonusProfit)
	{
		TeamId = teamId;
		DayStatistics = dayStatistics;
		Penalty = penalty;
		BonusProfit = bonusProfit;
	}

	internal static TeamStatisticDto Create(
		Guid teamId,
		IReadOnlyList<DayStatisticDto> dayStatistics,
		int penalty,
		int bonusProfit)
	{
		return new TeamStatisticDto(
			teamId,
			dayStatistics,
			penalty,
			bonusProfit);
	}
}