namespace Core.RequestContexts;

public class RequestContext
{
	private readonly Dictionary<string, string> headers = new();

	public IReadOnlyDictionary<string, string> Headers => headers;

	public void AddHeader(string key, string value)
	{
		headers[key] = value;
	}

	public Guid GetUserId()
	{
		return FindUserId() ?? throw new KeyNotFoundException();
	}
	
	public Guid? FindUserId()
	{
		if (headers.TryGetValue(RequestContextKeys.UserId, out var header))
		{
			return Guid.Parse(header);
		}
		
		return null;
	}
}