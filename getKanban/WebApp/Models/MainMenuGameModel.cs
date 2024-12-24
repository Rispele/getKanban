using Domain.Game;

namespace WebApp.Models;

public class MainMenuGameModel
{
	public Guid GameSessionId { get; set; }
	public Guid TeamId { get; set; }
	public string GameSessionName { get; set; }
	public int TeamsCount { get; set; }
	public ParticipantRole ParticipantRole { get; set; }
	public string GameSessionStatus { get; set; }
}