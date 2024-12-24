using Domain.Game.Days.Commands;
using Domain.Game.Days.Configurations;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Domain.Game.Days;

[EntityTypeConfiguration(typeof(AwaitedEventEntityTypeConfiguration))]
public class AwaitedCommands
{
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

	internal long Id { get; }

	internal void MarkRemoved()
	{
		Removed = true;
	}
}