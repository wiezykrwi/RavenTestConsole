using System;

namespace RavenTestConsole
{
	public interface IRepositoryWithChangeNotification
	{
		void AddIndexChangeHandler(Action onChange);
	}
}