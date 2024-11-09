using Core.DbContexts;
using Domain.Users;

namespace Core;

public class UserService : IUserService
{
	private UsersContext context;

	public UserService(UsersContext context)
	{
		this.context = context;
	}
	
	public async Task<Guid> CreateNewUser(string name)
	{
		var user = new User(name);
		context.User.Add(user);
		await context.SaveChangesAsync();
		return user.Id;
	}
}