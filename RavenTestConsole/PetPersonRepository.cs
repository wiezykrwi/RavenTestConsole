using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RavenTestConsole
{
	public class PetPersonRepository : RavenDbRepository<PetPerson, PetPerson, PetPersonIndex>
	{
		public PetPersonRepository(IRavenDatabase database) : base(database)
		{
		}

		public override Expression<Func<IEnumerable<PetPerson>, IEnumerable>> Index()
		{
			return petPeople => 
				from person in petPeople
				from petId in person.Pets
				let pet = LoadDocument<Pet>(petId)
				select new
				{
					Name = person.Name,
					PetName = pet.Name,
					PetAge = pet.Age
				};
		}

		public Task<IReadOnlyCollection<PetPerson>> GetByName(string name)
		{
			return Execute(x => x.Name == name);
		}

		public Task<IReadOnlyCollection<PetPerson>> GetByPetName(string name)
		{
			return Execute(x => x.PetName == name);
		}
	}

	public class PetPersonIndex
	{
		public string Name { get; set; }
		public string PetName { get; set; }
		public int PetAge { get; set; }
	}
}