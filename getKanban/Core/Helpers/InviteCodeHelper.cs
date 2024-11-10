namespace Core.Helpers;

public class InviteCodeHelper
{
	public Guid ResolveGameSessionId(string inviteCode)
	{
		var (gameSessionId, _) = SplitInviteCode(inviteCode);
		return Guid.Parse(gameSessionId);
	}
	
	private static (string sessionId, string teamId) SplitInviteCode(string inviteCode)
	{
		var inviteCodeTokens = inviteCode.Trim().Split("#");
		return (inviteCodeTokens[0], inviteCodeTokens[1]);
	}
}