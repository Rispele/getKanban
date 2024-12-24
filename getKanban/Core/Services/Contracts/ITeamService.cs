using System.Data;
using Core.Dtos;
using Core.RequestContexts;
using Domain.Game.Days.Commands;

namespace Core.Services.Contracts;

public interface ITeamService
{
	public Task<DayDto> PatchDayAsync(
		RequestContext requestContext,
		Guid gameSessionId,
		Guid teamId,
		DayCommand dayCommand);

	public Task<DayDto> GetCurrentDayAsync(RequestContext requestContext, Guid gameSessionId, Guid teamId);

	public Task RemoveCurrentlyAwaitedCommandsOfType(
		DayCommandType type,
		RequestContext requestContext,
		Guid gameSessionId,
		Guid teamId);
}