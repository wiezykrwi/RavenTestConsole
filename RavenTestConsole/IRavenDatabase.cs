using System;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace RavenTestConsole
{
    public interface IRavenDatabase : IDisposable
    {
      
        IDocumentSession GetSession();
        IDocumentStore Store { get; }
    }
}
