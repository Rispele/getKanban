using Core.Dtos.DayStatistics;

namespace Core.Services.Contracts;

public interface IStatisticsService
{
	public Task<TeamStatisticDto> CollectStatistic(Guid gameSessionId, Guid teamId);
}