using Core.Dtos.DayStatistics;

namespace Core.Dtos;

public class AdminPanelDaysDto
{
	public Guid SessionId { get; set; }

	public Guid TeamId { get; set; }

	public string TeamName { get; set; } = null!;

	public DayDto CurrentDay { get; init; } = null!;

	public int StartDayNumber { get; init; }

	public int FinishDayNumber { get; init; }

	public TeamStatisticDto? TeamStatistic { get; set; }

	public bool IsFinished { get; set; }
}