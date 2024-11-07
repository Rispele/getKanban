using Domain.DomainExceptions;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class ReleaseTicketsCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.ReleaseTickets;

	public bool Remove { get; init; }

	public IReadOnlyList<string> TicketIds { get; init; } = [];
	
	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		if (Remove)
		{
			TicketIds.ForEach(t => day.ReleaseTicketContainer.Remove(t));
		}
		else
		{
			EnsureCanReleaseTickets(team);
			TicketIds.ForEach(t => day.ReleaseTicketContainer.Update(t));
		}
		
		day.PostDayEvent(CommandType, null);
	}
	
	private void EnsureCanReleaseTickets(Team team)
	{
		if (!team.BuildTicketsInWork(team.Days).IsSupersetOf(TicketIds))
		{
			throw new DayActionIsProhibitedException("You cannot release not in work tickets");
		}
	}
}