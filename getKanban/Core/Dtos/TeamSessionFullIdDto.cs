namespace Core.Dtos;

public record TeamSessionFullIdDto(Guid GameSessionId, Guid TeamId)
{
	public static TeamSessionFullIdDto Create(Guid gameSessionId, Guid teamId)
	{
		return new TeamSessionFullIdDto(gameSessionId, teamId);
	}
}