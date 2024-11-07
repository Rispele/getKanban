using Domain.DomainExceptions;
using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class EndDayCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.EndDay;

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);
		EnsureCfdIsValid(team);
		
		day.ReleaseTicketContainer.Freeze();
		day.UpdateSprintBacklogContainer.Freeze();
		day.UpdateCfdContainer.Freeze();

		team.AddNextDay();
		day.PostDayEvent(CommandType, null);
	}

	private void EnsureCfdIsValid(Team team)
	{
		var currentDayCfd = team.CurrentDay.UpdateCfdContainer;
		var previousDayCfd = team.previousDay?.UpdateCfdContainer ?? UpdateCfdContainer.None;

		if (currentDayCfd
		    is { Released: null }
		    or { ToDeploy: null }
		    or { WithTesters: null }
		    or { WithProgrammers: null }
		    or { WithAnalysts: null })
		{
			throw new DomainException("Invalid cfd arguments");
		}

		var currentSumToValidate = currentDayCfd.Released! + currentDayCfd.ToDeploy!;
		var previousSumToValidate = previousDayCfd.Released! + previousDayCfd.ToDeploy!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithTesters;
		previousSumToValidate += previousDayCfd.WithTesters!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithProgrammers;
		previousSumToValidate += previousDayCfd.WithProgrammers!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		currentSumToValidate += currentDayCfd.WithAnalysts;
		previousSumToValidate += previousDayCfd.WithAnalysts!;
		ValidateArgumentsSum(currentSumToValidate.Value, previousSumToValidate.Value);

		void ValidateArgumentsSum(int currentSum, int previousSum)
		{
			if (currentSum < previousSum)
			{
				throw new DomainException("Invalid cfd arguments");
			}
		}
	}
}