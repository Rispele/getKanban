﻿using Domain.DomainExceptions;
using Domain.Game.Days.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(ReleaseTicketContainerEntityTypeConfiguration))]
public class ReleaseTicketContainer : FreezableDayContainer
{
	private readonly List<string> ticketIds;

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
		Version++;
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
		Version++;
	}
}