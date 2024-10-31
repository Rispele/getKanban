using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Domain.Game.Days.DayEvents.DayContainers.Configurations;

public class ReadOnlyListConverter<T> : ValueConverter<IReadOnlyList<T>, string>
{
	public ReadOnlyListConverter()
		: base(
			l => JsonConvert.SerializeObject(l),
			s => JsonConvert.DeserializeObject<IReadOnlyList<T>>(s)!)
	{
	}
}