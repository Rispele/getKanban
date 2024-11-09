using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Domain.Game.Days.Commands;

namespace Core;

public class TeamService
{
	private readonly DomainContext context;
	private readonly DayDtoConverter dayDtoConverter;

	public TeamService(DayDtoConverter dayDtoConverter, DomainContext context)
	{
		this.dayDtoConverter = dayDtoConverter;
		this.context = context;
	}

	public async Task<DayDto> PatchDayAsync(
		Guid gameSessionId,
		Guid teamId,
		Guid userId,
		DayCommand dayCommand)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);

		if (!team.HasAccess(userId))
		{
			throw new InvalidOperationException($"User with id: {userId} do not have access to this team.");
		}
		
		team.ExecuteCommand(dayCommand);

		await context.SaveChangesAsync();

		return dayDtoConverter.Convert(team.CurrentDay);
	}

	public async Task<DayDto> GetCurrentDayAsync(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);
		return dayDtoConverter.Convert(team.CurrentDay);
	}
}