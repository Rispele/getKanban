using Core.RequestContexts;
using Microsoft.AspNetCore.SignalR;

namespace WebApp;

public class RequestContextFactory
{
	public static RequestContext Build(HubCallerContext context)
	{
		return Build(context.GetHttpContext()!.Request);
	}

	public static bool TryBuild(HttpRequest request, out RequestContext? requestContext)
	{
		var userId = FindRequestUserId(request);

		if (userId is null)
		{
			requestContext = null;
			return false;
		}
		
		requestContext = new RequestContext();
		requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());
		return true;
	}
	
	public static RequestContext Build(HttpRequest request)
	{
		var requestContext = new RequestContext();
		var userId = GetUserIdOrThrow(request);
		requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());
		
		var sessionId = TryGetSessionId(request);
		if (sessionId is not null)
		{
			requestContext.AddHeader(RequestContextKeys.SessionId, sessionId.ToString());
		}
		
		return requestContext;
	}
	
	private static Guid GetUserIdOrThrow(HttpRequest request)
	{
		return FindRequestUserId(request) ?? throw new KeyNotFoundException();
	}
	
	private static Guid? TryGetSessionId(HttpRequest request)
	{
		return FindRequestSessionId(request);
	}
	
	private static Guid? FindRequestUserId(HttpRequest request)
	{
		var value = request.Cookies[RequestContextKeys.UserId];
		return value is null
			? null
			: Guid.Parse(value);
	}

	private static Guid? FindRequestSessionId(HttpRequest request)
	{
		var value = request.Cookies[RequestContextKeys.SessionId];
		return value is null
			? null
			: Guid.Parse(value);
	}
}