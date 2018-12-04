using System;

namespace Ludumdare43
{
    public interface IStatus<T> where T : struct, IComparable
    {
        void FullRestore();
        void Clear();
        void Restore(T value);
        void Remove(T value);
    }
}
