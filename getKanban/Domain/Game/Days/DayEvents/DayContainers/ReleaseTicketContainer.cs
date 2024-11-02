using Domain.Game.Days.DayEvents.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(ReleaseTicketContainerEntityTypeConfiguration))]
public class ReleaseTicketContainer
{
	public long Id { get; }
	public IReadOnlyList<string> TicketIds { get; } = null!;

	[UsedImplicitly]
	private ReleaseTicketContainer()
	{
	}

	private ReleaseTicketContainer(IReadOnlyList<string> ticketIds)
	{
		TicketIds = ticketIds;
	}

	internal static ReleaseTicketContainer CreateInstance(
		Day day,
		IReadOnlyList<string> ticketIds)
	{
		day.PostDayEvent(DayEventType.ReleaseTickets);
		return new ReleaseTicketContainer(ticketIds);
	}
}