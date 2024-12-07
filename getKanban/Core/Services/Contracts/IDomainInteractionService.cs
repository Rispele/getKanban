using Domain.Game;

namespace Core.Services.Contracts;

public interface IDomainInteractionService
{
	public Task<List<Ticket>> GetTicketsToRelease(Guid sessionId, Guid teamId);

	public Task<List<Ticket>> GetBacklogTickets(Guid sessionId, Guid teamId);

	public Task<bool> RestartDay(Guid sessionId, Guid teamId, int dayNumber);
}