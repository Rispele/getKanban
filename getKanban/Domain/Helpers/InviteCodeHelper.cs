using System.Text.RegularExpressions;

namespace Core.Helpers;

public static class InviteCodeHelper
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

	public static bool ValidateInviteCode(string inviteCode)
	{
		const string pattern = """
		                       ^([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})\.([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})$
		                       """;
		return Regex.IsMatch(inviteCode, pattern);
	}
}