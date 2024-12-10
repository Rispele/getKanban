using Microsoft.EntityFrameworkCore;

namespace Core.DbContexts.Helpers;

public static class DbContextHelper
{
	public static async Task<int> TrySaveChangesAsync(this DbContext context, int triesCount = 3)
	{
		var tryNumber = 1;
		while (true)
		{
			try
			{
				return await context.SaveChangesAsync();
			}
			catch (Exception exception) when (exception is DbUpdateConcurrencyException or DbUpdateException)
			{
				if (tryNumber >= triesCount)
				{
					throw;
				}

				tryNumber++;
			}
		}
	}
}