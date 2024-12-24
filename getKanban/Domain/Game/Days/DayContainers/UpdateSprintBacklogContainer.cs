using Domain.Attributes;
using Domain.DomainExceptions;
using Domain.Game.Days.Configurations;
using Domain.Game.Tickets;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days.DayContainers;

[EntityTypeConfiguration(typeof(UpdateSprintBacklogContainerEntityTypeConfiguration))]
public class UpdateSprintBacklogContainer : FreezableDayContainer
{
	[Tracking("is_updated_tracker")] private readonly List<string> ticketIds;

	private readonly Lazy<List<TicketDescriptor>> descriptors;

	public IReadOnlyList<string> TicketIds => ticketIds;

	internal UpdateSprintBacklogContainer()
	{
		ticketIds = [];
		descriptors =
			new Lazy<List<TicketDescriptor>>(() => ticketIds.Select(TicketDescriptors.GetByTicketId).ToList());
	}

	private void EnsurePreviousTicketsAdded(int? previousDayLastTicketNumber, string ticketId)
	{
		var descriptor = TicketDescriptors.GetByTicketId(ticketId);
		if (!descriptor.ShouldBeTakenSequentially)
		{
			return;
		}

		var lastTicketNumber = descriptors.Value
			                       .Where(t => t.ShouldBeTakenSequentially)
			                       .OrderBy(t => t.Number)
			                       .LastOrDefault()?.Number
		                    ?? previousDayLastTicketNumber
		                    ?? 0;

		if (lastTicketNumber + 1 != descriptor.Number)
		{
			throw new DomainException("Add previous ticket firstly");
		}
	}

	private void EnsureTicketIsOfMaxNumber(string ticketId)
	{
		var descriptor = TicketDescriptors.GetByTicketId(ticketId);
		if (!descriptor.ShouldBeTakenSequentially)
		{
			return;
		}

		if (descriptors.Value.Select(t => t.Number).Max() != descriptor.Number)
		{
			throw new DomainException("Could not remove ticket with not max number");
		}
	}

	internal void Update(int? previousDayLastTicketNumber, string ticketId)
	{
		EnsureNotFrozen();
		EnsurePreviousTicketsAdded(previousDayLastTicketNumber, ticketId);

		if (ticketIds.Contains(ticketId))
		{
			return;
		}

		ticketIds.Add(ticketId);
		SetUpdated();
	}

	internal void Remove(string ticketId)
	{
		EnsureNotFrozen();


		if (!ticketIds.Contains(ticketId))
		{
			return;
		}

		EnsureTicketIsOfMaxNumber(ticketId);

		ticketIds.Remove(ticketId);
		SetUpdated();
	}
}