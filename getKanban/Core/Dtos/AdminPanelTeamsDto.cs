namespace Core.Dtos;

public class AdminPanelTeamsDto
{
	public Guid GameSessionId { get; set; }
	
	public string GameSessionName { get; set; } = null!;
	
	public List<TeamDto> Teams { get; init; } = null!;
}