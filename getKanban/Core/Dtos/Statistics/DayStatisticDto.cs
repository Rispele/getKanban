using Core.Dtos.Statistics;
using Domain.Game;

namespace Core.Dtos.DayStatistics;

public class DayStatisticDto
{
	public int DayNumber { get; }

	public int ProfitGained { get; }

	public int ClientsGained { get; }

	public int ProfitPerClient { get; }

	public CfdStatisticDto CfdStatistic { get; }

	public IReadOnlyList<Ticket> TicketsReleased { get; }

	private DayStatisticDto(
		int dayNumber,
		int clientsGained,
		int profitGained,
		int profitPerClient,
		CfdStatisticDto cfdStatistic,
		IReadOnlyList<Ticket> ticketsReleased)
	{
		DayNumber = dayNumber;
		ClientsGained = clientsGained;
		ProfitGained = profitGained;
		ProfitPerClient = profitPerClient;
		CfdStatistic = cfdStatistic;
		TicketsReleased = ticketsReleased;
	}

	public static DayStatisticDto Create(
		int dayNumber,
		int clientsGained,
		int profitGained,
		int profitPerClient,
		CfdStatisticDto cfdStatistic,
		IReadOnlyList<Ticket> ticketsReleased)
	{
		return new DayStatisticDto(
			dayNumber,
			clientsGained,
			profitGained,
			profitPerClient,
			cfdStatistic,
			ticketsReleased);
	}
}