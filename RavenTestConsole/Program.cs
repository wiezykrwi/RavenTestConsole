using System;
using System.Data.SqlClient;
using System.Security.Cryptography.X509Certificates;

namespace RavenTestConsole
{
	partial class Program
    {
        //README First create the zoo database in Raven (4.0.0-nightly-20171215-0848), then run the code below
        static void Main(string[] args)
        {
            var fluffy = new Pet {Name = "Fluffy", Age = 2};
            var john = new Person {Name = "John", Pet = "Pet/2"};
            var pastis = new Pet {Name = "Pastis", Age = 4};
            var mathia = new Person {Name = "Mathia", Pet = "Pet/1"};


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



	        //var query = repo as IRepositoryWithChangeNotification;

	        //query.AddIndexChangeHandler(() =>
         //   {
         //       var bla = "refresh";
         //   });



            var result = repo.GetPersonWithPetsYoungerThan(age: 3);
         
   
            Console.ReadLine();
        }
	}
}