using Domain.Game.Days.DayContainers;
using Domain.Game.Days.DayContainers.RollDice;
using Domain.Game.Days.DayContainers.TeamMembers;
using Domain.Game.Teams;

namespace Domain.Game.Days.Commands;

public class RollDiceCommand : DayCommand
{
	public override DayCommandType CommandType => DayCommandType.RollDice;

	internal override void Execute(Team team, Day day)
	{
		day.EnsureCanPostEvent(CommandType);

		var diceRoller = new DiceRoller(new Random());
		var teamMembers = day.TeamMembersContainer.TeamMembers;
		day.DiceRollContainer = RollDiceContainer.CreateInstance(teamMembers.Select(RollDiceForMember));

		day.PostDayEvent(CommandType, null);

		return;

		DiceRollResult RollDiceForMember(TeamMember member)
		{
			var diceNumber = diceRoller.RollDice();
			var scores = MapDiceNumberToScoreSettings.MapByRole(
				member.InitialRole,
				member.CurrentRole,
				diceNumber);
			return new DiceRollResult(member.Id, diceNumber, scores);
		}
	}
}