using Core.Dtos;

namespace Core.Services.Contracts;

public interface IUserService
{
	public Task<Guid> CreateNewUser(string name);

	public Task<UserDto> GetUserById(Guid id);

	public Task SetUserName(Guid userId, string userName);
}