using Domain.Game;
using WebApp.Models;

namespace Core.Dtos;

public class TicketsViewDto
{
	public DayFullIdDto dayFullIdDto { get; init; } 
	
	public string PageType { get; init; }

	public List<Ticket> TicketIds { get; init; }
}