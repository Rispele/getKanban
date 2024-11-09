namespace Core.Dtos;

public class ParticipantsDto
{
	public string? InviteCode { get; init; }
	
	public IReadOnlyList<UserDto> Users { get; init; }
}