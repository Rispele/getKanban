namespace Core.Dtos.DayStatistics;

public class TeamStatisticDto
{
	public Guid TeamId { get; }

	public IReadOnlyList<DayStatisticDto> DayStatistics { get; }

	public int ClientsPenalty { get; }
	public int Penalty { get; }
	public int BonusProfit { get; }

	public int TotalProfitGained => DayStatistics.Select(t => t.ProfitGained).Sum() - Penalty + BonusProfit;
	public int TotalClientsGained => DayStatistics.Select(t => t.ClientsGained).Sum() - ClientsPenalty;

	private TeamStatisticDto(
		Guid teamId,
		IReadOnlyList<DayStatisticDto> dayStatistics,
		int clientsPenalty,
		int penalty,
		int bonusProfit)
	{
		TeamId = teamId;
		DayStatistics = dayStatistics;
		ClientsPenalty = clientsPenalty;
		Penalty = penalty;
		BonusProfit = bonusProfit;
	}

	internal static TeamStatisticDto Create(
		Guid teamId,
		IReadOnlyList<DayStatisticDto> dayStatistics,
		int penalty,
		int clientsPenalty,
		int bonusProfit)
	{
		return new TeamStatisticDto(
			teamId,
			dayStatistics,
			clientsPenalty,
			penalty,
			bonusProfit);
	}
}