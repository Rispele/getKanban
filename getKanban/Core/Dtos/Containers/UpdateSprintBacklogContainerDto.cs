namespace Core.Dtos.Containers;

public class UpdateSprintBacklogContainerDto : DayContainerDto
{
	public IReadOnlyList<string> TicketIds { get; init; }
}