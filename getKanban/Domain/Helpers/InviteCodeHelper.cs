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
}