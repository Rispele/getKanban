using Domain.Game.Days.Commands;

namespace Core.Dtos;

public class AwaitedEventsDto
{
	public bool Removed { get; set; }

	public DayCommandType CommandType { get; set; }
}