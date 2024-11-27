namespace Core.Dtos.Containers;

public class ReleaseTicketContainerDto : DayContainerDto
{
	public IReadOnlyList<string> TicketIds { get; init; }
}