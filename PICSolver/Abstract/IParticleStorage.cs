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
        void AddForce(int index, double forceX, double forceY);
        void SetCell(int index, int cell);
        int GetCell(int index);
        double GetForceX(int index);
        double GetForceY(int index);
        void Update(int index, double x, double y, double px, double py);
        IEnumerable<int> EnumerateIndexes();
    }
}
