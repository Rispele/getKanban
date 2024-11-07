using Core.DbContexts;
using Core.DbContexts.Extensions;

namespace Core;

public class TeamService
{
	private readonly TeamsContext context;

	public TeamService(TeamsContext context)
	{
		this.context = context;
	}

	public async Task RollDicesForAnotherTeam(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeam(gameSessionId, teamId);

		team.RollDiceForAnotherTeam();

		await context.SaveChangesAsync();
	}
}