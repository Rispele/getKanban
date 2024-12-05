namespace Core.Dtos;

public class UserCredentialsDto
{
	public Guid SessionId { get; set; }

	public Guid TeamId { get; set; }

	public Guid UserId { get; set; }
}