using JetBrains.Annotations;

namespace WebApp.Connection;

public class ConnectionStringProvider
{
	private readonly HttpClient httpClient = new();
	private readonly string secretId;

	public ConnectionStringProvider()
	{
		var token = Environment.GetEnvironmentVariable("IAM_TOKEN")
		         ?? throw new InvalidOperationException("IAM Token is missing");
		httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

		secretId = Environment.GetEnvironmentVariable("CONNECTION_STRING_SECRET_ID")
		        ?? throw new InvalidOperationException("CONNECTION_STRING_SECRET_ID is missing");
	}

	public async Task<string> GetConnectionString()
	{
		var secret = await httpClient.GetFromJsonAsync<YandexLockBoxSecret>(
			$"https://payload.lockbox.api.cloud.yandex.net/lockbox/v1/secrets/{secretId}/payload");
		return secret?.Entries.FirstOrDefault()?.TextValue
		    ?? throw new InvalidOperationException("Failed to get connection string");
	}

	[UsedImplicitly]
	private record YandexLockBoxSecret(YandexLockBoxSecretEntry[] Entries, string versionId);

	[UsedImplicitly]
	private record YandexLockBoxSecretEntry(string Key, string TextValue);
}