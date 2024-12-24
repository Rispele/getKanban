using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public abstract class DayCommand
{
	public abstract DayCommandType CommandType { get; }


	internal abstract void Execute(Team team, Day day);
}