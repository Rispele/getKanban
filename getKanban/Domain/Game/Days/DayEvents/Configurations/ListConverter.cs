using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Domain.Game.Days.DayEvents.Configurations;

public class ListConverter<T> : ValueConverter<List<T>, string>
{
	public ListConverter()
		: base(
			l => JsonConvert.SerializeObject(l),
			s => JsonConvert.DeserializeObject<List<T>>(s)!)
	{
	}
}