using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.DayStatistics;
using Core.Dtos.Statistics;
using Core.Services.Contracts;
using Domain;
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
		var clientsGainedPerDay = EvaluateClientsGainedPerDay(takenTickets, team.CurrentDay.Number);
		var profitPerClientPerDay = team.Days.ToDictionary(day => day.Number, day => day.DaySettings.ProfitPerClient);

		var clientsPerDay = EvaluateClientPerDay(team, clientsGainedPerDay);
		var profitGainedPerDay = EvaluateProfitGainedPerDay(profitPerClientPerDay, clientsPerDay);

		var dayStats = ConvertToDayStatisticDto(
			team,
			clientsGainedPerDay,
			profitGainedPerDay,
			profitPerClientPerDay);

		return TeamStatisticDto.Create(
			team.Id,
			dayStats,
			EvaluatePenalty(team),
			EvaluateBonusProfit(team));
	}

	public CfdStatisticDto CollectCfdStatistic(Day day)
	{
		var cfdContainer = day.UpdateCfdContainer;

		return CfdStatisticDto.Create(
			cfdContainer.WithAnalysts ?? 0,
			cfdContainer.WithProgrammers ?? 0,
			cfdContainer.WithTesters ?? 0,
			cfdContainer.ToDeploy ?? 0,
			cfdContainer.Released ?? 0);
	}

	private List<DayStatisticDto> ConvertToDayStatisticDto(
		Team team,
		Dictionary<int, int> clientsGainedPerDay,
		Dictionary<int, int> profitGainedPerDay,
		Dictionary<int, int> profitPerClientPerDay)
	{
		return team.Days
			.Select(
				day => DayStatisticDto.Create(
					dayNumber: day.Number,
					clientsGained: clientsGainedPerDay[day.Number],
					profitGained: profitGainedPerDay[day.Number],
					profitPerClient: profitPerClientPerDay[day.Number],
					cfdStatistic: CollectCfdStatistic(day)))
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