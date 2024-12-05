using Core.Dtos.DayStatistics;

namespace Core.Services.Contracts;

public interface IStatisticsService
{
	public Task<TeamStatisticDto> EvaluateProfitsAsync(Guid gameSessionId, Guid teamId);
}