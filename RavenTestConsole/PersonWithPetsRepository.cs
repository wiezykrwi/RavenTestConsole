using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Linq;

using RavenQuery;

namespace RavenTestConsole
{
	public class PersonWithPetsRepository : RavenDbRepository<Person, PersonWithPetsAndAge>, IPersonWithPetsRepository
	{
		protected override void Suggestions(IndexDefinitionBuilder<Person, PersonWithPetsAndAge> builder)
		{
			builder.SuggestionsOptions.Add(x => x.PersonName);
		}

		public override Expression<Func<IEnumerable<Person>, IEnumerable>> Index()
		{
			return people => from person in people
				let pet = LoadDocument<Pet>(person.Pet)
				select new { PersonName = person.Name, PetName = pet.Name, PetAge = pet.Age };
		}

		public PersonWithPetsRepository(IRavenDatabase database) : base(database)
		{
		}

		public Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThan(int age)
		{
			return Execute(x => x.PetAge < age);
		}
		
		public Task<PersonWithPetsAndAge> GetByName2(string name)
		{
			using (var reefer = Query())
			{
				return Task.FromResult(reefer.Query.FirstOrDefault(x => x.PersonName == name));
			}
		}

		public async Task<IReadOnlyCollection<string>> GetSuggestionsByName(string name)
		{
			var results = await ExecuteSuggestionsQuery(query => query
				//.Where(x => x.PersonName == name)
				.SuggestUsing(builder => builder.ByField(x => x.PersonName, name)));

			var suggestions = results["PersonName"].Suggestions;

			return await Task.FromResult(suggestions);
		}

		public Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThanQuerySearchName(string name, int age)
		{
			return ExecuteQuery(query => query.Search(x => x.PersonName, name).Where(x => x.PetAge < age));
		}
	}
}