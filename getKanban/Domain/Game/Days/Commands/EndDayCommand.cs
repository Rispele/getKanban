using Domain.DomainExceptions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class EndDayCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.EndDay;

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);
		EnsureCfdIsValid(team);
		
		day.ReleaseTicketContainer.Freeze();
		day.UpdateSprintBacklogContainer.Freeze();
		day.UpdateCfdContainer.Freeze();

		team.AddNextDay();
		day.PostDayEvent(CommandType, null);
	}

	private void EnsureCfdIsValid(Team team)
	{
		var previousDayCfd = team.PreviousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None;

		if (!team.CurrentDay!.IsCfdValid(previousDayCfd))
		{
			throw new DomainException("Invalid cfd arguments");
		}
	}
}