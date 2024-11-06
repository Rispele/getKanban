using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class LobbyHub : Hub
{
	public async Task Join(Guid gameSessionId, Guid? teamId)
	{
		var userId = GetUserIdOrThrow();

		//TODO: валидируем что пользователь действительно есть в комманде или в ангелах

		var groupId = GetGroupId(gameSessionId);
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
		await Clients.OthersInGroup(groupId).SendCoreAsync("NotifyJoined", [teamId, userId]);
	}

	public async Task StartGame(Guid gameSessionId)
	{
		var userId = GetUserIdOrThrow();
		
		//TODO: проверяем, что пользователь - создатель игры
	
		var groupId = GetGroupId(gameSessionId);
		await Clients.Group(groupId).SendAsync("NotifyStarted");
	}

	private string GetUserIdOrThrow() =>
		Context.GetHttpContext()?.Request.Cookies["userId"] ?? throw new NullReferenceException();

	private static string GetGroupId(Guid gameSessionId) => $"lobby-{gameSessionId}";
}