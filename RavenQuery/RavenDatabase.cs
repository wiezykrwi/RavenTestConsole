using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RavenQuery
{
	public class RavenDatabase : IRavenDatabase
	{
		public IDocumentStore Store { get; private set; }

		public RavenDatabase(string database)
		{
			Store = new DocumentStore
			{
				Urls = new[] {"http://localhost:8080/"},
				Database = database    
			};
			Store.Initialize();
		}

		public void Dispose()
		{
			Store.Dispose();
		}

		public IDocumentSession GetSession()
		{
			return Store.OpenSession();
		}
	}
}