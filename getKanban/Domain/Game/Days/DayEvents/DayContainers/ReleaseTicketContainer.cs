using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(ReleaseTicketContainerEntityTypeConfiguration))]
public class ReleaseTicketContainer
{
	public long Id { get; }
	public long DayId { get; }
	public IReadOnlyList<string> TicketIds { get; } = null!;

	[UsedImplicitly]
	private ReleaseTicketContainer()
	{
	}

	private ReleaseTicketContainer(long dayId, IReadOnlyList<string> ticketIds)
	{
		DayId = dayId;
		TicketIds = ticketIds;
	}

	internal static ReleaseTicketContainer CreateInstance(
		Day day,
		IReadOnlyList<string> ticketIds)
	{
		day.PostDayEvent(DayEventType.ReleaseTickets);
		return new ReleaseTicketContainer(day.Id, ticketIds);
	}
}