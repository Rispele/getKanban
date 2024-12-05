using Core.DbContexts;
using Core.DbContexts.Extensions;
using Core.Dtos;
using Core.Dtos.Converters;
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
		UserCredentialsDto userCredentialsDto,
		DayCommand dayCommand)
	{
		var team = await context.GetTeamAsync(userCredentialsDto.SessionId, userCredentialsDto.TeamId);

		if (!team.HasAccess(userCredentialsDto.UserId))
		{
			throw new InvalidOperationException($"User with id: {userCredentialsDto.UserId} do not have access to this team.");
		}
		
		team.ExecuteCommand(dayCommand);

		await context.SaveChangesAsync();

		return dayDtoConverter.Convert(team.CurrentDay!);
	}

	public async Task<DayDto> GetCurrentDayAsync(Guid gameSessionId, Guid teamId)
	{
		var team = await context.GetTeamAsync(gameSessionId, teamId);
		return dayDtoConverter.Convert(team.CurrentDay!);
	}
}