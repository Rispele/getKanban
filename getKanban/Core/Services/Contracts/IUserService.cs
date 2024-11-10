namespace Core.Services.Contracts;

public interface IUserService
{
	public Task<Guid> CreateNewUser(string name);
}