using Core.Dtos.Statistics;

namespace Core.Dtos.DayStatistics;

public class DayStatisticDto
{
	public int DayNumber { get; }

	public int ProfitGained { get; }

	public int ClientsGained { get; }

	public int ProfitPerClient { get; }

	public CfdStatisticDto CfdStatistic { get; }

	private DayStatisticDto(
		int dayNumber,
		int clientsGained,
		int profitGained,
		int profitPerClient,
		CfdStatisticDto cfdStatistic)
	{
		DayNumber = dayNumber;
		ClientsGained = clientsGained;
		ProfitGained = profitGained;
		ProfitPerClient = profitPerClient;
		CfdStatistic = cfdStatistic;
	}

	public static DayStatisticDto Create(
		int dayNumber,
		int clientsGained,
		int profitGained,
		int profitPerClient,
		CfdStatisticDto cfdStatistic)
	{
		return new DayStatisticDto(
			dayNumber,
			clientsGained,
			profitGained,
			profitPerClient,
			cfdStatistic);
	}
}