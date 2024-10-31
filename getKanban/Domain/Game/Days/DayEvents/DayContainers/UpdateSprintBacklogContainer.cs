namespace Domain.Game.Days.DayEvents.DayContainers;

public class UpdateSprintBacklogContainer
{
	private UpdateSprintBacklogContainer(int dayId, string[] ticketIds)
	{
		DayId = dayId;
		TicketIds = ticketIds;
	}

	public int DayId { get; }
	public IReadOnlyList<string> TicketIds { get; }

	internal static UpdateSprintBacklogContainer CreateInstance(DayContext dayContext, string[] ticketIds)
	{
		dayContext.PostDayEvent(DayEventType.UpdateSprintBacklog);

		return new UpdateSprintBacklogContainer(dayContext.DayId, ticketIds);
	}
}