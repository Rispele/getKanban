namespace Core.Dtos;

public class TeamDto
{
	public Guid Id { get; set; }
	
	public string Name { get; init; } = null!;
	
	public ParticipantsDto Participants { get; init; } = null!;
}