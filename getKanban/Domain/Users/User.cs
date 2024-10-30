namespace Domain.Users;

public class User
{
	public User(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
	}

	public Guid Id { get; }

	public string Name { get; private set; }
}