using Domain.Game.Days.Commands;

namespace Core.Dtos;

public class AwaitedEventsDto
{
	public DayCommandType CommandType { get; set; }
}