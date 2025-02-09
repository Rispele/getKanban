﻿using Core.Dtos.Containers.TeamMembers;

namespace Core.Dtos.Containers;

public class TeamMemberDto
{
	public long Id { get; init; }
	public TeamRoleDto InitialRole { get; init; }
	public TeamRoleDto CurrentRole { get; init; }
}