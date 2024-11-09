using Core.RequestContexts;
using Microsoft.AspNetCore.SignalR;

namespace WebApp;

public class RequestContextFactory
{
	public static RequestContext Build(HubCallerContext context)
	{
		return Build(context.GetHttpContext()!.Request);
	}
	
	public static RequestContext Build(HttpRequest request)
	{
		var requestContext = new RequestContext();
		var userId = GetUserIdOrThrow(request);
		
		requestContext.AddHeader(RequestContextKeys.UserId, userId.ToString());
		return requestContext;
	}
	
	private static Guid GetUserIdOrThrow(HttpRequest request)
	{
		var userId = request.Cookies["userId"] ?? throw new KeyNotFoundException();
		return Guid.Parse(userId);
	}
}