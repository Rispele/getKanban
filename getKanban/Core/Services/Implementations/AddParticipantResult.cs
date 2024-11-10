using Core.Dtos;

namespace Core.Services.Implementations;

public record AddParticipantResult(
	bool Updated,
	GameSessionDto GameSession,
	Guid UpdatedTeamId,
	UserDto User);