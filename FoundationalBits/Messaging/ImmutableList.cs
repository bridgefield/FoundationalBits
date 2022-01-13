using System.Collections;
using System.Collections.Generic;

namespace bridgefield.FoundationalBits.Messaging
{
    internal readonly struct ImmutableList<T> : IEnumerable<T>
    {
        private readonly IBucket bucket;

        private ImmutableList(IBucket bucket) => this.bucket = bucket;

        public IEnumerator<T> GetEnumerator() => bucket.AsEnumerable().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public ImmutableList<T> Add(T item) => bucket.Add(item).ToList();
        public ImmutableList<T> Remove(T item) => bucket.Remove(item).ToList();
        public static ImmutableList<T> Create() => new EmptyBucket().ToList();

        private interface IBucket
        {
            IEnumerable<T> AsEnumerable();
            ImmutableList<T> ToList();
            IBucket Add(T item) => new TailedBucket(item, this);
            IBucket Remove(T item);
        }

        private sealed record EmptyBucket : IBucket
        {
            public IEnumerable<T> AsEnumerable()
            {
                yield break;
            }

            public ImmutableList<T> ToList() => new(this);
            public IBucket Remove(T item) => this;
        }

        private sealed record TailedBucket(T Head, IBucket Tail) : IBucket
        {
            public IEnumerable<T> AsEnumerable() =>
                new ValueEnumerable(this);

            public ImmutableList<T> ToList() => new(this);

            public IBucket Remove(T item) =>
                Equals(item, Head)
                    ? Tail.Remove(item)
                    : new TailedBucket(Head, Tail.Remove(item));
        }

        private sealed class ValueEnumerable : IEnumerable<T>
        {
            private readonly TailedBucket bucket;

            public ValueEnumerable(TailedBucket bucket) => this.bucket = bucket;

            public IEnumerator<T> GetEnumerator() => new ValueEnumerator(bucket);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ValueEnumerator : IEnumerator<T>
        {
            private TailedBucket currentBucket;
            private bool started;
            private bool hasMore = true;

            public ValueEnumerator(TailedBucket currentBucket) =>
                this.currentBucket = currentBucket;

            public bool MoveNext()
            {
                if (!started)
                    started = true;
                else if (currentBucket.Tail is TailedBucket tail)
                    currentBucket = tail;
                else
                    hasMore = false;

                return hasMore;
            }

            public void Reset()
            {
            }

            public T Current => currentBucket.Head;

            object IEnumerator.Current => Current;

            public void Dispose() => Reset();
        }
    }
}