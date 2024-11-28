using Domain.Users;

namespace Core.Dtos.Converters;

public static class UserDtoConverter
{
	public static UserDto Convert(User user)
	{
		return new UserDto
		{
			Id = user.Id,
			Name = user.Name,
		};
	}
}