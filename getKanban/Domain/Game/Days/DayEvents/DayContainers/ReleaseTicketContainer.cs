using Domain.DomainExceptions;
using Domain.Game.Days.DayEvents.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayEvents.DayContainers;

[EntityTypeConfiguration(typeof(ReleaseTicketContainerEntityTypeConfiguration))]
public class ReleaseTicketContainer
{
	private readonly List<string> ticketIds;

	public long Id { get; }
	public bool Frozen { get; private set; }
	public IReadOnlyList<string> TicketIds => ticketIds;

	internal ReleaseTicketContainer()
	{
		ticketIds = [];
	}
	
	internal void Update(string ticketId)
	{
		if (Frozen)
		{
			throw new DomainException("Cannot update frozen container");
		}

		if (ticketIds.Contains(ticketId))
		{
			return;
		}
		
		ticketIds.Add(ticketId);
	}
	
	internal void Remove(string ticketId)
	{
		if (Frozen)
		{
			throw new DomainException("Cannot update frozen container");
		}

		if (!ticketIds.Contains(ticketId))
		{
			return;
		}
		
		ticketIds.Remove(ticketId);
	}
	
	internal void Freeze()
	{
		Frozen = true;
	}
}