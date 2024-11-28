using Domain.DomainExceptions;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class UpdateSprintBacklogCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.UpdateSprintBacklog;

	public IReadOnlyList<string> TicketIds { get; init; } = null!;

	public bool Remove { get; init; }

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		foreach (var ticketId in TicketIds)
		{
			if (Remove)
			{
				day.UpdateSprintBacklogContainer.Remove(ticketId);
			}
			else
			{
				EnsureCanTakeTickets(team);
				day.UpdateSprintBacklogContainer.Update(ticketId);
			}
		}

		day.PostDayEvent(CommandType, null);
	}

	private void EnsureCanTakeTickets(Team team)
	{
		if (team.BuildTakenTickets(team.Days).Overlaps(TicketIds))
		{
			throw new DayActionIsProhibitedException("You cannot take already taken tickets");
		}
	}
}