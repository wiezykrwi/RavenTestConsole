using System;
using System.Linq;
using System.Threading;

namespace RavenTestConsole
{
	public static class Program
	{
		//README First create the zoo database in Raven (4.0.0-nightly-20171215-0848), then run the code below
		static void Main(string[] args)
		{
			var fluffy = new Pet { Name = "Fluffy", Age = 2 };
			var john = new Person { Name = "John", Pet = "Pet/2" };
			var spookje = new Pet { Name = "Spookje", Age = 7 };
			var pastis = new Pet { Name = "Pastis", Age = 4 };
			var diggles = new Pet { Name = "Diggles", Age = 2 };
			var mathia = new PetPerson { Name = "Mathia", Pets = new []{ "Pet/1", "Pet/5", "Pet/6" } };
			var jim = new Person { Name = "Jim", Pet = "Pet/3"};
			var jimmy = new Pet { Name = "Jimmy" };
			var jimmy2 = new Pet { Name = "Jimmy2" };

			var database = new RavenDatabase();

			using (var session = database.GetSession())
			{
				session.Store(pastis, "Pet/1");
				session.Store(fluffy, "Pet/2");
				session.Store(john, "Person/1");
				session.Store(mathia, "Person/4");
				session.Store(jim, "Person/3");
				session.Store(jimmy, "Pet/3");
				session.Store(jimmy2, "Pet/4");
				session.Store(spookje, "Pet/5");
				session.Store(diggles, "Pet/6");

				session.SaveChanges();
			}

			var repo = new PersonWithPetsRepository(database);
			repo.DefineIndex();
			var changeRepo = new PersonRepositoryWithChangeNotification(database);
			changeRepo.DefineIndex();
			var petrepo = new PetPersonRepository(database);
			petrepo.DefineIndex();

			Thread.Sleep(1000);

			var test = repo as IPersonWithPetsRepository;
			
			var result = test.GetPersonWithPetsYoungerThan(age: 3);
			Console.WriteLine($"{result.Result.Count} with pets younger than 3");

			var results = repo.GetSuggestionsByName("Jim");
			Console.WriteLine($"Jim heeft {results.Result.Count} resultaten");

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

			var petPerson = petrepo.GetByName("Mathia");
			var person = petPerson.Result.First();
			Console.WriteLine($"{person.Name} has {person.Pets.Length} pets");

			petPerson = petrepo.GetByPetName2("Pastis");
			person = petPerson.Result.First();
			Console.WriteLine($"Pastis is owned by {person.Name}");

			Console.ReadLine();
		}
	}
}