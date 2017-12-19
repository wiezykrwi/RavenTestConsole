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

                Thread.Sleep(2000); // wait for index

                using (var session = store.OpenSession())
                {
                    session.Store(pastis, "Pet/1");
                    session.Store(fluffy, "Pet/2");
                    session.Store(john, "Person/1");
                    session.Store(mathia, "Person/2");

                    session.SaveChanges();
                }
                Thread.Sleep(2000); // wait for index

                // This query execution does not give me the stored index json but the actual Person docs => results in not mapped properties
                using (var session = store.OpenSession())
                {
                    var q = session.Query<PersonWithPetsAndAgeIndex.Result, PersonWithPetsAndAgeIndex>()
                        .Where(r => r.PetAge > 3)
                        .ProjectInto<PersonWithPetsAndAgeIndex.Result>();
                    // q = {FROM INDEX 'PersonWithPetsAndAgeIndex' WHERE PetAge > $p0} 
                    // in raven studio using this and setting take fields from index gives me the correct json
                    q = q.Customize(x => x.AfterQueryExecuted(y =>
                    {
                        var bla = y;
                    }));
                    var
                        res = q
                            .ToList(); // how can I get a list of PersonWithPetsAndAgeIndex.Result with all properties filled in
                    
                }

                // This query gives me the expected result, but is very hacky
                using (var session = store.OpenSession())
                {
                    var q = session.Query<PersonWithPetsAndAgeIndex.Result, PersonWithPetsAndAgeIndex>() // need to query on the projected type to use the PetAge prop 
                        .Where(r => r.PetAge > 3)
                        .ProjectInto<Person>()
                        .Select(person => new PersonWithPetsAndAgeIndex.Result
                        {
                            PetAge = session.Load<Pet>(person.Pet).Age,
                            PetName = session.Load<Pet>(person.Pet).Name,
                            PersonName = person.Name
                        });
                    // q = {FROM INDEX 'PersonWithPetsAndAgeIndex' as person WHERE person.PetAge > $p0 SELECT { PetAge : load(person.Pet).Age, PetName : load(person.Pet).Name, PersonName : person.Name }}
                    // Same output as the above, but why should I do this complex mapping if the index has already done this? (I Cannot convince my co-workers to do it like this, and I don't blame them. ) 
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