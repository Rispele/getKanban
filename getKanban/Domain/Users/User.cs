using Microsoft.EntityFrameworkCore;

namespace Domain.Users;

[EntityTypeConfiguration(typeof(UserEntityTypeConfiguration))]
public class User
{
	public Guid Id { get; }

	public string Name { get; set; }

	public User(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
	}
}