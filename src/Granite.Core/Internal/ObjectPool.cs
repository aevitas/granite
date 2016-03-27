using System;
using System.Collections.Concurrent;

namespace Granite.Core.Internal
{
    internal class ObjectPool<T> : IPool<T>
    {
        private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();
        private readonly Func<T> _retriever;

        public ObjectPool(Func<T> retriever)
        {
            Requires.NotNull(retriever, nameof(retriever));

            _retriever = retriever;
        } 

        public T Take()
        {
            T item;
            if (_pool.TryTake(out item))
                return item;

            return _retriever();
        }

        public void Return(T item)
        {
            _pool.Add(item);
        }
    }
}
