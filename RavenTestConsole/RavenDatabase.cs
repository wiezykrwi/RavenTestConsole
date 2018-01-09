using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RavenTestConsole
{
	public class RavenDatabase : IRavenDatabase
	{
		public IDocumentStore Store { get; private set; }

		public RavenDatabase()
		{
			Store = new DocumentStore
			{
				Urls = new[] {"http://localhost:8080/"},
				Database = "zoo"    
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