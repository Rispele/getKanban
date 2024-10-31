using Domain.Game.Days.DayEvents.DayContainers.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(UpdateSprintBacklogContainerEntityTypeConfiguration))]
public class UpdateSprintBacklogContainer
{
	public int DayId { get; }
	public IReadOnlyList<string> TicketIds { get; }

	private UpdateSprintBacklogContainer(int dayId, string[] ticketIds)
	{
		DayId = dayId;
		TicketIds = ticketIds;
	}

	internal static UpdateSprintBacklogContainer CreateInstance(Day day, string[] ticketIds)
	{
		day.PostDayEvent(DayEventType.UpdateSprintBacklog);

		return new UpdateSprintBacklogContainer(day.Id, ticketIds);
	}
}