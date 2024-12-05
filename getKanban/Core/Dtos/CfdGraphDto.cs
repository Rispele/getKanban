using WebApp.Models;

namespace Core.Dtos;

public class CfdGraphDto
{
	public HashSet<int> DaysToShow = new HashSet<int>();
	public HashSet<int> TotalTasksToShow = new HashSet<int>();
	public Dictionary<string, List<(int, int)>> GraphPointsPerLabel = new ();
	public DayFullIdDto DayFullIdDto { get; init; }
}