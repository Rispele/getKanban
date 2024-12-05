namespace Domain.Game.Tickets;

public record TicketDescriptor(
	string Id,
	int ClientsProvides,
	int ClientOffRate,
	TicketBonusDescriptor? Bonus,
	TicketPenaltyDescriptor? Penalty);