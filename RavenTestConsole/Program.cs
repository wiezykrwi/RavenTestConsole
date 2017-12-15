using System.Linq;
using System.Threading;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;

namespace RavenTestConsole
{
    class Program
    {
        //README First create the zoo database in Raven (4.0.0-nightly-20171215-0848), then run the code below
        static void Main(string[] args)
        {
            var fluffy = new Pet {Name = "Fluffy", Age = 2};
            var john = new Person {Name = "John", Pet = "Pet/2"};
            var pastis = new Pet {Name = "Pastis", Age = 4};
            var mathia = new Person {Name = "Mathia", Pet = "Pet/1"};

            using (var store = new DocumentStore
            {
                Urls = new[] {"http://localhost:8080/"},
                Database = "zoo"
            })
            {
                store.Initialize();
                store.ExecuteIndex(new PersonWithPetsAndAgeIndex());

                Thread.Sleep(5000); // wait for index

                using (var session = store.OpenSession())
                {
                    session.Store(pastis, "Pet/1");
                    session.Store(fluffy, "Pet/2");
                    session.Store(john);
                    session.Store(mathia);

                    session.SaveChanges();
                }

                // This query execution does not give me the stored index json but the actual Person docs => results in not mapped properties
                using (var session = store.OpenSession())
                {
                    var q = session.Query<PersonWithPetsAndAgeIndex.Result, PersonWithPetsAndAgeIndex>()
                        .Where(r => r.PetAge > 3);
                    var
                        res = q
                            .ToList(); // how can I get a list of PersonWithPetsAndAgeIndex.Result with all properties filled in
                }
            }
        }
    }

    class PersonWithPetsAndAgeIndex : AbstractIndexCreationTask<Person>
    {
        public class Result
        {
            public string PersonName { get; set; }
            public string PetName { get; set; }
            public int PetAge { get; set; }
        }

        public PersonWithPetsAndAgeIndex()
        {
            Map = people => from person in people
                let pet = LoadDocument<Pet>(person.Pet)
                select new Result {PersonName = person.Name, PetName = pet.Name, PetAge = pet.Age};
            StoreAllFields(FieldStorage.Yes);
        }
    }

    class Person
    {
        public string Name { get; set; }
        public string Pet { get; set; }
    }

    class Pet
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}