namespace Core.Dtos;

public class UpdateSprintBacklogContainerDto : DayContainerDto
{
	public IReadOnlyList<string> TicketIds { get; init; }
}