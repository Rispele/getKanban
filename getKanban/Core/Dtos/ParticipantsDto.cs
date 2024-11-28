namespace Core.Dtos;

public class ParticipantsDto
{
	public string? InviteCode { get; init; }

	public IReadOnlyList<UserDto> Users { get; init; }

	public ParticipantsDto(string? inviteCode, IReadOnlyList<UserDto> users)
	{
		InviteCode = inviteCode;
		Users = users ?? throw new ArgumentNullException(nameof(users));
	}
}