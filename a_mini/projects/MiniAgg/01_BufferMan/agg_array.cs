//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

//MIT 2014, WinterDev
//----------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Collections;

namespace MatterHackers.Agg
{
    //POD= Plain Old Data  

    public class ArrayPOD<T>
    {
        public ArrayPOD(int size)
        {
            m_array = new T[size];
            m_size = size;
        }
        public void Resize(int size)
        {
            if (size != m_size)
            {
                m_array = new T[size];
            }
        }

        public int Size() { return m_size; }

        public T this[int index]
        {
            get
            {
                return m_array[index];
            }
            set
            {
                m_array[index] = value;
            }
        }

        public T[] Array
        {
            get
            {
                return m_array;
            }
        }

        private T[] m_array;
        private int m_size;
    }


    //--------------------------------------------------------------pod_vector
    // A simple class template to store Plain Old Data, a vector
    // of a fixed size. The data is contiguous in memory
    //------------------------------------------------------------------------
    public class VectorPOD<T>
    {
        protected int currentSize;
        private T[] internalArray = new T[0];

        public int Count
        {
            get { return currentSize; }
        }

        public IEnumerable<T> DataIterator()
        {
            for (int index = 0; index < currentSize; index++)
            {
                // Yield each day of the week. 
                yield return internalArray[index];
            }
        }

        public IEnumerator GetEnumerator()
        {
            for (int index = 0; index < currentSize; index++)
            {
                // Yield each day of the week. 
                yield return internalArray[index];
            }
        }

        public int AllocatedSize
        {
            get
            {
                return internalArray.Length;
            }
        }

        public VectorPOD()
        {
        }

        public VectorPOD(int cap)
            : this(cap, 0)
        {
        }

        public VectorPOD(int capacity, int extraTail)
        {
            Allocate(capacity, extraTail);
        }

        //public virtual void Remove(int indexToRemove)
        //{
        //    if (indexToRemove >= Length)
        //    {
        //        throw new Exception("requested remove past end of array");
        //    }

        //    for (int i = indexToRemove; i < Length - 1; i++)
        //    {
        //        internalArray[i] = internalArray[i + 1];
        //    }

        //    currentSize--;
        //}

        public virtual void RemoveLast()
        {
            if (currentSize != 0)
            {
                currentSize--;
            }
        }

        // Copying
        public VectorPOD(VectorPOD<T> vectorToCopy)
        {
            currentSize = vectorToCopy.currentSize;
            internalArray = (T[])vectorToCopy.internalArray.Clone();
        }

        public void CopyFrom(VectorPOD<T> vetorToCopy)
        {
            Allocate(vetorToCopy.currentSize);
            if (vetorToCopy.currentSize != 0)
            {
                vetorToCopy.internalArray.CopyTo(internalArray, 0);
            }
        }

        // Set new capacity. All data is lost, size is set to zero.
        public void Capacity(int newCapacity)
        {
            Capacity(newCapacity, 0);
        }

        public void Capacity(int newCapacity, int extraTail)
        {
            currentSize = 0;
            if (newCapacity > AllocatedSize)
            {
                internalArray = null;
                int sizeToAllocate = newCapacity + extraTail;
                if (sizeToAllocate != 0)
                {
                    internalArray = new T[sizeToAllocate];
                }
            }
        }

        public int Capacity() { return AllocatedSize; }

        // Allocate n elements. All data is lost, 
        // but elements can be accessed in range 0...size-1. 
        public void Allocate(int size)
        {
            Allocate(size, 0);
        }

        public void Allocate(int size, int extraTail)
        {
            Capacity(size, extraTail);
            currentSize = size;
        }

        // Resize keeping the content.
        public void Resize(int newSize)
        {
            if (newSize > currentSize)
            {
                if (newSize > AllocatedSize)
                {
                    var newArray = new T[newSize];
                    if (internalArray != null)
                    {
                        for (int i = internalArray.Length - 1; i >= 0; --i)
                        {
                            newArray[i] = internalArray[i];
                        }

                    }
                    internalArray = newArray;
                }
            }
        }

#pragma warning disable 649
        static T zeroed_object;
#pragma warning restore 649

        public void zero()
        {
            int NumItems = internalArray.Length;
            for (int i = 0; i < NumItems; i++)
            {
                internalArray[i] = zeroed_object;
            }
        }

        public void Add(T v)
        {
            add(v);
        }

        public virtual void add(T v)
        {
            if (internalArray.Length < (currentSize + 1))
            {
                if (currentSize < 100000)
                {
                    Resize(currentSize + (currentSize / 2) + 16);
                }
                else
                {
                    Resize(currentSize + currentSize / 4);
                }
            }
            internalArray[currentSize++] = v;
        } 
        public void inc_size(int size) { currentSize += size; }
        public int size() { return currentSize; }

        public T this[int i]
        {
            get
            {
                return internalArray[i];
            }
        }

        public T[] Array
        {
            get
            {
                return internalArray;
            }
        }

        public T at(int i) { return internalArray[i]; }
        public T value_at(int i) { return internalArray[i]; }

        public T[] data() { return internalArray; }

        public void remove_all() { currentSize = 0; }
        public void clear() { currentSize = 0; }
        public void cut_at(int num) { if (num < currentSize) currentSize = num; }

        public int Length
        {
            get
            {
                return currentSize;
            }
        }

        public void Clear()
        {
            currentSize = 0;
        }
         
    }

    //----------------------------------------------------------range_adaptor
    public class VectorPOD_RangeAdaptor
    {
        VectorPOD<int> m_array;
        int m_start;
        int m_size;

        public VectorPOD_RangeAdaptor(VectorPOD<int> array, int start, int size)
        {
            m_array = (array);
            m_start = (start);
            m_size = (size);
        }

        public int size() { return m_size; }
        public int this[int i]
        {
            get
            {
                return m_array.Array[m_start + i];
            }

            set
            {
                m_array.Array[m_start + i] = value;
            }
        }
        public int at(int i) { return m_array.Array[m_start + i]; }
        public int value_at(int i) { return m_array.Array[m_start + i]; }
    }

    public class FirstInFirstOutQueue<T>
    {
        T[] itemArray;
        int size;
        int head;
        int shiftFactor;
        int mask;

        public int Count
        {
            get { return size; }
        }

        public FirstInFirstOutQueue(int shiftFactor)
        {
            this.shiftFactor = shiftFactor;
            mask = (1 << shiftFactor) - 1;
            itemArray = new T[1 << shiftFactor];
            head = 0;
            size = 0;
        }

        public T First
        {
            get { return itemArray[head & mask]; }
        }

        public void Enqueue(T itemToQueue)
        {
            if (size == itemArray.Length)
            {
                int headIndex = head & mask;
                shiftFactor += 1;
                mask = (1 << shiftFactor) - 1;
                T[] newArray = new T[1 << shiftFactor];
                // copy the from head to the end
                Array.Copy(itemArray, headIndex, newArray, 0, size - headIndex);
                // copy form 0 to the size
                Array.Copy(itemArray, 0, newArray, size - headIndex, headIndex);
                itemArray = newArray;
                head = 0;
            }
            itemArray[(head + (size++)) & mask] = itemToQueue;
        }

        public T Dequeue()
        {
            int headIndex = head & mask;
            T firstItem = itemArray[headIndex];
            if (size > 0)
            {
                head++;
                size--;
            }
            return firstItem;
        }
    }
}