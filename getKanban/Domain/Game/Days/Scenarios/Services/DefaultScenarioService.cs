using Domain.Game.Teams;

namespace Domain.Game.Days.Scenarios.Services;

public class DefaultScenarioService : IScenarioService
{
	public bool IsCfdNotValid(Team team)
	{
		return !team.IsCurrentDayCfdValid();
	}

	public bool IsCfdValid(Team team)
	{
		return team.IsCurrentDayCfdValid();
	}
}