using Core.DbContexts.Extensions;
using Core.DbContexts.Helpers;
using Core.Dtos;
using Core.Dtos.Converters;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Users;

namespace Core.Services.Implementations;

public class UserService : IUserService
{
	private readonly DomainContext context;

	public UserService(DomainContext context)
	{
		this.context = context;
	}

	public async Task<Guid> CreateNewUser(string name)
	{
		var user = new User(name);

		context.Users.Add(user);

		await context.TrySaveChangesAsync();

		return user.Id;
	}

	public async Task<UserDto> GetUserById(Guid id)
	{
		var user = await context.GetUserAsync(id);

		return UserDtoConverter.Convert(user);
	}

	public async Task DeleteUserById(Guid id)
	{
		var user = await context.GetUserAsync(id);

		context.Users.Remove(user);
	}

	public async Task SetUserName(Guid userId, string userName)
	{
		var user = await context.GetUserAsync(userId);

		if (user is null)
		{
			throw new NullReferenceException("User does not exist");
		}

		user.Name = userName;

		await context.TrySaveChangesAsync();
	}
}