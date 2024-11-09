namespace Core.Services;

public interface IUserService
{
	public Task<Guid> CreateNewUser(string name);
}