using Newtonsoft.Json;

namespace Domain.Serializers;

public static class SerializerExtensions
{
	private static readonly JsonSerializerSettings Settings = new()
	{
		TypeNameHandling = TypeNameHandling.Objects
	};

	public static List<T> WriteLine<T>(this List<T> l)
	{
		Console.WriteLine(string.Join(',', l));
		return l;
	}

	public static string WriteLine(this string l)
	{
		Console.WriteLine(l);
		return l;
	}

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