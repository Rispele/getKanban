namespace Core.Helpers;

public class InviteCodeHelper
{
	public static (Guid sessionId, Guid teamId) SplitInviteCode(string inviteCode)
	{
		var inviteCodeTokens = inviteCode.Trim().Split(".");
		return (Guid.Parse(inviteCodeTokens[0]), Guid.Parse(inviteCodeTokens[1]));
	}

	public static string ConcatInviteCode(Guid sessionId, Guid teamId)
	{
		var inviteCode = $"{sessionId}.{teamId}";
		return inviteCode;
	}
	
	public static Guid ResolveGameSessionId(string inviteCode)
	{
		var (gameSessionId, _) = SplitInviteCode(inviteCode);
		return gameSessionId;
	}
	
	public static Guid ResolveTeamId(string inviteCode)
	{
		var (_, teamId) = SplitInviteCode(inviteCode);
		return teamId;
	}
}