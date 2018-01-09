using System.Collections.Generic;
using System.Threading.Tasks;

namespace RavenTestConsole
{
	internal interface IPersonWithPetsRepository
	{
		Task<IReadOnlyCollection<PersonWithPetsAndAge>> GetPersonWithPetsYoungerThan(int age);
	}
}