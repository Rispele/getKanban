using Domain.Serializers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Domain.Game.Days.Configurations;

public class ReadOnlyListConverter<T> : ValueConverter<IReadOnlyList<T>, string>
{
	public ReadOnlyListConverter()
		: base(
			l => l.ToJson(),
			s => s.FromJson<IReadOnlyList<T>>())
	{
	}
}