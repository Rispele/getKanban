using Core.Dtos;

namespace Core.Services;

public record AddParticipantResult(
	bool Updated,
	GameSessionDto GameSession,
	Guid UpdatedTeamId,
	UserDto User);