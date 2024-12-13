﻿using Core.DbContexts;
using Domain;
using Domain.DbContexts;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Tests.DbContexts;

[TestFixture]
public class Connection_Test
{
	[Test]
	public void Connect_EnsureCreate_ShouldSuccessfullyCreate()
	{
		var db = new DomainContext();
		db.Database.EnsureCreated();
	}

	[Test]
	public void Remove_Created()
	{
		var db = new DomainContext();
		db.Model
			.GetEntityTypes()
			.Select(t => t.GetTableName()!)
			.Select(DropTableSql)
			.ForEach(
				sql =>
				{
					try
					{
						db.Database.ExecuteSqlRaw(sql);
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
					}
				});

		string DropTableSql(string tableName)
		{
			return $"""DROP TABLE public."{tableName}" CASCADE;""";
		}
	}
}