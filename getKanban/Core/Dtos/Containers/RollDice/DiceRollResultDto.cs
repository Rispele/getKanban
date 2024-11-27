using Core.Dtos.Containers.TeamMembers;

namespace Core.Dtos.Containers.RollDice;

public record DiceRollResultDto(
	long TeamMemberId,
	TeamRoleDto InitialRole,
	TeamRoleDto CurrentRole,
	int DiceNumber,
	int Scores);