using Domain.DomainExceptions;
using Domain.Game.Teams;
using Domain.Game.Tickets;

namespace Domain.Game.Days.Commands;

public class UpdateSprintBacklogCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.UpdateSprintBacklog;

	public IReadOnlyList<string> TicketIds { get; init; } = null!;

	public bool Remove { get; init; }

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);
		var previousDayLastTicketNumber = team.PreviousDay?.UpdateSprintBacklogContainer.TicketIds
			.Select(TicketDescriptors.GetByTicketId)
			.Where(t => t.ShouldBeTakenSequentially)
			.MaxBy(t => t.Number)
			?.Number ?? team.Settings.InitiallyTakenTickets
			.Select(t => TicketDescriptors.GetByTicketId(t.id)).MaxBy(t => t.Number)?.Number;

		foreach (var ticketId in TicketIds)
		{
			if (Remove)
			{
				day.UpdateSprintBacklogContainer.Remove(ticketId);
			}
			else
			{
				EnsureCanTakeTickets(team);
				day.UpdateSprintBacklogContainer.Update(previousDayLastTicketNumber, ticketId);
			}
		}

		day.PostDayEvent(CommandType, null);
	}

	private void EnsureCanTakeTickets(Team team)
	{
		if (team.GetTakenTicketIds(team.Days).Overlaps(TicketIds))
		{
			throw new DayActionIsProhibitedException("You cannot take already taken tickets");
		}
	}
}