using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(AwaitedEventEntityTypeConfiguration))]
public class AwaitedCommands
{
	public long Id { get; }

	public bool Removed { get; private set; }

	public DayCommandType CommandType { get; }


	[UsedImplicitly]
	public AwaitedCommands()
	{
	}

	public AwaitedCommands(DayCommandType commandType)
	{
		CommandType = commandType;
	}

	public void MarkRemoved()
	{
		Removed = true;
	}
}