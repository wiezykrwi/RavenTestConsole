using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Raven.Client.Documents.Indexes;

using RavenQuery;

namespace Multimap
{
	public class MessageRepository : CustomRavenDbRepository<MessageBase, MessageRepository.MessageRepositoryIndex>
	{
		public MessageRepository(IRavenDatabase database) : base(database)
		{
		}
		
		public Task<IReadOnlyCollection<MessageBase>> GetAllValidMessages()
		{
			return Execute(x => x.IsValid);
		}

		public class MessageRepositoryIndex : AbstractMultiMapIndexCreationTask
		{
			public MessageRepositoryIndex()
			{
				AddMapForAll<MessageBase>(messages =>
					from message in messages
					select new
					{
						message.IsValid
					});

				

				//AddMap<Iftmbc>(x => from y in x
				//					select new
				//					{
				//						Type = "IFTMBC",
				//						IsValid = y.IsValid
				//					});
				//AddMap<ThreeFifteen>(x => from y in x
				//						  select new
				//						  {
				//							  Type = "315",
				//							  IsValid = y.IsValid
				//						  });
			}
		}
	}
}