using System;

namespace IEC16022Sharp
{
    public class Immutable2DArray<T> : IEquatable<Immutable2DArray<T>>
    {
        private readonly T[,] _array;

        public Immutable2DArray(T[,] data)
        {
            _array = data ?? throw new ArgumentNullException(nameof(data));
        }

        public T this[int x, int y]
        {
            get => _array[x, y];
        }

        public int Length0()
            => GetLength(0);

        public int Length1()
            => GetLength(1);

        public int GetLength(int dimension)
            => _array.GetLength(dimension);

        public bool Equals(Immutable2DArray<T> other)
        {
            return _array.GetLength(0) == other._array.GetLength(0)
                && _array.GetLength(1) == other._array.GetLength(1)
                && _array.GetHashCode() == other._array.GetHashCode();
        }

        public override bool Equals(object obj)
            => obj is Immutable2DArray<T> other ? Equals(other) : false;

        public override int GetHashCode()
            => _array.GetHashCode();
    }
}
