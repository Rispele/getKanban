using Domain.DomainExceptions;
using Domain.Game.Days.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(UpdateSprintBacklogContainerEntityTypeConfiguration))]
public class UpdateSprintBacklogContainer : FreezableDayContainer
{
	private readonly List<string> ticketIds;

	public IReadOnlyList<string> TicketIds => ticketIds;
		
	internal UpdateSprintBacklogContainer()
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
		SetUpdated();
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
		SetUpdated();
	}
}