namespace Core.Dtos;

public class AngelsDto
{
	public Guid Id { get; set; }
	
	public ParticipantsDto Participants { get; init; } = null!;
}