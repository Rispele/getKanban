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

		day.EndDay();

		team.AddNextDay();
		day.PostDayEvent(CommandType, null);
	}

	private void EnsureCfdIsValid(Team team)
	{
		if (!team.IsCurrentDayCfdValid())
		{
			throw new DomainException("Invalid cfd arguments");
		}
	}
}