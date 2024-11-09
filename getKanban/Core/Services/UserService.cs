using Core.DbContexts;
using Domain.Users;

namespace Core.Services;

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
}