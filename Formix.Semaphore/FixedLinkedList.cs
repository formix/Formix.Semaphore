using System;
using System.Collections.Generic;

namespace Formix.Semaphore
{
    /// <summary>
    /// I don't thing that needs any documentation.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class FixedLinkedList<T> : LinkedList<T>, IList<T>
    {
        public T this[int index]
        {
            get
            {
                return GetAt(index).Value;
            }
            set
            {
                GetAt(index).Value = value;
            }
        }

        public int IndexOf(T item)
        {
            int i = 0;
            var node = First;
            while (node != null && !node.Value.Equals(item))
            {
                i++;
                node = node.Next;
            }

            if (node == null)
            {
                return -1;
            }

            return i;
        }

        public void Insert(int index, T item)
        {
            AddBefore(GetAt(index), new LinkedListNode<T>(item));
        }

        public void RemoveAt(int index)
        {
            Remove(GetAt(index));
        }

        private LinkedListNode<T> GetAt(int index)
        {
            CheckIndex(index);
            int i = 0;
            var node = First;
            while (i < index)
            {
                i++;
                node = node.Next;
            }

            return node;
        }

        private void CheckIndex(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Parameter must be greater or equal than 0.");
            }

            if (index >= Count)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    "Parameter must be lower than the list items Count.");
            }
        }
    }
}
