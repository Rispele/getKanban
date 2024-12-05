using Domain.Attributes;
using Domain.DomainExceptions;
using Domain.Game.Days.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(ReleaseTicketContainerEntityTypeConfiguration))]
public class ReleaseTicketContainer : FreezableDayContainer
{
	[Tracking("is_updated_tracker")]
	private readonly List<string> ticketIds;

	public IReadOnlyList<string> TicketIds => ticketIds;

	public bool CanReleaseNotImmediatelyTickets { get; private set; }

	internal ReleaseTicketContainer(bool canReleaseNotImmediatelyTickets)
	{
		ticketIds = [];
		CanReleaseNotImmediatelyTickets = canReleaseNotImmediatelyTickets;
	}

	internal void CanReleaseNotImmediately(bool canReleaseNotImmediatelyTickets = true)
	{
		CanReleaseNotImmediatelyTickets = canReleaseNotImmediatelyTickets;
	}
	
	internal void Update(TicketDescriptor ticket)
	{
		EnsureNotFrozen();
		if (!CanReleaseNotImmediatelyTickets && !ticket.CanBeReleasedImmediately)
		{
			throw new DomainException("Cannot release ticket in that state of day");
		}

		if (ticketIds.Contains(ticket.Id))
		{
			return;
		}
		
		ticketIds.Add(ticket.Id);
		SetUpdated();
	}
	
	internal void Remove(TicketDescriptor ticket)
	{
		EnsureNotFrozen();

		if (!ticketIds.Contains(ticket.Id))
		{
			return;
		}
		
		ticketIds.Remove(ticket.Id);
		SetUpdated();
	}
}