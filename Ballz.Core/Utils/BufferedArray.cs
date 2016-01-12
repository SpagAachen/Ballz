using System;
using System.Linq;

namespace Ballz.Utils
{
    public class BufferedArray<T>
    {
        private readonly T[] data;
        private readonly T[] buffer;

        private readonly int[] size;

        public BufferedArray(params int[] size)
        {
            this.size = size;
            data = new T[size.Aggregate(1,(x,y) => x*y)];
            buffer = new T[size.Aggregate(1, (x, y) => x * y)];
        }

        private int ToIndex(int[] pos)
        {
            if (size.Length != pos.Length)
                throw new ArgumentOutOfRangeException();


            var val = 0;
            var mul = 1;
            for (var i = size.Length-1; i >= 0; --i)
            {
                val += pos[i]*mul;
                mul *= size[i];
            }
            return val;
        }

        public T this[params int[] pos]
        {
            get { return data[ToIndex(pos)]; }
            set { buffer[ToIndex(pos)] = value; }
        }

        public void Unbuffer()
        {
            for (var i = 0; i < data.Length; ++i)
                data[i] = buffer[i];
        }
    }
}
