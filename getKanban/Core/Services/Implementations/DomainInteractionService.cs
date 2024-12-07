using Core.DbContexts.Extensions;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game;
using Domain.Game.Tickets;

namespace Core.Services.Implementations;

public class DomainInteractionService : IDomainInteractionService
{
	private readonly DomainContext context;

	public DomainInteractionService(DomainContext context)
	{
		this.context = context;
	}
	
	public async Task<List<Ticket>> GetTicketsToRelease(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		var tickets = team.BuildTakenTickets();

		var previousDayNumber = team.CurrentDay.Number - 1;
		return tickets
			.Where(x => x.IsInWork(previousDayNumber))
			.Where(
				x => team.CurrentDay.ReleaseTicketContainer.CanReleaseNotImmediatelyTickets
				     || TicketDescriptors.GetByTicketId(x.id).CanBeReleasedImmediately)
			.ToList();
	}

	public async Task<List<Ticket>> GetBacklogTickets(Guid sessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);

		var tickets = team.BuildTakenTickets();

		var currentDayNumber = team.CurrentDay.Number;
		var previousDayNumber = currentDayNumber - 1;
		var ticketsTakenThisDay = tickets
			.Where(ticket => !ticket.IsTaken(previousDayNumber) && ticket.IsTaken(currentDayNumber))
			.ToList();

		var ticketsNotTaken = TicketDescriptors.AllTicketDescriptors
			.Where(x => tickets.All(t => t.id != x.Id))
			.Select(t => new Ticket(t.Id, int.MaxValue, null));

		return ticketsTakenThisDay.Concat(ticketsNotTaken).ToList();
	}

	public async Task<bool> RestartDay(Guid sessionId, Guid teamId, int dayNumber)
	{
		var team = await context.GetTeamAsync(sessionId, teamId);
		var rolled = team.RollbackToDay(dayNumber);
		await context.SaveChangesAsync();
		return rolled;
	}
}