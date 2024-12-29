using Core.DbContexts.Extensions;
using Core.Dtos.DayStatistics;
using Core.Dtos.Statistics;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Days;
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

	public async Task<TeamStatisticDto> CollectStatistic(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);

		var takenTickets = team.BuildTakenTickets();
		var currentDayNumber = team.CurrentDay.Number;
		var clientsGainedPerTicket = EvaluateClientsGainedPerTickets(takenTickets, currentDayNumber);
		var clientsGainedPerDay = EvaluateClientsGainedPerDay(clientsGainedPerTicket);
		var profitPerClientPerDay = team.Days.ToDictionary(day => day.Number, day => day.DaySettings.ProfitPerClient);

		var clientsPerDay = EvaluateClientPerDay(team, clientsGainedPerDay);
		var profitGainedPerDay = EvaluateProfitGainedPerDay(profitPerClientPerDay, clientsPerDay);

		var dayStats = ConvertToDayStatisticDto(
			team,
			takenTickets,
			clientsGainedPerDay,
			profitGainedPerDay,
			profitPerClientPerDay);

		var (profitPenalty, clientsPenalty) = EvaluatePenalty(team);

		return TeamStatisticDto.Create(
			team.Id,
			dayStats,
			profitPenalty,
			clientsPenalty,
			EvaluateBonusProfit(takenTickets, team),
			clientsGainedPerTicket);
	}

	public CfdStatisticDto CollectCfdStatistic(Day day)
	{
		var cfdContainer = day.UpdateCfdContainer;

		return CfdStatisticDto.Create(
			cfdContainer.WithAnalysts,
			cfdContainer.WithProgrammers,
			cfdContainer.WithTesters,
			cfdContainer.ToDeploy,
			cfdContainer.Released);
	}

	private List<DayStatisticDto> ConvertToDayStatisticDto(
		Team team,
		HashSet<Ticket> takenTickets,
		Dictionary<int, int> clientsGainedPerDay,
		Dictionary<int, int> profitGainedPerDay,
		Dictionary<int, int> profitPerClientPerDay)
	{
		var releasedTickets = takenTickets
			.Where(t => t.IsReleasedAnyDate())
			.GroupBy(t => t.releaseDay!.Value)
			.ToDictionary(t => t.Key, t => t.ToList());
		return team.Days
			.Select(
				day => DayStatisticDto.Create(
					day.Number,
					clientsGainedPerDay.GetValueOrDefault(day.Number),
					profitGainedPerDay.GetValueOrDefault(day.Number),
					profitPerClientPerDay.GetValueOrDefault(day.Number),
					CollectCfdStatistic(day),
					releasedTickets.GetValueOrDefault(day.Number) ?? []))
			.ToList();
	}

	private (int profitPenalty, int clientsPenalty) EvaluatePenalty(Team team)
	{
		var ticketsPenalty = TicketDescriptors.AllTicketDescriptors
			.Where(t => t.Penalty != null)
			.Where(t => !team.IsTicketDeadlineNotExceededAtReleaseDay(t.Id, t.Penalty!.Deadline - 1))
			.Sum(t => t.Penalty!.Size);

		if (team.IsLastDay)
		{
			var tasksResultedInPenalty = team.BuildTakenTickets()
				.Where(t => t.IsInWork(team.CurrentDay.Number))
				.Count(t => t.takingDay <= team.Settings.DayBeforeInclusiveNotReleasedTasksTakenResultsInPenalty);

			var clientsPenalty = tasksResultedInPenalty * team.Settings.ClientsPenaltyPerTaskNotReleased;

			return (ticketsPenalty + clientsPenalty * team.Settings.ProfitPenaltyPerClient, clientsPenalty);
		}

		return (ticketsPenalty, 0);
	}

	private int EvaluateBonusProfit(HashSet<Ticket> takenTickets, Team team)
	{
		return takenTickets
			.Where(t => t.IsReleased(team.CurrentDay.Number))
			.Select(t => TicketDescriptors.GetByTicketId(t.id))
			.Where(t => t.Bonus != null)
			.Where(t => team.IsTicketDeadlineNotExceededAtReleaseDay(t.Id, t.Bonus!.Deadline))
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

	private Dictionary<int, int> EvaluateClientPerDay(Team team, Dictionary<int, int> clientsGainedPerDay)
	{
		var currentClients = 0;
		var clientsPerDay = new Dictionary<int, int>();
		foreach (var day in team.Days)
		{
			var dayNumber = day.Number;
			if (clientsGainedPerDay.TryGetValue(dayNumber, out var clients))
			{
				currentClients += clients;
			}

			clientsPerDay[dayNumber] = currentClients;
		}

		return clientsPerDay;
	}

	private static Dictionary<int, int> EvaluateClientsGainedPerDay(Dictionary<Ticket, int> clientsGainedPerTicket)
	{
		return clientsGainedPerTicket
			.Select(pair => (releaseDay: pair.Key.releaseDay!.Value, clientsGained: pair.Value))
			.GroupBy(ticket => ticket.releaseDay)
			.ToDictionary(grouping => grouping.Key, grouping => grouping.Select(tuple => tuple.clientsGained).Sum());
	}

	private static Dictionary<Ticket, int> EvaluateClientsGainedPerTickets(
		HashSet<Ticket> takenTickets,
		int currentDayNumber)
	{
		return takenTickets
			.Where(ticket => ticket.IsReleased(currentDayNumber))
			.ToDictionary(t => t, GetClientsGainedByTicket);

		int GetClientsGainedByTicket(Ticket ticket)
		{
			var descriptor = TicketDescriptors.GetByTicketId(ticket.id);
			var clientsLost = descriptor.ClientOffRate * (ticket.releaseDay!.Value - ticket.takingDay);
			return descriptor.ClientsProvides - clientsLost;
		}
	}
}