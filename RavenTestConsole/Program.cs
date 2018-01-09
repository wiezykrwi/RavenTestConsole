using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
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

            var repo = new PersonWithPetsRepository(database);

            repo.DefineIndex();
            repo.AddIndexChangeHandler(() =>
            {
                var bla = "refresh";
            });
           var result = repo.GetPersonWithPetsYoungerThan(age: 3);
         
   
            Console.ReadLine();
        }

        class PersonWithPetsRepository : RavenDbRepository<Person, PersonWithPetsAndAge>, IPersonWithPetsRepository
        {
            public override Expression<Func<IEnumerable<Person>, IEnumerable>> Index()
            {
                return people => from person in people
                    let pet = LoadDocument<Pet>(person.Pet)
                    select new PersonWithPetsAndAge {PersonName = person.Name, PetName = pet.Name, PetAge = pet.Age};
            }
            public PersonWithPetsRepository(IRavenDatabase db) : base(db)
            {
            }

            public Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThan(int age)
            {
                
                Where(x => x.PetAge < age);
                return Execute();
            }
        }

        internal interface IPersonWithPetsRepository : IRepository
        {
            Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThan(int age);
        }

      
        public abstract class RavenDbRepository<TAggregate, TResult> :
            AbstractCommonApiForIndexes,
            IQueryDefiner,
            IRepository
        {
            private class IndexObserver : IObserver<IndexChange>
            {
                private readonly Action<IndexChange> _onNext;

                public IndexObserver(Action<IndexChange> onNext)
                {
                    _onNext = onNext;
                }

                public void OnCompleted()
                {
                   
                }

                public void OnError(Exception error)
                {
                   
                }

                public void OnNext(IndexChange value)
                {
                    _onNext(value);
                }
            }

            private readonly IRavenDatabase _db;

            protected RavenDbRepository(IRavenDatabase db)
            {
                _db = db;
                _indexChangeHandlers = new List<Action>();
            }

            private readonly List<Expression<Func<TResult, bool>>> _wheres = new List<Expression<Func<TResult, bool>>>();
            private readonly List<Action> _indexChangeHandlers;

            public void DefineIndex()
            {
                var builder = new IndexDefinitionBuilder<TAggregate>();

                builder.Map = Index();
                builder.StoresStrings.Add(Constants.Documents.Indexing.Fields.AllFields, FieldStorage.Yes);
                var indexDefinition = builder.ToIndexDefinition(_db.Store.Conventions);
                indexDefinition.Name = IndexName;
               
                _db.Store.Maintenance.Send(new PutIndexesOperation(indexDefinition));
                _db.Store.Changes().ForIndex(indexDefinition.Name).Subscribe(new IndexObserver(OnChange));
            }

            public void AddIndexChangeHandler(Action onChange)
            {
                _indexChangeHandlers.Add(onChange);
            }

            private void OnChange(IndexChange o)
            {
                foreach (var h in _indexChangeHandlers)
                {
                    h();
                }
            }

            public string IndexName => GetType().FullName;

            public abstract Expression<Func<IEnumerable<TAggregate>, IEnumerable>> Index();

            public void Where(Expression<Func<TResult, bool>> predicate)
            {
                _wheres.Add(predicate);
            }

            
            protected async Task<IReadOnlyCollection<TResult>> Execute()
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

        public interface IRepository
        {
            void AddIndexChangeHandler(Action onChange);
        }
        public interface IQueryDefiner
        {
            void DefineIndex();
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