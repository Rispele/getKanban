using Domain.Game;

namespace Core.Dtos;

public class GameSessionDto
{
	public Guid Id { get; init; }
	
	public string Name { get; init; }

	public TeamDto Angels { get; init; } = null!;

	public IReadOnlyList<TeamDto> Teams { get; init; } = null!;

	public ParticipantRole RequesterRole { get; init; }

	public Guid RequesterId { get; set; }

	public bool IsRecruitmentFinished = false;
}