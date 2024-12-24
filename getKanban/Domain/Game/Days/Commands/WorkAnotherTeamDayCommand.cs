using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class WorkAnotherTeamDayCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.WorkAnotherTeam;

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		var diceRoller = new DiceRoller(new Random());
		var diceNumber = diceRoller.RollDice();
		var diceScores = MapDiceNumberToScoreSettings.MapAnotherTeam(diceNumber);

		day.WorkAnotherTeamContainer = WorkAnotherTeamContainer.CreateInstance(diceNumber, diceScores);
		day.PostDayEvent(CommandType, null);
	}
}