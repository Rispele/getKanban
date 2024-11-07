using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Domain.Game.Days.DayEvents.DayContainers;

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
	
	public async Task<UpdateTeamRolesContainerDto> UpdateTeamRole(
		Guid gameSessionId,
		Guid teamId,
		TeamRole from,
		TeamRole to)
	{
		var team = await context.GetTeam(gameSessionId, teamId);

		team.UpdateTeamRoles(from, to);
		await context.SaveChangesAsync();
		
		return dayDtoConverter.Convert(team.CurrentDay).UpdateTeamRolesContainer;
	}
}