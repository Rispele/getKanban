using Domain.Game.Teams;

namespace WebApp.ViewModels;

public class LobbyViewModel
{
	public string GameTitle { get; set; }
	public IEnumerable<Team> Teams { get; set; }
}