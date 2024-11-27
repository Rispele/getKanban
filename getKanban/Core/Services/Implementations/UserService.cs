using Core.DbContexts;
using Core.Dtos;
using Core.Services.Contracts;
using Domain.DbContexts;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

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
		
		await context.SaveChangesAsync();
		
		return user.Id;
	}

	public async Task<User?> GetUserById(Guid id)
	{
		return await context.Users.FirstOrDefaultAsync(x => x.Id == id);
	}

	public async Task<User?> GetUser(UserDto userDto)
	{
		var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userDto.Id && x.Name == userDto.Name);
		return user;
	}

	public async Task DeleteUserById(Guid id)
	{
		var user = await GetUserById(id);
		if (user != null)
		{
			context.Users.Remove(user);
		}
	}

	public async Task SetUserName(Guid userId, string userName)
	{
		var user = await GetUserById(userId);
		if (user != null)
		{
			user.Name = userName;
		}

		await context.SaveChangesAsync();
	}
}