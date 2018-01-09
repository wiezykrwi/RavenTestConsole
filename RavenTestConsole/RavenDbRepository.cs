using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations.Indexes;

namespace RavenTestConsole
{
	public abstract class RavenDbRepository<TAggregate, TResult> : AbstractCommonApiForIndexes, IQueryDefiner
	{
		protected readonly IRavenDatabase Database;

		protected RavenDbRepository(IRavenDatabase database)
		{
			Database = database;
		}
		
		public virtual void DefineIndex()
		{
			var builder = new IndexDefinitionBuilder<TAggregate>();

			builder.Map = Index();
			builder.StoresStrings.Add(Constants.Documents.Indexing.Fields.AllFields, FieldStorage.Yes);
			var indexDefinition = builder.ToIndexDefinition(Database.Store.Conventions);
			indexDefinition.Name = IndexName;

			Database.Store.Maintenance.Send(new PutIndexesOperation(indexDefinition));
		}

		public string IndexName => GetType().FullName;

		public abstract Expression<Func<IEnumerable<TAggregate>, IEnumerable>> Index();
		
		protected async Task<IReadOnlyCollection<TResult>> Execute(Expression<Func<TResult, bool>> predicate)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TResult> q = session.Query<TResult>(IndexName)
					.Where(predicate)
					.ProjectInto<TResult>();

				var result = q.ToList();

				return await Task.FromResult(result);
			}
		}
	}
}