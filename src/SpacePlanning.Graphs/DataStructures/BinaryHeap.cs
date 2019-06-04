/***************************************************************************************
* This code is originally created by Alvaro Alvaro Ortega Pickmans, and is available
* in his Graphical Packages
* Title: Graphical
* Author: Alvaro Ortega Pickmans
* Date: 2017
* Availability: https://github.com/alvpickmans/Graphical
*
***************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autodesk.RefineryToolkits.SpacePlanning.Graphs.DataStructures
{
    /// <summary>
    /// Custom Enum to determine behaviour of BinaryHeap
    /// </summary>
    public enum BinaryHeapType
    {
        /// <summary>
        /// BinaryHeap's items sorted in increasing order
        /// </summary>
        MinHeap,
        /// <summary>
        /// BinaryHeap's items sorted in decreasing order
        /// </summary>
        MaxHeap
    }

    /// <summary>
    /// Abstract BinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object implementing IComparable<TObject> interface</TObject></typeparam>
    public abstract class BinaryHeap<TObject> where TObject : IComparable<TObject>
    {
        #region Private Properties
        private const int DEFAULT_SIZE = 32;
        private const int GROWTH_FACTOR = 2;

        internal TObject[] _heapItems = null;
        private int _capacity;
        private int _size;
        private BinaryHeapType _heapType { get; set; }
        #endregion

        #region Public Properties

        /// <summary>
        /// HeapItem array capacity
        /// </summary>
        public int Capacity
        {
            get { return _capacity; }
            set { SetCapacity(value); }
        }

        /// <summary>
        /// HeapItem array size
        /// </summary>
        public int Size { get { return _size; } }

        /// <summary>
        /// Type of BinaryHeap: Min or Max.
        /// </summary>
        public BinaryHeapType HeapType { get { return _heapType; } }

        #endregion

        #region Constructor

        /// <summary>
        /// BinaryHeap default constructor
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        public BinaryHeap(BinaryHeapType heapType)
        {
            Capacity = DEFAULT_SIZE;
            _heapType = heapType;
            _heapItems = new TObject[DEFAULT_SIZE];
        }

        /// <summary>
        /// BinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        /// <param name="capacity">Heap initial capacity</param>
        public BinaryHeap(BinaryHeapType heapType, int capacity)
        {
            if( capacity < 1) { throw new ArgumentException("Capacity must be greater than zero"); }
            Capacity = capacity;
            _heapType = heapType;
            _heapItems = new TObject[capacity];
        }

        #endregion

        #region Private Methods

        internal virtual void SetCapacity (int newCapacity)
        {
            newCapacity = Math.Max(newCapacity, _size);
            if(_capacity != newCapacity)
            {
                _capacity = newCapacity;
                Array.Resize(ref _heapItems, _capacity);
            }
        }

        private void EnsureCapacity(int itemsToAdd = 0)
        {
            if(_size + itemsToAdd >= _capacity)
            {
                SetCapacity(_capacity * GROWTH_FACTOR);
            }
        }

        private void CheckHeap()
        {
            if (_size == 0) { throw new InvalidOperationException("Heap is empty."); }
        }

        #endregion

        #region Internal Methods

        internal int ParentIndex (int childIndex) { return (childIndex - 1) / 2; }
        internal int LeftChildIndex (int parentIndex) { return 2 * parentIndex + 1; }
        internal int RightChildIndex (int parentIndex) { return 2 * parentIndex + 2; }

        internal bool HasLeftChild (int parentIndex) { return LeftChildIndex(parentIndex) < _size; }
        internal bool HasRightChild (int parentIndex) { return RightChildIndex(parentIndex) < _size; }
        internal bool HasParent (int childIndex) { return ParentIndex(childIndex) >= 0; }

        internal TObject LeftChild (int parentIndex) { return _heapItems[LeftChildIndex(parentIndex)]; }
        internal TObject RightChild (int parentIndex) { return _heapItems[RightChildIndex(parentIndex)]; }
        internal TObject Parent (int childIndex) { return _heapItems[ParentIndex(childIndex)]; }

        /// <summary>
        /// Swap items
        /// </summary>
        /// <param name="firstIndex">Index of first item</param>
        /// <param name="secondIndex">Index of second item</param>
        internal virtual void Swap(int firstIndex, int secondIndex)
        {
            var tempItem = _heapItems[firstIndex];
            _heapItems[firstIndex] = _heapItems[secondIndex];
            _heapItems[secondIndex] = tempItem;
        }

        internal void HeapifyDown(int index = 0)
        {
            while (HasLeftChild(index))
            {
                int smallestChildIndex = LeftChildIndex(index);
                if (HasRightChild(index))
                {
                    switch (HeapType)
                    {
                        case BinaryHeapType.MinHeap:
                            if(RightChild(index).CompareTo(LeftChild(index)) == -1) { smallestChildIndex = RightChildIndex(index); }
                            break;
                        case BinaryHeapType.MaxHeap:
                            if (RightChild(index).CompareTo(LeftChild(index)) == 1) { smallestChildIndex = RightChildIndex(index); }
                            break;
                        default:
                            break;
                    }
                    
                }

                if (HeapType == BinaryHeapType.MinHeap && _heapItems[index].CompareTo(_heapItems[smallestChildIndex]) == -1)
                {
                    break;
                }
                else if(HeapType == BinaryHeapType.MaxHeap && _heapItems[index].CompareTo(_heapItems[smallestChildIndex]) == 1)
                {
                    break;
                }
                else
                {
                    Swap(index, smallestChildIndex);
                }
                index = smallestChildIndex;
            }
        }

        internal void HeapifyUp(int index = -1)
        {
            index = (index < 0) ? this.Size - 1 : index;
            if(HeapType == BinaryHeapType.MinHeap)
            {
                while (HasParent(index) && _heapItems[index].CompareTo(Parent(index)) == -1)
                {
                    Swap(index, ParentIndex(index));
                    index = ParentIndex(index);
                }
            }else
            {
                while (HasParent(index) && _heapItems[index].CompareTo(Parent(index)) == 1)
                {
                    Swap(index, ParentIndex(index));
                    index = ParentIndex(index);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the heap contains any element.
        /// </summary>
        /// <returns></returns>
        public virtual bool Any()
        {
            return Size > 0;
        }

        /// <summary>
        /// Returns first TObject from the Heap
        /// </summary>
        /// <returns>Object of first item.</returns>
        public virtual TObject Peek()
        {
            CheckHeap();
            return _heapItems[0];
        }

        /// <summary>
        /// Returns first TObject and removes it from the Heap
        /// </summary>
        /// <returns>First TObject</returns>
        public virtual TObject Take()
        {
            TObject first = this.Peek();
            _heapItems[0] = _heapItems[_size - 1];
            _size--;
            HeapifyDown();
            return first;
        }

        /// <summary>
        /// Removes all items on the Heap
        /// </summary>
        public virtual void Clear()
        {
            _size = 0;
            Array.Clear(_heapItems, 0, _heapItems.Length);
        }

        /// <summary>
        /// Adds a new TObject on the Heap
        /// </summary>
        /// <param name="item">Heap item</param>
        public virtual void Add(TObject item)
        {
            EnsureCapacity();
            _heapItems[_size] = item;
            _size++;
            HeapifyUp();
        }

        /// <summary>
        /// Adds a range of TObjects to the Heap
        /// </summary>
        /// <param name="items">Set of items</param>
        public virtual void AddRange(IEnumerable<TObject> items)
        {
            EnsureCapacity(items.Count());
            foreach(var item in items)
            {
                _heapItems[_size] = item;
                _size++;
                HeapifyUp();
            }
        }

        #endregion

    }

    /// <summary>
    /// Abstract BinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object to store</typeparam>
    /// <typeparam name="TValue">Type of value associated to the object, implementing IComparable interface</typeparam>
    public abstract class BinaryHeap<TObject, TValue> : BinaryHeap<HeapItem> where TValue : IComparable
    {
        #region Constructors
        /// <summary>
        /// BinaryHeap default constructor
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        public BinaryHeap(BinaryHeapType heapType) : base(heapType) { }

        /// <summary>
        /// BinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        /// <param name="capacity">Heap initial capacity</param>
        public BinaryHeap(BinaryHeapType heapType, int capacity) : base(heapType, capacity) { }
        #endregion

        /// <summary>
        /// Adds a new TObject with an associated TValue to the Heap
        /// </summary>
        /// <param name="item">TObject</param>
        /// <param name="value">TValue</param>
        public virtual void Add(TObject item, TValue value)
        {
            base.Add(new HeapItem(item, value));
        }

        /// <summary>
        /// Returns the first TObject and removes it from the Heap
        /// </summary>
        /// <returns>First TObject</returns>
        public new virtual TObject Take()
        {
            return (TObject)base.Take().Item;
        }

        /// <summary>
        /// Returns first TObject from the Heap
        /// </summary>
        /// <returns>Object of first item.</returns>
        public new virtual TObject Peek()
        {
            return (TObject)base.Peek().Item;
        }
    }

    /// <summary>
    /// MinBinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object implementing IComparable<TObject> interface</TObject></typeparam>
    public class MinBinaryHeap<TObject> : BinaryHeap<TObject> where TObject : IComparable<TObject>
    {
        #region Constructors
        /// <summary>
        /// MinBinaryHeap default constructor
        /// </summary>
        public MinBinaryHeap() : base(BinaryHeapType.MinHeap) { }

        /// <summary>
        /// MinBinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="capacity"></param>
        public MinBinaryHeap(int capacity) : base(BinaryHeapType.MinHeap, capacity) { }
        #endregion
    }

    /// <summary>
    /// MinBinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object to store</typeparam>
    /// <typeparam name="TValue">Type of value associated to the object, implementing IComparable interface</typeparam>
    public class MinBinaryHeap<TObject, TValue> : BinaryHeap<TObject, TValue> where TValue : IComparable
    {
        #region Constructors
        /// <summary>
        /// MinBinaryHeap default constructor
        /// </summary>
        public MinBinaryHeap() : base(BinaryHeapType.MinHeap) { }

        /// <summary>
        /// MinBinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="capacity"></param>
        public MinBinaryHeap(int capacity) : base(BinaryHeapType.MinHeap, capacity) { }
        #endregion
    }

    /// <summary>
    /// MaxBinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object implementing IComparable<TObject> interface</TObject></typeparam>
    public class MaxBinaryHeap<TObject> : BinaryHeap<TObject> where TObject : IComparable<TObject>
    {
        #region Constructors
        /// <summary>
        /// MaxBinaryHeap default constructor
        /// </summary>
        public MaxBinaryHeap() : base(BinaryHeapType.MaxHeap) { }

        /// <summary>
        /// MaxBinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="capacity"></param>
        public MaxBinaryHeap(int capacity) : base(BinaryHeapType.MaxHeap, capacity) { }
        #endregion
    }

    /// <summary>
    /// MaxBinaryHeap class
    /// </summary>
    /// <typeparam name="TObject">Type of object to store</typeparam>
    /// <typeparam name="TValue">Type of value associated to the object, implementing IComparable interface</typeparam>
    public class MaxBinaryHeap<TObject, TValue> : BinaryHeap<TObject, TValue> where TValue : IComparable
    {
        #region Constructors
        /// <summary>
        /// MaxBinaryHeap default constructor
        /// </summary>
        public MaxBinaryHeap() : base(BinaryHeapType.MaxHeap) { }

        /// <summary>
        /// MaxBinaryHeap constructor with initial capacity
        /// </summary>
        /// <param name="capacity"></param>
        public MaxBinaryHeap(int capacity) : base(BinaryHeapType.MaxHeap, capacity) { }
        #endregion
    }

    /// <summary>
    /// Class to use in BinaryHeaps when the object is not of type IComparable
    /// </summary>
    public class HeapItem : IEquatable<HeapItem>, IComparable<HeapItem>
    {
        #region Properties
        /// <summary>
        /// Object to store
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Item's associated value
        /// </summary>
        public IComparable Value { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// HeapItem default constructor
        /// </summary>
        /// <param name="data">Object to store</param>
        /// <param name="value">Associated value</param>
        public HeapItem(object data, IComparable value)
        {
            Item = data;
            Value = value;
        } 
        #endregion

        /// <summary>
        /// Method to set/update HeapItem's value
        /// </summary>
        /// <param name="newValue"></param>
        public void SetValue(IComparable newValue)
        {
            Value = newValue;
        }

        #region Override IComparable Methods
        /// <summary>
        /// HeapItem's equality comparer
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Equals(HeapItem obj)
        {
            return this.Item.Equals(obj.Item);
        }

        /// <summary>
        /// HeapItem's HashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Item.GetHashCode();
        }

        /// <summary>
        /// Implementation of IComparable interface
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(HeapItem obj)
        {
            return this.Value.CompareTo(obj.Value);
        }

        /// <summary>
        /// IComparable less than operator
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator <(HeapItem item1, HeapItem item2)
        {
            return item1.CompareTo(item2) < 0;
        }

        /// <summary>
        /// IComparable greater than operator
        /// </summary>
        /// <param name="item1"></param>
        /// <param name="item2"></param>
        /// <returns></returns>
        public static bool operator >(HeapItem item1, HeapItem item2)
        {
            return item1.CompareTo(item2) > 0;
        }
        #endregion

        /// <summary>
        /// HeapItem's string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[Item: {0}, Value: {1}", Item.ToString(), Value.ToString());
        }
    }
}
