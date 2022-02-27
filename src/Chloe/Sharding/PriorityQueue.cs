using System.Collections;

namespace Chloe.Sharding
{
    internal class PriorityQueue<T> : IEnumerable<T>
    {
        private const int defaultCapacity = 0x10; //默认容量为16

        private bool descending;
        private int heapLength;
        private T[] buffer;

        private IComparer<T> comparer;
        public PriorityQueue()
            : this(defaultCapacity)
        {
        }
        public PriorityQueue(int initCapacity, bool ascending = true, IComparer<T> comparer = null)
        {
            buffer = new T[initCapacity];
            heapLength = 0;
            descending = ascending;
            this.comparer = comparer ?? Comparer<T>.Default;
        }

        public bool IsEmpty()
        {
            return heapLength == 0;
        }


        public bool TryPeek(out T item)
        {
            item = default(T);
            if (this.IsEmpty())
            {
                return false;
            }

            item = buffer[0];
            return true;
        }

        public T Peek()
        {
            if (heapLength == 0)
                throw new OverflowException("queu is empty no element can return");

            return buffer[0];
        }


        public void Push(T obj)
        {
            if (IsFull())
                expand();

            buffer[heapLength] = obj;
            Heap<T>.heapAdjustFromBottom(buffer, heapLength, descending, comparer);
            heapLength++;
        }

        public void Pop()
        {
            if (heapLength == 0)
                throw new OverflowException("优先队列为空时无法执行出队操作");

            --heapLength;
            swap(0, heapLength);
            Heap<T>.heapAdjustFromTop(buffer, 0, heapLength, descending, this.comparer);
        }

        public T Poll()
        {
            if (this.IsEmpty())
                return default(T);
            var first = this.Peek();
            this.Pop();
            return first;
        }

        /// <summary>
        /// 集合是否满了
        /// </summary>
        /// <returns></returns>
        public bool IsFull()
        {
            return heapLength == buffer.Length;
        }

        private void expand()
        {
            Array.Resize<T>(ref buffer, buffer.Length * 2);
        }

        private void swap(int a, int b)
        {
            T tmp = buffer[a];
            buffer[a] = buffer[b];
            buffer[b] = tmp;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in this.buffer)
            {
                if (item == null)
                    continue;

                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    internal class Heap<T>
    {
        public static void HeapSort(T[] objects, IComparer<T> comparer)
        {
            HeapSort(objects, false, comparer);
        }
        public static void HeapSort(T[] objects, bool descending, IComparer<T> comparer)
        {
            for (int i = objects.Length / 2 - 1; i >= 0; --i)
                heapAdjustFromTop(objects, i, objects.Length, descending, comparer);
            for (int i = objects.Length - 1; i > 0; --i)
            {
                swap(objects, i, 0);
                heapAdjustFromTop(objects, 0, i, descending, comparer);
            }
        }

        public static void heapAdjustFromBottom(T[] objects, int n, IComparer<T> comparer)
        {
            heapAdjustFromBottom(objects, n, false, comparer);
        }

        public static void heapAdjustFromBottom(T[] objects, int n, bool descending, IComparer<T> comparer)
        {
            while (n > 0 && descending ^ comparer.Compare(objects[(n - 1) >> 1], objects[n]) < 0)
            {
                swap(objects, n, (n - 1) >> 1);
                n = (n - 1) >> 1;
            }
        }

        public static void heapAdjustFromTop(T[] objects, int n, int len, IComparer<T> comparer)
        {
            heapAdjustFromTop(objects, n, len, false, comparer);
        }

        public static void heapAdjustFromTop(T[] objects, int n, int len, bool descending, IComparer<T> comparer)
        {
            while ((n << 1) + 1 < len)
            {
                int m = (n << 1) + 1;
                if (m + 1 < len && descending ^ comparer.Compare(objects[m], objects[m + 1]) < 0)
                    ++m;
                if (descending ^ comparer.Compare(objects[n], objects[m]) > 0) return;
                swap(objects, n, m);
                n = m;
            }
        }

        private static void swap(T[] objects, int a, int b)
        {
            T tmp = objects[a];
            objects[a] = objects[b];
            objects[b] = tmp;
        }
    }
}
