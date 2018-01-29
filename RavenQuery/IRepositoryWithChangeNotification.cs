using System;

namespace RavenQuery
{
	public interface IRepositoryWithChangeNotification
	{
		void AddIndexChangeHandler(Action onChange);
	}
}