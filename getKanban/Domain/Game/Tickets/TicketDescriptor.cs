using Domain.Game.Teams;

namespace Domain.Game.Tickets;

public record TicketDescriptor(
	string Id,
	int ClientsProvides,
	int ClientOffRate,
	bool ShouldBeTakenSequentially = true,
	int Number = 0,
	bool CanBeReleasedImmediately = false,
	TicketBonusDescriptor? Bonus = null,
	TicketPenaltyDescriptor? Penalty = null,
	Action<Team>? OnRelease = null,
	Action<Team>? OnRemove = null);