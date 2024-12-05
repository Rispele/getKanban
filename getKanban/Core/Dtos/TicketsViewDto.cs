namespace Core.Dtos;

public class TicketsViewDto
{
	public string PageType { get; set; }

	public Guid SessionId { get; set; }

	public List<string> TicketIds { get; set; }
}