using Domain.Game.Teams;

namespace Domain.Game.Tickets;

public record TicketDescriptor(
	string Id,
	int ClientsProvides,
	int ClientOffRate,
	bool CanBeReleasedImmediately = false,
	TicketBonusDescriptor? Bonus = null,
	TicketPenaltyDescriptor? Penalty = null,
	Action<Team>? OnRelease = null,
	Action<Team>? OnRemove = null);