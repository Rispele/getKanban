using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.RequestContexts;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Game.Days.Commands;

namespace Core.Services.Implementations;

public class TeamService : ITeamService
{
	private readonly DomainContext context;
	private readonly DayDtoConverter dayDtoConverter;

	public TeamService(DayDtoConverter dayDtoConverter, DomainContext context)
	{
		this.dayDtoConverter = dayDtoConverter;
		this.context = context;
	}

	public async Task<DayDto> PatchDayAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		Guid teamId,
		DayCommand dayCommand)
	{
		var userId = requestContext.GetUserId();
		var team = await context.GetTeamAsync(gameSessionId, teamId);

		if (!team.HasAccess(userId))
		{
			throw new InvalidOperationException($"User with id: {userId} do not have access to this team.");
		}

		team.ExecuteCommand(dayCommand);

		await context.SaveChangesAsync();

		return dayDtoConverter.Convert(team, team.CurrentDay);
	}

	public async Task<DayDto> GetCurrentDayAsync(RequestContext requestContext, Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);

		team.EnsureHasAccess(requestContext.GetUserId());

		return dayDtoConverter.Convert(team, team.CurrentDay);
	}
}