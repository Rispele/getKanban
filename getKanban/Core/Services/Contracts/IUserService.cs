using Core.Dtos;
using Domain.Users;

namespace Core.Services.Contracts;

public interface IUserService
{
	public Task<Guid> CreateNewUser(string name);
	public Task<User?> GetUserById(Guid id);

	public Task<User?> GetUser(UserDto userDto);

	public Task DeleteUserById(Guid id);

	public Task SetUserName(User user, string userName);
}