using Domain.Game;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[Route("team-selection")]
public class TeamSelectionController : Controller
{
	public TeamSelectionController()
	{
		
	}
	
	[HttpGet]
	[Route(nameof(Roll))]
	public int Roll()
	{
		return new DiceRoller(new Random()).RollDice();
	}

	[HttpGet]
	[Route(nameof(Add))]
	public string Add(int count)
	{
		var result = new List<string>();
		Enumerable.Range(1, count).ToList().ForEach(x => result.Add($"Team {x}"));
		return string.Join('\n', result.ToArray());
	}
}