using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class UpdateCfdCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.UpdateCfd;
	
	public UpdateCfdContainerPatchType PatchType { get; init; }
	
	public int Value { get; init; }
	
	internal override void Execute(Team _, Day day)
	{
		day.EnsureCanPostEvent(CommandType);
		
		day.UpdateCfdContainer.Update(PatchType, Value);
		
		day.PostDayEvent(CommandType, day.UpdateCfdContainer);
	}
}