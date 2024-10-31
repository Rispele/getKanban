namespace Domain.Users;

public class User
{
	public Guid Id { get; }

	public string Name { get; private set; }

	public User(string name)
	{
		Id = Guid.NewGuid();
		Name = name;
	}
}