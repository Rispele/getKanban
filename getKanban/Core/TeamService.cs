using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;

namespace Core;

public class TeamService
{
	private readonly TeamsContext context;
	private readonly DayDtoConverter dayDtoConverter;

	public TeamService(TeamsContext context, DayDtoConverter dayDtoConverter)
	{
		this.context = context;
		this.dayDtoConverter = dayDtoConverter;
	}

	public async Task<WorkAnotherTeamContainerDto> RollDicesForAnotherTeam(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeam(gameSessionId, teamId);

		team.RollDiceForAnotherTeam();
		await context.SaveChangesAsync();
		
		return dayDtoConverter.Convert(team.CurrentDay).WorkAnotherTeamContainer!;
	}
}