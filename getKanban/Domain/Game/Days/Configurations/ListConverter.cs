using Domain.Serializers;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Domain.Game.Days.Configurations;

public class ListConverter<T> : ValueConverter<List<T>, string>
{
	public ListConverter()
		: base(
			l => l.ToJson(),
			s => s.FromJson<List<T>>())
	{
	}
}