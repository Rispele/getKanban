using Domain.Game.Days.Scenarios.Services;
using JetBrains.Annotations;

namespace Tests.Domain;

public class TestScenarioService : IScenarioService
{
	[UsedImplicitly]
	public bool IsParamTrue(bool parameter)
	{
		return parameter;
	}

	[UsedImplicitly]
	public bool IsParamsTrue(bool param1, bool param2)
	{
		return param1 && param2;
	}
}