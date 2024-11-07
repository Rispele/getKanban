using Newtonsoft.Json;

namespace Domain.Serializers;

public static class SerializerExtensions
{
	private static readonly JsonSerializerSettings Settings = new()
	{
		TypeNameHandling = TypeNameHandling.Objects
	};

	public static string ToJson<T>(this T value)
	{
		return JsonConvert.SerializeObject(value, Settings);
	}

	public static T FromJson<T>(this string value)
	{
		return JsonConvert.DeserializeObject<T>(value, Settings) ??
		       throw new InvalidOperationException("Couldn't deserialize value");
	}
}