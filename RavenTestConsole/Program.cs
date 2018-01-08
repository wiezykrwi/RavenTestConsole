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


            var database = new RavenDatabase();

            using (var session = database.GetSession())
            {
                session.Store(pastis, "Pet/1");
                session.Store(fluffy, "Pet/2");
                session.Store(john, "Person/1");
                session.Store(mathia, "Person/2");

                session.SaveChanges();
            }

            var query = new QueryPersonWithPetsYoungerThan(database);

            query.DefineQuery();

            query.Prepare(age: 5);

            var result = query.Execute().Result;
        }

        class QueryPersonWithPetsYoungerThan : RavenDbQuery<Person, PersonWithPetsAndAge>, IGetPersonWithPetsYoungerThan
        {
            public override Expression<Func<IEnumerable<Person>, IEnumerable>> Index()
            {
                return people => from person in people
                    let pet = LoadDocument<Pet>(person.Pet)
                    select new PersonWithPetsAndAge {PersonName = person.Name, PetName = pet.Name, PetAge = pet.Age};
            }

            public void Prepare(int age)
            {
                Where(x => x.PetAge < age);
            }

            public QueryPersonWithPetsYoungerThan(IRavenDatabase db) : base(db)
            {
            }
        }

        internal interface IGetPersonWithPetsYoungerThan : IQuery<PersonWithPetsAndAge>
        {
            void Prepare(int age);
        }


        public abstract class RavenDbQuery<TAggregate, TResult> :
            AbstractCommonApiForIndexes,
            IQueryDefiner,
            IQuery<TResult>
        {
            private readonly IRavenDatabase _db;

            protected RavenDbQuery(IRavenDatabase db)
            {
                _db = db;
            }

            private readonly List<Expression<Func<TResult, bool>>> _wheres = new List<Expression<Func<TResult, bool>>>();

            public void DefineQuery()
            {
                var builder = new IndexDefinitionBuilder<TAggregate>();

                builder.Map = Index();
                builder.StoresStrings.Add(Constants.Documents.Indexing.Fields.AllFields, FieldStorage.Yes);
                var indexDefinition = builder.ToIndexDefinition(_db.Store.Conventions);
                indexDefinition.Name = IndexName;
               
                _db.Store.Maintenance.Send(new PutIndexesOperation(indexDefinition));

            }

            public string IndexName => GetType().FullName;

            public abstract Expression<Func<IEnumerable<TAggregate>, IEnumerable>> Index();

            public void Where(Expression<Func<TResult, bool>> predicate)
            {
                _wheres.Add(predicate);
            }

            public async Task<IReadOnlyCollection<TResult>> Execute()
            {
                using (var session = _db.GetSession())
                {
                    IRavenQueryable<TResult> q = session.Query<TResult>(IndexName);
                    foreach (var where in _wheres)
                    {
                        q = q.Where(where);
                    }
                    var result = q.ProjectInto<TResult>().ToList();

                    var r = await Task.FromResult(result);
                    return r;
                }
            }
        }

        public interface IQueryDefiner
        {
            void DefineQuery();
        }

        public interface IQuery<TResult>
        {
            Task<IReadOnlyCollection<TResult>> Execute();
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
}