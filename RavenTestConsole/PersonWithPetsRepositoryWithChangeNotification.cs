using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RavenTestConsole
{
	public class PersonWithPetsRepository : RavenDbRepository<Person, PersonWithPetsAndAge>, IPersonWithPetsRepository
	{
		public override Expression<Func<IEnumerable<Person>, IEnumerable>> Index()
		{
			return people => from person in people
				let pet = LoadDocument<Pet>(person.Pet)
				select new PersonWithPetsAndAge { PersonName = person.Name, PetName = pet.Name, PetAge = pet.Age };
		}

		public PersonWithPetsRepository(IRavenDatabase database) : base(database)
		{
		}

		public Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThan(int age)
		{
			return Execute(x => x.PetAge < age);
		}
	}
}