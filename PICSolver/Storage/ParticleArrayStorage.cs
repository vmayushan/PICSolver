using System;
using System.Collections;
using System.Collections.Generic;
using PICSolver.Abstract;

namespace PICSolver.Storage
{
    public class ParticleArrayStorage<T> : IParticleStorage<T> where T : IParticle, new()
    {
        /// <summary>
        /// Total number of elements the internal data structure can hold without resizing.
        /// </summary>
        private readonly int capacity;

        /// <summary>
        ///     Particle cellId id.
        /// </summary>
        private readonly int[] cell;

        /// <summary>
        ///     Number of items.
        /// </summary>
        private int count;

        /// <summary>
        ///     Particle data storage.
        /// </summary>
        private readonly double[] data;

        /// <summary>
        ///     List with deleted particles indexes.
        /// </summary>
        private readonly IList<int> deleted;

        /// <summary>
        ///     Number of data columns.
        /// </summary>
        private readonly int width;

        /// <summary>
        ///     Initializes a new instance of the ParticleArrayStorage class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that ParticleArrayStorage can store.</param>
        public ParticleArrayStorage(int capacity)
        {
            width = 9;
            this.capacity = capacity;
            data = new double[width * capacity];
            cell = new int[this.capacity];
            deleted = new List<int>(capacity);
        }

        /// <summary>
        ///     Gets the number of elements contained in the ParticleArrayStorage.
        /// </summary>
        public int Count => count - deleted.Count;

        /// <summary>
        ///     Gets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public int Capacity => capacity;

        /// <summary>
        ///     Adds a new item into ParticleArrayStorage
        /// </summary>
        /// <param name="particle">The value to add.</param>
        public int Add(T particle)
        {
            if (deleted.Count > 0)
            {
                var last = deleted[deleted.Count - 1];
                At(last, particle);
                deleted.RemoveAt(deleted.Count - 1);
                return last;
            }
            At(count, particle);
            count++;
            return count - 1;
        }

        /// <summary>
        ///     Reset forces on particles.
        /// </summary>
        public void ResetForces()
        {
            for (var i = 0; i < capacity; i++)
            {
                data[width * i + 5] = 0;
                data[width * i + 6] = 0;
            }
        }

        /// <summary>
        ///     Get particle property value
        /// </summary>
        /// <param name="field">Property name</param>
        /// <param name="index">Particle id</param>
        /// <returns>Property value</returns>
        public double Get(Field field, int index)
        {
            return data[width * index + (int)field];
        }

        /// <summary>
        /// Set particle property
        /// </summary>
        /// <param name="field">Property name</param>
        /// <param name="index">Particle id</param>
        /// <param name="value">Property value</param>
        public void Set(Field field, int index, double value)
        {
            data[width * index + (int)field] = value;
        }

        /// <summary>
        /// Increment particle property value
        /// </summary>
        /// <param name="field">Property name</param>
        /// <param name="index">Particle id</param>
        /// <param name="value">Property value</param>
        public void Increment(Field field, int index, double value)
        {
            data[width * index + (int)field] += value;
        }
        /// <summary>
        /// Myltiply particle property value
        /// </summary>
        /// <param name="field">Property name</param>
        /// <param name="index">Particle id</param>
        /// <param name="value">Property value</param>
        public void Multiply(Field field, int index, double value)
        {
            data[width * index + (int)field] *= value;
        }
        /// <summary>
        /// </summary>
        /// <param name="index">Particle id</param>
        /// <param name="cellId">cellId id</param>
        public void SetParticleCell(int index, int cellId)
        {
            cell[index] = cellId;
        }

        /// <summary>
        /// </summary>
        /// <param name="index">particle id</param>
        /// <returns>cellId id</returns>
        public int GetParticleCell(int index)
        {
            return cell[index];
        }

        /// <summary>
        ///     Removes item at the specified index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        public void RemoveAt(int index)
        {
            if (index >= capacity) throw new ArgumentOutOfRangeException(nameof(index));
            data[width * index + 4] = 0;
            lock (deleted)
            deleted.Add(index);
        }

        /// <summary>
        ///     Get the value of the given element at the specified index without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The requested element.</returns>
        public T At(int index)
        {
            var particle = new T
            {
                Id = index,
                X = data[width * index],
                Y = data[width * index + 1],
                Px = data[width * index + 2],
                Py = data[width * index + 3],
                Q = data[width * index + 4],
                Ex = data[width * index + 5],
                Ey = data[width * index + 6],
                PrevX = data[width * index + 7],
                PrevY = data[width * index + 8]
            };
            return particle;
        }

        /// <summary>
        ///     Sets the value of the given element without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <param name="particle">The value to set the element to.</param>
        public void At(int index, T particle)
        {
            data[width * index] = particle.X;
            data[width * index + 1] = particle.Y;
            data[width * index + 2] = particle.Px;
            data[width * index + 3] = particle.Py;
            data[width * index + 4] = particle.Q;
            data[width * index + 5] = particle.Ex;
            data[width * index + 6] = particle.Ey;
            data[width * index + 7] = particle.PrevX;
            data[width * index + 8] = particle.PrevY;
        }

        /// <summary>
        ///     Update particle data
        /// </summary>
        /// <param name="index">particle id</param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="px"></param>
        /// <param name="py"></param>
        public void Update(int index, double x, double y, double px, double py)
        {
            data[width * index] = x;
            data[width * index + 1] = y;
            data[width * index + 2] = px;
            data[width * index + 3] = py;
        }

        /// <summary>
        ///     Gets of sets the value of the given element at the specified index with range checking.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                if (i >= count) throw new ArgumentOutOfRangeException(nameof(i));
                return At(i);
            }
            set
            {
                if (i >= count) throw new ArgumentOutOfRangeException(nameof(i));
                At(i, value);
            }
        }

        /// <summary>
        ///     Returns an IEnumerable that can be used to iterate through all values of the storage.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < count; i++)
            {
                var particle = At(i);
                if (deleted.Contains(i)) continue;
                yield return particle;
            }
        }

        public IEnumerable<int> EnumerateIndexes()
        {
            for (var i = 0; i < count; i++)
            {
                if (deleted.Contains(i)) continue;
                yield return i;
            }
        }

        /// <summary>
        ///     Returns an IEnumerable that can be used to iterate through all values of the storage.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}