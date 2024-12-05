using Domain.Game;

namespace Core.Dtos;

public class TicketsViewDto
{
	public string PageType { get; init; }
	
	public int CurrentDayNumber { get; init; }

	public Guid SessionId { get; init; }

	public List<Ticket> TicketIds { get; init; }
}