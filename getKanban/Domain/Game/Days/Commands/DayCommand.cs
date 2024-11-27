using Domain.DbContexts;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public abstract class DayCommand
{
	public abstract DayCommandType CommandType { get; }
	
	internal abstract void Execute(DomainContext context, Team team, Day day);
}