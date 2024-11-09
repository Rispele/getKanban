namespace Core.Dtos;

public class GameSessionDto
{
	public Guid Id { get; init; }
	
	public string Name { get; init; }

	public ParticipantsDto Angels { get; init; } = null!;

	public IReadOnlyList<TeamDto> Teams { get; init; } = null!;
}