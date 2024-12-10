using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts.Helpers;

public static class DbContextHelper
{
	public static async Task<int> TrySaveChangesAsync(this DbContext context, int tryCount = 3)
	{
		var result = 0;
		var retriesRemaining = tryCount;
		var success = false;
		do
		{
			try
			{
				result = await context.SaveChangesAsync();
				success = true;
			}
			catch (Exception e) when
				(e is DbUpdateConcurrencyException or DbUpdateException)
			{
				var operationTypeName = e switch
				{
					DbUpdateConcurrencyException => "Concurrent DB update",
					DbUpdateException => "DB update",
					_ => string.Empty
				};
				if (retriesRemaining == tryCount)
				{
					Console.WriteLine($"{operationTypeName} failed: {e.Message}");
				}
				if (retriesRemaining == 0)
				{
					Console.WriteLine($"{operationTypeName} failed after {tryCount} tries.");
					throw;
				}
				
				Console.WriteLine($"{operationTypeName} retry: {tryCount - retriesRemaining + 1} try.");
			}
		} while (!success || retriesRemaining-- != 0);

		return result;
	}
}