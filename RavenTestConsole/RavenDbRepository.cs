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
using Raven.Client.Documents.Queries.Suggestions;
using Raven.Client.Documents.Session;

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

		protected async Task<IReadOnlyCollection<TResult>> ExecuteQuery(Func<IRavenQueryable<TResult>, IRavenQueryable<TResult>> predicate)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TResult> query = session.Query<TResult>(IndexName)
					.ProjectInto<TResult>();

				query = predicate(query);

				var result = query.ToList();

				return await Task.FromResult(result);
			}
		}

		protected async Task<Dictionary<string, SuggestionResult>> ExecuteSuggestionsQuery(Func<IRavenQueryable<TResult>, ISuggestionQuery<TResult>> predicate)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TResult> query = session.Query<TResult>(IndexName);

				var suggestionsQuery = predicate(query);

				var result = await suggestionsQuery.ExecuteAsync();

				return await Task.FromResult(result);
			}
		}

		protected async Task<IReadOnlyCollection<TResult>> ExecuteSearch(Expression<Func<TResult, object>> selector, string values)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TResult> q = session.Query<TResult>(IndexName)
					.Search(selector, values)
					.ProjectInto<TResult>();

				var result = q.ToList();

				return await Task.FromResult(result);
			}
		}

		protected ReeferQuery Query()
		{
			var session = Database.GetSession();
			var query = session.Query<TResult>(IndexName).ProjectInto<TResult>();

			return new ReeferQuery
			{
				Session = session,
				Query = query
			};
		}

		protected class ReeferQuery : IDisposable
		{
			public IDocumentSession Session { get; set; }
			public IRavenQueryable<TResult> Query { get; set; }

			public void Dispose()
			{
				Session.Dispose();
			}
		}
	}
	public abstract class RavenDbRepository<TAggregate, TResult, TQuery> : AbstractCommonApiForIndexes, IQueryDefiner
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

		protected async Task<IReadOnlyCollection<TResult>> Execute(Expression<Func<TQuery, bool>> predicate)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TResult> q = session.Query<TQuery>(IndexName)
					.Where(predicate)
					.ProjectInto<TResult>();

				var result = q.ToList();

				return await Task.FromResult(result);
			}
		}

		protected async Task<IReadOnlyCollection<TResult>> ExecuteQuery(Func<IRavenQueryable<TQuery>, IRavenQueryable<TQuery>> predicate)
		{
			using (var session = Database.GetSession())
			{
				IRavenQueryable<TQuery> query = session.Query<TQuery>(IndexName);

				query = predicate(query);
				var resultQuery = query.ProjectInto<TResult>();

				var result = resultQuery.ToList();

				return await Task.FromResult(result);
			}
		}

		//protected async Task<Dictionary<string, SuggestionResult>> ExecuteSuggestionsQuery(Func<IRavenQueryable<TResult>, ISuggestionQuery<TResult>> predicate)
		//{
		//	using (var session = Database.GetSession())
		//	{
		//		IRavenQueryable<TResult> query = session.Query<TResult>(IndexName);

		//		var suggestionsQuery = predicate(query);

		//		var result = await suggestionsQuery.ExecuteAsync();

		//		return await Task.FromResult(result);
		//	}
		//}

		//protected async Task<IReadOnlyCollection<TResult>> ExecuteSearch(Expression<Func<TResult, object>> selector, string values)
		//{
		//	using (var session = Database.GetSession())
		//	{
		//		IRavenQueryable<TResult> q = session.Query<TResult>(IndexName)
		//			.Search(selector, values)
		//			.ProjectInto<TResult>();

		//		var result = q.ToList();

		//		return await Task.FromResult(result);
		//	}
		//}

		//protected ReeferQuery Query()
		//{
		//	var session = Database.GetSession();
		//	var query = session.Query<TResult>(IndexName).ProjectInto<TResult>();

		//	return new ReeferQuery
		//	{
		//		Session = session,
		//		Query = query
		//	};
		//}

		//protected class ReeferQuery : IDisposable
		//{
		//	public IDocumentSession Session { get; set; }
		//	public IRavenQueryable<TResult> Query { get; set; }

		//	public void Dispose()
		//	{
		//		Session.Dispose();
		//	}
		//}
	}
}