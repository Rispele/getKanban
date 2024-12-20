using Domain.Game;

namespace WebApp.Models;

public class MainMenuGameModel
{
	public string GameSessionName { get; set; }
	public int TeamsCount { get; set; }
	public ParticipantRole ParticipantRole { get; set; }
	public string GameSessionStatus { get; set; }
}