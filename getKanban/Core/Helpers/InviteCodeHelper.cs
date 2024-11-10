namespace Core.Helpers;

public class InviteCodeHelper
{
	public static (string sessionId, string teamId) SplitInviteCode(string inviteCode)
	{
		var inviteCodeTokens = inviteCode.Trim().Split("#");
		return (inviteCodeTokens[0], inviteCodeTokens[1]);
	}
}