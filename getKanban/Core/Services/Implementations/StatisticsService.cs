using Core.DbContexts.Extensions;
using Core.Dtos.DayStatistics;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Teams;
using Domain.Game.Tickets;

namespace Core.Services.Implementations;

public class StatisticsService : IStatisticsService
{
	private readonly DomainContext context;

	public StatisticsService(DomainContext context)
	{
		this.context = context;
	}

	public async Task<TeamStatisticDto> EvaluateProfitsAsync(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);

		var takenTickets = team.BuildTakenTickets();
		var clientsGainedPerDay = EvaluateClientsGainedPerDay(takenTickets, team.CurrentDay.Number);
		var profitPerClientPerDay = team.Days.ToDictionary(day => day.Number, day => day.DaySettings.ProfitPerClient);

		var firstDayNumber = team.Days.Select(d => d.Number).Min();
		var currentDayNumber = team.CurrentDay.Number;
		var clientsPerDay = EvaluateClientPerDay(firstDayNumber, currentDayNumber, clientsGainedPerDay);
		var profitGainedPerDay = EvaluateProfitGainedPerDay(profitPerClientPerDay, clientsPerDay);

		var dayStats = ConvertToDayStatisticDto(
			firstDayNumber,
			currentDayNumber,
			clientsGainedPerDay,
			profitGainedPerDay,
			profitPerClientPerDay);
		
		return new TeamStatisticDto
		{
			TeamId = teamId,
			DayStatistics = dayStats,
			Penalty = EvaluatePenalty(team),
			BonusProfit = EvaluateBonusProfit(team)
		};
	}
	

	private static List<DayStatisticDto> ConvertToDayStatisticDto(
		int firstDayNumber,
		int currentDayNumber,
		Dictionary<int, int> clientsGainedPerDay,
		Dictionary<int, int> profitGainedPerDay,
		Dictionary<int, int> profitPerClientPerDay)
	{
		return Enumerable.Range(firstDayNumber, currentDayNumber - firstDayNumber + 1)
			.Select(
				dayNumber => new DayStatisticDto
				{
					DayNumber = dayNumber,
					ClientsGained = clientsGainedPerDay[dayNumber],
					ProfitGained = profitGainedPerDay[dayNumber],
					ProfitPerClient = profitPerClientPerDay[dayNumber]
				})
			.ToList();
	}

	private int EvaluatePenalty(Team team)
	{
		return TicketDescriptors.AllTicketDescriptors
			.Where(t => t.Penalty != null)
			.Where(t => !team.IsTicketDeadlineNotExceeded(t.Id, t.Penalty!.Deadline))
			.Sum(t => t.Penalty!.Size);
	}

	private int EvaluateBonusProfit(Team team)
	{
		return TicketDescriptors.AllTicketDescriptors
			.Where(t => t.Bonus != null)
			.Where(t => team.IsTicketDeadlineNotExceeded(t.Id, t.Bonus!.Deadline))
			.Sum(t => t.Bonus!.Size);
	}

	private Dictionary<int, int> EvaluateProfitGainedPerDay(
		Dictionary<int, int> profitPerClientPerDay,
		Dictionary<int, int> clientsPerDay)
	{
		return clientsPerDay
			.Select(kv => (dayNumber: kv.Key, profit: kv.Value * profitPerClientPerDay[kv.Key]))
			.ToDictionary(tuple => tuple.dayNumber, tuple => tuple.profit);
	}

	private Dictionary<int, int> EvaluateClientPerDay(
		int firstDayNumber,
		int currentDayNumber,
		Dictionary<int, int> clientsGainedPerDay)
	{
		var currentClients = 0;
		var clientsPerDay = new Dictionary<int, int>();
		for (var i = firstDayNumber; i <= currentDayNumber; i++)
		{
			if (clientsGainedPerDay.TryGetValue(i, out var clients))
			{
				currentClients += clients;
			}

			clientsPerDay[i] = currentClients;
		}

		return clientsPerDay;
	}

	private static Dictionary<int, int> EvaluateClientsGainedPerDay(HashSet<Ticket> takenTickets, int currentDayNumber)
	{
		var ticketDescriptors = TicketDescriptors.AllTicketDescriptors.ToDictionary(d => d.Id, t => t);

		return takenTickets
			.Where(ticket => ticket.IsReleased(currentDayNumber))
			.OrderBy(ticket => ticket.releaseDay!)
			.Select(ticket => (releaseDay: ticket.releaseDay!.Value, clientsGained: GetClientsGainedByTicket(ticket)))
			.GroupBy(ticket => ticket.releaseDay)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Select(tuple => tuple.clientsGained).Sum());

		int GetClientsGainedByTicket(Ticket ticket)
		{
			var descriptor = ticketDescriptors[ticket.id];
			var clientsLost = descriptor.ClientOffRate * (ticket.releaseDay!.Value - ticket.takingDay);
			return descriptor.ClientsProvides - clientsLost;
		}
	}
}