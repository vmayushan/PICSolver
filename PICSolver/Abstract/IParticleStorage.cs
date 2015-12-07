using PICSolver.Storage;
using System.Collections.Generic;

namespace PICSolver.Abstract
{
    public interface IParticleStorage<T> : IEnumerable<T> where T : IParticle
    {
        int Count { get; }
        int Capacity { get; }
        int Add(T particle);
        void RemoveAt(int index);
        T At(int index);
        void At(int index, T particle);
        T this[int index] { get; set; }
        void ResetForces();
        void SetParticleCell(int index, int cellId);
        int GetParticleCell(int index);
        double Get(Field field, int index);
        void Set(Field field, int index, double value);
        void Increment(Field field, int index, double value);
        void Multiply(Field field, int index, double value);
        void Update(int index, double x, double y, double px, double py);
        IEnumerable<int> EnumerateIndexes();
    }
}
