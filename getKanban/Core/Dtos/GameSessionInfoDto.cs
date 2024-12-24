using Domain.Game;

namespace Core.Dtos;

public class GameSessionInfoDto
{
	public Guid GameSessionId { get; set; }
	public Guid TeamId { get; set; }
	public string GameSessionName { get; set; }
	public int TeamsCount { get; set; }
	public ParticipantRole RequesterParticipantRole { get; set; }
	public string GameSessionStatus { get; set; }
}