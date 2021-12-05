using System;
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

        public TResult Match<TResult>(
            Func<TResult> empty,
            Func<T, ImmutableList<T>, TResult> headAndTail) =>
            bucket.Match(
                empty,
                (h, t) => headAndTail(h, t.ToList()));

        private interface IBucket
        {
            IEnumerable<T> AsEnumerable();
            ImmutableList<T> ToList();
            IBucket Add(T item) => new TailedBucket(item, this);
            IBucket Remove(T item);

            TResult Match<TResult>(
                Func<TResult> empty,
                Func<T, IBucket, TResult> headAndTail);
        }

        private sealed record EmptyBucket : IBucket
        {
            public IEnumerable<T> AsEnumerable()
            {
                yield break;
            }

            public ImmutableList<T> ToList() => new(this);
            public IBucket Remove(T item) => this;

            public TResult Match<TResult>(Func<TResult> empty, Func<T, IBucket, TResult> headAndTail) =>
                empty();
        }

        private sealed record TailedBucket(T Head, IBucket Tail) : IBucket
        {
            public IEnumerable<T> AsEnumerable()
            {
                yield return Head;
                foreach (var item in Tail.AsEnumerable())
                {
                    yield return item;
                }
            }

            public ImmutableList<T> ToList() => new(this);

            public IBucket Remove(T item) =>
                Equals(item, Head)
                    ? Tail.Remove(item)
                    : new TailedBucket(Head, Tail.Remove(item));

            public TResult Match<TResult>(
                Func<TResult> empty,
                Func<T, IBucket, TResult> headAndTail) => headAndTail(Head, Tail);
        }
    }
}