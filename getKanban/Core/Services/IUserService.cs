namespace Core;

public interface IUserService
{
	public Task<Guid> CreateNewUser(string name);
}