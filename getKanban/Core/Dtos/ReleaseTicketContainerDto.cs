namespace Core.Dtos;

public class ReleaseTicketContainerDto : DayContainerDto
{
	public IReadOnlyList<string> TicketIds { get; init; }
}