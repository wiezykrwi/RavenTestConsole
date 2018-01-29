using System;
using System.Reflection;
using System.Threading;

using Raven.Client.Documents.Indexes;

using RavenQuery;

namespace Multimap
{
	class Program
	{
		static void Main(string[] args)
		{
			var database = new RavenDatabase("multimap");
			IndexCreation.CreateIndexes(Assembly.GetEntryAssembly(), database.Store);

			SeedData(database);
			Console.WriteLine($"data seeded");
			Console.ReadKey(true);

			var repo = new MessageRepository(database);

			Thread.Sleep(500);

			var result = repo.GetAllValidMessages();

			Console.WriteLine($"Found {result.Result.Count} messages");
			Console.ReadKey(true);
		}

		private static void SeedData(RavenDatabase database)
		{
			using (var session = database.GetSession())
			{
				session.Store(new Iftmbc
				{
					Id = "Iftmbc/1",
					BookingNumber = "Booking/1"
				});
				session.Store(new Iftmbc
				{
					Id = "Iftmbc/2",
					BookingNumber = "Booking/2"
				});
				session.Store(new Iftmbc
				{
					Id = "Iftmbc/3",
					BookingNumber = "Booking/3"
				});

				session.Store(new ThreeFifteen
				{
					Id = "ThreeFifteen/1",
					ContainerNumber = "Container/1"
				});
				session.Store(new ThreeFifteen
				{
					Id = "ThreeFifteen/2",
					ContainerNumber = "Container/2"
				});
				session.Store(new ThreeFifteen
				{
					Id = "ThreeFifteen/3",
					ContainerNumber = "Container/3"
				});
				session.Store(new ThreeFifteen
				{
					Id = "ThreeFifteen/4",
					ContainerNumber = "Container/4"
				});
				session.Store(new ThreeFifteen
				{
					Id = "ThreeFifteen/5",
					ContainerNumber = "Container/5"
				});

				session.SaveChanges();
			}
		}
	}

	public interface IMessageBase
	{
		string Id { get; set; }
		bool IsValid { get; set; }
	}
	public abstract class MessageBase : IMessageBase
	{
		public string Id { get; set; }
		public bool IsValid { get; set; }
	}

	class Iftmbc : MessageBase
	{
		public string BookingNumber { get; set; }
	}

	class ThreeFifteen : MessageBase
	{
		public string ContainerNumber { get; set; }
	}
}
