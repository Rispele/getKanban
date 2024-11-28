using Domain.Game.Days.DayContainers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class RollDiceCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.RollDice;

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		var diceRoller = new DiceRoller(new Random());
		var swapByRole = day.UpdateTeamRolesContainer.BuildTeamRolesUpdate();
		var (analystsDiceNumber, analystsScores) = RollDiceForRole(day.AnalystsNumber, TeamRole.Analyst);
		var (programmersDiceNumber, programmersScores) = RollDiceForRole(day.ProgrammersNumber, TeamRole.Programmer);
		var (testersDiceNumber, testersScores) = RollDiceForRole(day.TestersNumber, TeamRole.Tester);

		day.RollDiceContainer = RollDiceContainer.CreateInstance(
			analystsDiceNumber,
			programmersDiceNumber,
			testersDiceNumber,
			analystsScores,
			programmersScores,
			testersScores);

		day.PostDayEvent(CommandType, null);
		return;

		(int[] diceNumber, int[] diceScores) RollDiceForRole(int roleSize, TeamRole role)
		{
			var swaps = swapByRole.GetValueOrDefault(role, []);
			var diceNumber = new int[roleSize];
			var diceScores = new int[roleSize];
			for (var i = 0; i < roleSize; i++)
			{
				var asRole = i < swaps.Length ? swaps[i] : role;
				diceNumber[i] = diceRoller.RollDice();
				diceScores[i] = MapDiceNumberToScoreSettings.MapByRole(role, asRole, diceNumber[i]);
			}

			return (diceNumber, diceScores);
		}
	}
}