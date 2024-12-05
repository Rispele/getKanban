using Domain.Game.Days;
using Domain.Game.Days.Scenarios;
using Domain.Game.Teams;
using Domain.Game.Tickets;

namespace Domain.Services;

public class TeamSessionPatchService
{
	public async Task AddDay(Team team)
	{
		var dayNumber = team.CurrentDay.Number + 1;
		var settings = team.Settings;
		var daysToProcess = team.Days;
		var releasedTickets = team.GetReleasedTicketIds(daysToProcess);
		
		var endOfReleaseCycle = dayNumber % settings.ReleaseCycleLength == 0;
		var autoReleaseTicketDone = releasedTickets.Contains(TicketDescriptors.AutoRelease.Id);
		
		var shouldRelease = endOfReleaseCycle || autoReleaseTicketDone;
		var shouldUpdateSpringBacklog = endOfReleaseCycle || dayNumber >= settings.UpdateSprintBacklogEveryDaySince;
		var anotherTeamAppeared = dayNumber >= settings.AnotherTeamShouldWorkSince
		                       && team.BuildAnotherTeamScores() < settings.ScoresAnotherTeamShouldGain;

		var scenario = ScenarioBuilder.Create()
			.DefaultScenario(anotherTeamAppeared, shouldRelease, shouldUpdateSpringBacklog)
			.Build();

		var testersNumber = dayNumber >= settings.IncreaseTestersNumberSince
			? settings.IncreasedTestersNumber
			: settings.DefaultTestersNumber;

		var daySettings = new DaySettings
		{
			Number = dayNumber,

			AnalystsCount = settings.AnalystsNumber,
			ProgrammersCount = settings.ProgrammersNumber,
			TestersCount = testersNumber,

			ProfitPerClient = settings.GetProfitPerDay(dayNumber)
		};
	}
}