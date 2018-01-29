using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Raven.Client.Documents.Linq;

using RavenQuery;

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
					PetAge = pet.Age,

					PetNames = (from petId2 in person.Pets
						        let pet2 = LoadDocument<Pet>(petId2)
								select new { pet2.Name }).ToArray(),

					Pet = new
					{
						Name = pet.Name,
						Age = pet.Age
					}
				};
		}

		public Task<IReadOnlyCollection<PetPerson>> GetByName(string name)
		{
			return Execute(x => x.Name == name);
		}

		public Task<IReadOnlyCollection<PetPerson>> GetSuggestionsByName(string name)
		{
			return Execute(x => x.Name == name);
		}

		public Task<IReadOnlyCollection<PetPerson>> GetByPetName(string name)
		{
			return Execute(x => x.PetName == name);
		}

		public Task<IReadOnlyCollection<PetPerson>> GetByPetName2(string name)
		{
			return Execute(x => x.PetNames.ContainsAny(new []{ "Pastis", "Diggles" }));
		}
	}

	public class PetPersonIndex
	{
		public string Name { get; set; }
		public string PetName { get; set; }
		public string[] PetNames { get; set; }
		public int PetAge { get; set; }
		public Pet Pet { get; set; }
	}
}