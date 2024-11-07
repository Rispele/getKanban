using Domain.Game.Teams;
using Microsoft.AspNetCore.SignalR;

namespace WebApp.Hubs;

public class GameSessionHub : Hub
{
	/// <summary>
	/// Метод используется при подключении к начавшейся игре, чтобы зарегаться в системе
	/// и получить инфу о текущем состоянии игры. Также будем юзать при перезаходе в игру, например если закрыли вкладку
	/// </summary>
	/// <param name="gameSessionId"></param>
	public async Task JoinTeamSession(Guid gameSessionId)
	{
		var userId = GetUserIdOrThrow();

		//TODO: вычисляем teamId по GameSession и userId
		var teamId = Guid.Empty;

		var groupId = GetGroupId(gameSessionId, teamId);
		await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
		await Clients.Caller.SendAsync("UpdateTeamSession"); //TODO: сюды передаем какой-нибудь TeamSessionDto,
																	//в котором храним состояние игры и все выполненные
																	//действия, которые будем строить по AwaitedEvents
	}
	
	/// <summary>
	/// Сэмпл для метода какого-то действия над игровой сессией команды
	/// </summary>
	/// <param name="gameSessionId"></param>
	/// <param name="teamId"></param>
	public async Task RollDiceForAnotherTeam(Guid gameSessionId, Guid teamId)
	{
		var userId = GetUserIdOrThrow();
		
		//TODO: вычисляем team по gameSessionId и teamId 
		Team team = null!;
		
		//TODO: имея на руках Team:
		var actionResult = team.RollDiceForAnotherTeam();
		//TODO: думаю все действия над тимой должны возвращать какой-нибудь actionResult, который будет содержать
		//		тип действия, которое сделали, его результаты или ошибку.
		//		Потом на фронте будут обновлять модельку страницы на основе этих данных.
		
		//Если actionResult содержит ошибку:
		await Clients.Caller.SendAsync("NotifyActionProcessed", actionResult);
		//Если все ок
		await Clients.Group(GetGroupId(gameSessionId, teamId)).SendAsync("NotifyActionProcessed", actionResult);
	}
	
	private string GetUserIdOrThrow() =>
		Context.GetHttpContext()?.Request.Cookies["userId"] ?? throw new NullReferenceException();
	
	private static string GetGroupId(Guid gameSessionId, Guid teamId) => $"gameSession-{gameSessionId}-team-{teamId}";
}