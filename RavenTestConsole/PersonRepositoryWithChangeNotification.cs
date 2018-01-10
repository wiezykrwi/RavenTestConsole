﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RavenTestConsole
{
	public class PersonRepositoryWithChangeNotification : RavenDbRepositoryWithChangeNotification<Person, Person>
	{
		public PersonRepositoryWithChangeNotification(IRavenDatabase database) : base(database)
		{
		}

		public override Expression<Func<IEnumerable<Person>, IEnumerable>> Index()
		{
			return people => from person in people
				select new
				{
					person.Name
				};
		}

		public Task<IReadOnlyCollection<Person>> PeopleWithName(string name)
		{
			return Execute(x => x.Name == name);
		}

		public Task<IReadOnlyCollection<Person>> PeopleWithNameThatStartsWith(string name)
		{
			return Execute(x => x.Name.StartsWith(name));
		}
	}
}