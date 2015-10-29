namespace PICSolver.Storage
{
    using Abstract;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ParticleArrayStorage<T> : IParticleStorage<T> where T : IParticle, new()
    {
        /// <summary>
        /// Particle data storage.
        /// x y px py q fx fy
        /// </summary>
        private double[] _data;

        /// <summary>
        /// Particle cell id.
        /// </summary>
        private int[] _cell;

        /// <summary>
        /// List with deleted particles indexes.
        /// </summary>
        private IList<int> _deleted;

        /// <summary>
        /// Number of items.
        /// </summary>
        private int _count;

        /// <summary>
        /// Number of data columns.
        /// </summary>
        private int _width;

        /// <summary>
        /// Total number of elements the internal data structure can hold without resizing.
        /// </summary>
        private int _capacity;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ParticleArrayStorage"/>.
        /// </summary>
        public int Count { get { return _count - _deleted.Count; } }

        /// <summary>
        /// Gets the total number of elements the internal data structure can hold without resizing.
        /// </summary>
        public int Capacity { get { return _capacity; } }

        /// <summary>
        /// Initializes a new instance of the ParticleArrayStorage class that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that <see cref="ParticleArrayStorage"/> can store.</param>
        public ParticleArrayStorage(int capacity)
        {
            _width = 7;
            _capacity = capacity;
            _data = new double[_width * capacity];
            _cell = new int[_capacity];
            _deleted = new List<int>(capacity);
        }

        /// <summary>
        /// Adds a new item into <see cref="ParticleArrayStorage"/>
        /// </summary>
        /// <param name="particle">The value to add.</param>
        public int Add(T particle)
        {
            if (_deleted.Count > 0)
            {
                var last = _deleted[_deleted.Count - 1];
                At(last, particle);
                _deleted.RemoveAt(_deleted.Count - 1);
                return last;
            }
            else
            {
                At(_count, particle);
                _count++;
                return _count - 1;
            }
        }

        /// <summary>
        /// Reset forces on particles.
        /// </summary>
        public void ResetForces()
        {
            for (int i = 0; i < _capacity; i++)
            {
                _data[_width * i + 5] = 0;
                _data[_width * i + 6] = 0;
            }
        }

        /// <summary>
        /// Add force to particle.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <param name="forceX">Force on particle.</param>
        /// <param name="forceY">Force on particle.</param>
        public void AddForce(int index, double forceX, double forceY)
        {
            _data[_width * index + 5] += forceX;
            _data[_width * index + 6] += forceY;
        }

        public double Get(ParticleField field, int index)
        {
            return _data[_width * index + (int)field];
        }

        public void SetParticleCell(int index, int cell)
        {
            _cell[index] = cell;
        }

        public int GetParticleCell(int index)
        {
            return _cell[index];
        }

        /// <summary>
        /// Removes item at the specified index.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        public void RemoveAt(int index)
        {
            if (index >= _capacity) throw new ArgumentOutOfRangeException("index");
            _data[_width * index+ 4] = 0;
            _deleted.Add(index);
        }

        /// <summary>
        /// Get the value of the given element at the specified index without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <returns>The requested element.</returns>
        public T At(int index)
        {
            var particle = default(T);
            particle = new T();
            particle.Id = index;
            particle.X = _data[_width * index];
            particle.Y = _data[_width * index + 1];
            particle.Px = _data[_width * index + 2];
            particle.Py = _data[_width * index + 3];
            particle.Q = _data[_width * index + 4];
            particle.Ex = _data[_width * index + 5];
            particle.Ey = _data[_width * index + 6];
            return particle;
        }

        /// <summary>
        ///  Sets the value of the given element without range checking.
        /// </summary>
        /// <param name="index">The index of the element.</param>
        /// <param name="particle">The value to set the element to.</param>
        public void At(int index, T particle)
        {
            _data[_width * index] = particle.X;
            _data[_width * index + 1] = particle.Y;
            _data[_width * index + 2] = particle.Px;
            _data[_width * index + 3] = particle.Py;
            _data[_width * index + 4] = particle.Q;
            _data[_width * index + 5] = particle.Ex;
            _data[_width * index + 6] = particle.Ey;
        }
        public void Update(int index, double x, double y, double px, double py)
        {
            _data[_width * index] = x;
            _data[_width * index + 1] = y;
            _data[_width * index + 2] = px;
            _data[_width * index + 3] = py;
        }
        /// <summary>
        /// Gets of sets the value of the given element at the specified index with range checking.
        /// </summary>
        /// <param name="i">The index of the element.</param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                if (i >= _count) throw new ArgumentOutOfRangeException("i");
                return At(i);
            }
            set
            {
                if (i >= _count) throw new ArgumentOutOfRangeException("i");
                At(i, value);
            }
        }
        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the storage.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                var particle = At(i);
                if (_deleted.Contains(i)) continue;
                yield return particle;
            }
        }

        public IEnumerable<int> EnumerateIndexes()
        {
            for (int i = 0; i < _count; i++)
            {
                if (_deleted.Contains(i)) continue;
                yield return i;
            }
        }

        /// <summary>
        /// Returns an IEnumerable that can be used to iterate through all values of the storage.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
