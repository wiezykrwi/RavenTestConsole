using System;
using System.Linq;

namespace RavenTestConsole
{
	public static class Program
	{
		//README First create the zoo database in Raven (4.0.0-nightly-20171215-0848), then run the code below
		static void Main(string[] args)
		{
			var fluffy = new Pet { Name = "Fluffy", Age = 2 };
			var john = new Person { Name = "John", Pet = "Pet/2" };
			var pastis = new Pet { Name = "Pastis", Age = 4 };
			var mathia = new Person { Name = "Mathia", Pet = "Pet/1" };

			var database = new RavenDatabase();

			using (var session = database.GetSession())
			{
				session.Store(pastis, "Pet/1");
				session.Store(fluffy, "Pet/2");
				session.Store(john, "Person/1");
				session.Store(mathia, "Person/2");

				session.SaveChanges();
			}

			var repo = new PersonWithPetsRepository(database);

			var indexDefiner = repo as IQueryDefiner;

			indexDefiner.DefineIndex();
			
			var result = repo.GetPersonWithPetsYoungerThan(age: 3);
			Console.WriteLine($"{result.Result.Count} with pets younger than 3");

			var changeRepo = new PersonRepositoryWithChangeNotification(database);
			changeRepo.DefineIndex();
			changeRepo.AddIndexChangeHandler(() =>
			{
				Console.WriteLine("Person updated");
			});

			var personResult = changeRepo.PeopleWithName("Mathia");
			var temp = personResult.Result.Any() ? "" : "not";
			Console.WriteLine($"person was {temp} found");

			var personResult2 = changeRepo.PeopleWithNameThatStartsWith("Mat");
			var temp2 = personResult2.Result.Any() ? "" : "not";
			Console.WriteLine($"person was {temp2} found");

			Console.ReadLine();
		}
	}
}