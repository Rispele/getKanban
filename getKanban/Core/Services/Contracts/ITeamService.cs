using Core.Dtos;
using Domain.Game.Days.Commands;

namespace Core.Services.Contracts;

public interface ITeamService
{
	public Task<DayDto> PatchDayAsync(
		Guid gameSessionId,
		Guid teamId,
		Guid userId,
		DayCommand dayCommand);

	public Task<DayDto> GetCurrentDayAsync(Guid gameSessionId, Guid teamId);
}