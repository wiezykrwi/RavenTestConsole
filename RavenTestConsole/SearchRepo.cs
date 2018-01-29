using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Raven.Client.Documents.Indexes;

using RavenQuery;

namespace RavenTestConsole
{
	public class SearchRepo : RavenDbRepository<Person, Person>
	{
		public SearchRepo(IRavenDatabase database) : base(database)
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

		protected override void IndexFields(IDictionary<Expression<Func<Person, object>>, FieldIndexing> builder)
		{
			builder.Add(p => p.Name, FieldIndexing.Search);
		}

		public Task<IReadOnlyCollection<Person>> Test(string name)
		{
			return ExecuteSearch(p => p.Name, name);
		}
	}
}