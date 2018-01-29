using System;
using System.Collections.Generic;

using Raven.Client.Documents.Changes;

namespace RavenQuery
{
	public abstract class RavenDbRepositoryWithChangeNotification<TAggregate, TResult> : RavenDbRepository<TAggregate, TResult>
	{
		private readonly List<Action<string>> _indexChangeHandlers;

		protected RavenDbRepositoryWithChangeNotification(IRavenDatabase database) : base(database)
		{
			_indexChangeHandlers = new List<Action<string>>();
		}

		public override void DefineIndex()
		{
			base.DefineIndex();

			Database.Store.Changes().ForIndex(IndexName).Subscribe(new ChangeObserver<IndexChange>(OnIndexChange));
			Database.Store.Changes().ForDocumentsInCollection<TAggregate>().Subscribe(new ChangeObserver<DocumentChange>(OnDocumentChange));
		}

		public void AddIndexChangeHandler(Action<string> onChange)
		{
			_indexChangeHandlers.Add(onChange);
		}

		private void OnIndexChange(IndexChange o)
		{
			foreach (var handler in _indexChangeHandlers)
			{
				handler($"Index change: {o.Type}");
			}
		}

		private void OnDocumentChange(DocumentChange o)
		{
			foreach (var handler in _indexChangeHandlers)
			{
				handler($"Document change: {o.Type}");
			}
		}
		
		private class ChangeObserver<T> : IObserver<T>
		{
			private readonly Action<T> _onNext;

			public ChangeObserver(Action<T> onNext)
			{
				_onNext = onNext;
			}

			public void OnCompleted()
			{
			}

			public void OnError(Exception error)
			{
			}

			public void OnNext(T value)
			{
				_onNext(value);
			}
		}
	}
}