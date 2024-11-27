using Domain.DbContexts;
using Domain.DomainExceptions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class UpdateTeamRolesCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.UpdateTeamRoles;
	
	public bool Remove { get; init; }
	public TeamRole From { get; init; }
	public TeamRole To { get; init; }
	public long UpdateIdToRemove { get; init; }
	
	internal override void Execute(DomainContext context, Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);
		
		if (Remove)
		{
			day.UpdateTeamRolesContainer.Remove(UpdateIdToRemove);
		}
		
		EnsureCanUpdateTeamRoles(day);
		day.UpdateTeamRolesContainer.AddUpdate(From, To);
		
		day.PostDayEvent(CommandType, null);
	}
	
	private void EnsureCanUpdateTeamRoles(Day day)
	{
		var update = day.UpdateTeamRolesContainer.BuildTeamRolesUpdate();

		if (!update.TryGetValue(From, out var updates))
		{
			return;
		}

		var limit = From switch
		{
			TeamRole.Analyst => day.AnalystsNumber,
			TeamRole.Programmer => day.ProgrammersNumber,
			TeamRole.Tester => day.TestersNumber,
			_ => throw new ArgumentOutOfRangeException()
		};

		if (updates.Length + 1 > limit)
		{
			throw new DayActionIsProhibitedException(
				$"{From} role updates count can't be more than members count{limit}");
		}
	}
}