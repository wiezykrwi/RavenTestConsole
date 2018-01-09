using System;
using System.Collections.Generic;

using Raven.Client.Documents.Changes;

namespace RavenTestConsole
{
	public abstract class RavenDbRepositoryWithChangeNotification<TAggregate, TResult> : RavenDbRepository<TAggregate, TResult>
	{
		private readonly List<Action> _indexChangeHandlers;

		protected RavenDbRepositoryWithChangeNotification(IRavenDatabase database) : base(database)
		{
			_indexChangeHandlers = new List<Action>();
		}

		public override void DefineIndex()
		{
			base.DefineIndex();

			Database.Store.Changes().ForIndex(IndexName).Subscribe(new IndexObserver(OnChange));
		}

		public void AddIndexChangeHandler(Action onChange)
		{
			_indexChangeHandlers.Add(onChange);
		}

		private void OnChange(IndexChange o)
		{
			foreach (var handler in _indexChangeHandlers)
			{
				handler();
			}
		}

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
	}
}