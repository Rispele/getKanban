using Domain.Game.Days.DayEvents.UpdateTeamRolesDayEvent;

#pragma warning disable CS8509 // The switch expression does not handle all possible values of its input type (it is not exhaustive).

namespace Domain.Game;

public class MapDiceNumberToScoreSettings
{
	public static int MapByRole(TeamRole role, TeamRole asRole, int diceNumber)
	{
		ValidateDiceNumber(diceNumber);
		return role switch
		{
			TeamRole.Analyst => MapAnalyst(diceNumber, asRole),
			TeamRole.Programmer => MapProgrammer(diceNumber, asRole),
			TeamRole.Tester => MapTester(diceNumber, asRole),
			_ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid team role")
		};
	}

	public static int MapAnotherTeam(int diceNumber)
	{
		ValidateDiceNumber(diceNumber);

		return diceNumber switch
		{
			1 => 0,
			2 => 2,
			3 => 3,
			4 => 4,
			5 => 5,
			6 => 15
		};
	}

	private static int MapAnalyst(int diceNumber, TeamRole asRole)
	{
		return asRole switch
		{
			TeamRole.Analyst => diceNumber switch
			{
				1 => 3,
				2 => 4,
				3 => 4,
				4 => 5,
				5 => 5,
				6 => 6
			},
			TeamRole.Programmer => diceNumber switch
			{
				1 => 1,
				2 => 2,
				3 => 0,
				4 => 2,
				5 => 1,
				6 => 3
			},
			TeamRole.Tester => diceNumber switch
			{
				1 => 2,
				2 => 1,
				3 => 2,
				4 => 3,
				5 => 3,
				6 => 1
			},
			_ => throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role")
		};
	}

	private static int MapProgrammer(int diceNumber, TeamRole asRole)
	{
		return asRole switch
		{
			TeamRole.Analyst => diceNumber switch
			{
				1 => 2,
				2 => 2,
				3 => 1,
				4 => 1,
				5 => 3,
				6 => 1
			},
			TeamRole.Programmer => diceNumber switch
			{
				1 => 3,
				2 => 3,
				3 => 4,
				4 => 4,
				5 => 5,
				6 => 6
			},
			TeamRole.Tester => diceNumber switch
			{
				1 => 1,
				2 => 1,
				3 => 2,
				4 => 0,
				5 => 2,
				6 => 3
			},
			_ => throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role")
		};
	}

	private static int MapTester(int diceNumber, TeamRole asRole)
	{
		return asRole switch
		{
			TeamRole.Analyst => diceNumber switch
			{
				1 => 1,
				2 => 0,
				3 => 2,
				4 => 1,
				5 => 2,
				6 => 3
			},
			TeamRole.Programmer => diceNumber switch
			{
				1 => 2,
				2 => 1,
				3 => 1,
				4 => 2,
				5 => 3,
				6 => 1
			},
			TeamRole.Tester => diceNumber switch
			{
				1 => 3,
				2 => 4,
				3 => 4,
				4 => 5,
				5 => 5,
				6 => 6
			},
			_ => throw new ArgumentOutOfRangeException(nameof(asRole), asRole, "Unknown role")
		};
	}

	private static void ValidateDiceNumber(int diceNumber)
	{
		if (diceNumber is < 1 or > 6)
		{
			throw new InvalidOperationException(
				$"Invalid {nameof(diceNumber)}:{diceNumber}. Should be between 1 and 6");
		}
	}
}