using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(UpdateSprintBacklogContainerEntityTypeConfiguration))]
public class UpdateSprintBacklogContainer
{
	public long Id { get; set; }
	public long DayId { get; }
	public IReadOnlyList<string> TicketIds { get; } = null!;

	[UsedImplicitly]
	private UpdateSprintBacklogContainer()
	{
	}

	private UpdateSprintBacklogContainer(long dayId, string[] ticketIds)
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