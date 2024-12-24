using Domain.DomainExceptions;
using Domain.Game.Teams;
using Domain.Game.Tickets;

namespace Domain.Game.Days.Commands;

public class ReleaseTicketsCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.ReleaseTickets;

	public string TicketId { get; }

	public bool Remove { get; }

	private ReleaseTicketsCommand(string ticketId, bool remove)
	{
		TicketId = ticketId;
		Remove = remove;
	}

	public static ReleaseTicketsCommand Create(string ticketId, bool remove)
	{
		return new ReleaseTicketsCommand(ticketId, remove);
	}

	private void EnsureCanReleaseTickets(Team team)
	{
		if (!team.GetTicketsInWorkIds(team.Days).Contains(TicketId))
		{
			throw new DayActionIsProhibitedException("You cannot release not in work tickets");
		}
	}

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		var ticket = TicketDescriptors.GetByTicketId(TicketId);

		if (Remove)
		{
			day.ReleaseTicketContainer.Remove(ticket);
			ticket.OnRemove?.Invoke(team);
		}
		else
		{
			EnsureCanReleaseTickets(team);

			ticket.OnRelease?.Invoke(team);
			day.ReleaseTicketContainer.Update(ticket);
		}

		day.PostDayEvent(CommandType, null);
	}
}