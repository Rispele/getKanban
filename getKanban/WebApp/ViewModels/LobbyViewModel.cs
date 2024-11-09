using Core.Dtos;
using Domain.Game.Teams;

namespace WebApp.ViewModels;

public class LobbyViewModel
{
	public string GameTitle { get; init; } = null!;
	public IReadOnlyList<TeamDto> Teams { get; init; } = null!;
}