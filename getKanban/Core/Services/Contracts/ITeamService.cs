using Core.Dtos;
using Core.RequestContexts;
using Domain.Game.Days.Commands;

namespace Core.Services.Contracts;

public interface ITeamService
{
	public Task<DayDto> PatchDayAsync(UserCredentialsDto userCredentialsDto, DayCommand dayCommand);

	public Task<DayDto> GetCurrentDayAsync(RequestContext requestContext, Guid gameSessionId, Guid teamId);
}