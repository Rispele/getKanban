using Core.Dtos;

namespace Core;

public record AddParticipantResult(
	bool Updated,
	GameSessionDto GameSession,
	Guid UpdatedTeamId,
	UserDto User);