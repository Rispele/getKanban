namespace Domain;

public static class ObjectExtensions
{
	public static bool IsNullOrEmpty<T>(this IEnumerable<T>? value)
	{
		return value is null || !value.Any();
	}
}