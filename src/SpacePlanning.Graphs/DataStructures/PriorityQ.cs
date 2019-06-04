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
    /// Binary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable and IComparable interface</typeparam>
    public class PriorityQ<TObject> : BinaryHeap<TObject> where TObject : IEquatable<TObject>, IComparable<TObject>
    {
        #region Private Properties
        internal Dictionary<TObject, int> heapIndices { get; private set; }
        #endregion

        #region Public Properties
        /// <summary>
        /// HeapIndices dictionary. For testing purposes only.
        /// </summary>
        public Dictionary<TObject, int> HeapIndices { get { return heapIndices; } } 
        #endregion

        #region Constructors

        /// <summary>
        /// PriorityQ default constructor
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        public PriorityQ(BinaryHeapType heapType) : base(heapType)
        {
            heapIndices = new Dictionary<TObject, int>(this.Capacity);
        }

        /// <summary>
        /// PriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        /// <param name="capacity">Initial capacity</param>
        public PriorityQ(BinaryHeapType heapType, int capacity) : base(heapType, capacity)
        {
            heapIndices = new Dictionary<TObject, int>(this.Capacity);
        }

        #endregion

        #region Private Methods
        internal override void Swap(int firstIndex, int secondIndex)
        {
            var firstItem = _heapItems[firstIndex];
            var secondItem = _heapItems[secondIndex];
            _heapItems[firstIndex] = secondItem;
            _heapItems[secondIndex] = firstItem;
            heapIndices[firstItem] = secondIndex;
            heapIndices[secondItem] = firstIndex;
        }
        #endregion

        /// <summary>
        /// Adds a new TObject to the Heap
        /// </summary>
        /// <param name="item">TObject to add</param>
        public override void Add(TObject item)
        {
            heapIndices[item] = this.Size;
            base.Add(item);
        }

        /// <summary>
        /// Adds a range of TObjects to the Heap
        /// </summary>
        /// <param name="items"></param>
        public override void AddRange(IEnumerable<TObject> items)
        {
            foreach(TObject item in items)
            {
                heapIndices[item] = this.Size;
                base.Add(item);
            }
        }

        /// <summary>
        /// Updates the TObject item and restores the Heap property
        /// </summary>
        /// <param name="index">Existing TObject in Heap with new comparison value</param>
        public void UpdateAtIndex(int index)
        {
            //if (!heapIndices.ContainsKey(newItem)) { throw new ArgumentException("Element not existing in Priority Queue"); }
            //int heapIndex = heapIndices[newItem];
            TObject item = _heapItems[index];
            //IComparable currentValue = heapItem.Value;
            //heapItem.SetValue(newValue);

            int comparison = item.CompareTo(this.Parent(index));
            // Update item in Heap
            //_heapItems[heapIndex] = newItem;

            if ( (HeapType == BinaryHeapType.MinHeap && comparison < 0) ||
                (HeapType == BinaryHeapType.MaxHeap && comparison > 0))
            {
                HeapifyUp(index);
            }
            else
            {
                HeapifyDown(index);
            }
        }

        /// <summary>
        /// Returns the first item on the Heap
        /// </summary>
        /// <returns></returns>
        public new TObject Peek()
        {
            return (TObject)base.Peek();
        }

        /// <summary>
        /// Returns the first item 
        /// </summary>
        /// <returns></returns>
        public new TObject Take()
        {
            TObject first = (TObject)base.Take();
            heapIndices.Remove(first);
            return first;
        }

        /// <summary>
        /// Returns the index of an item. If item no in PriorityQ, returns -1
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int IndexOf(TObject item)
        {
            if (heapIndices.ContainsKey(item))
            {
                return heapIndices[item];
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Removes all items from the queue
        /// </summary>
        public override void Clear()
        {
            heapIndices.Clear();
            base.Clear();
        }
    }

    /// <summary>
    /// Binary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class PriorityQ<TObject, TValue> : PriorityQ<HeapItem> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Constructors
        /// <summary>
        /// PriorityQ default constructor
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        public PriorityQ(BinaryHeapType heapType) : base(heapType) { }

        /// <summary>
        /// PriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="heapType">MinHeap or MaxHeap</param>
        /// <param name="capacity">Initial capacity</param>
        public PriorityQ(BinaryHeapType heapType, int capacity) : base(heapType, capacity) { }
        #endregion

        /// <summary>
        /// Adds a new TObject with an associated TValue to the Heap
        /// </summary>
        /// <param name="item">TObject</param>
        /// <param name="value">TValue</param>
        public void Add(TObject item, TValue value)
        {
            base.Add(new HeapItem(item, value));
        }

        /// <summary>
        /// Returns first TObject from the Heap
        /// </summary>
        /// <returns>Object of first item.</returns>
        public new TObject Peek()
        {
            return (TObject)base.Peek().Item;
        }

        /// <summary>
        /// Returns the value associated with the first TObject from the Heap
        /// </summary>
        /// <returns></returns>
        public TValue PeekValue()
        {
            return (TValue)base.Peek().Value;
        }

        /// <summary>
        /// Returns the first TObject and removes it from the Heap
        /// </summary>
        /// <returns>First TObject</returns>
        public new TObject Take()
        {
            return (TObject)base.Take().Item;
        }

        /// <summary>
        /// Gets the associated value to a given TObject
        /// </summary>
        /// <param name="item">TObject</param>
        /// <returns>Associated value</returns>
        public TValue GetValue(TObject item)
        {
            HeapItem tempItem = new HeapItem(item, null);
            if (!heapIndices.ContainsKey(tempItem)) { throw new ArgumentException("Element not existing in Priority Queue"); }
            int heapIndex = heapIndices[tempItem];
            return (TValue)_heapItems[heapIndex].Value;
        }

        /// <summary>
        /// Updates the TObject item and restores the Heap property
        /// </summary>
        /// <param name="item">TObject in Heap </param>
        /// <param name="value">Value to update object with</param>
        public void UpdateItem(TObject item, TValue value)
        {
            HeapItem newItem = new HeapItem(item, value);
            if (!heapIndices.ContainsKey(newItem)) { throw new ArgumentException("Element not existing in Priority Queue"); }
            int heapIndex = heapIndices[newItem];
            _heapItems[heapIndex] = newItem;
            base.UpdateAtIndex(heapIndex);
        }
    }

    /// <summary>
    /// MinBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable and IComparable interfaces</typeparam>
    public class MinPriorityQ<TObject> : PriorityQ<TObject> where TObject : IEquatable<TObject>, IComparable<TObject>
    {
        #region Constructor
        /// <summary>
        /// MinPriorityQ default constructor
        /// </summary>
        public MinPriorityQ() : base(BinaryHeapType.MinHeap) { }

        /// <summary>
        /// MinPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MinPriorityQ(int capacity) : base(BinaryHeapType.MinHeap, capacity) { }

        #endregion
    }

    /// <summary>
    /// MinBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class MinPriorityQ<TObject, TValue> : PriorityQ<TObject, TValue> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Constructor
        /// <summary>
        /// MinPriorityQ default constructor
        /// </summary>
        public MinPriorityQ() : base(BinaryHeapType.MinHeap) { }

        /// <summary>
        /// MinPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MinPriorityQ(int capacity) : base(BinaryHeapType.MinHeap, capacity) { }

        #endregion
    }

    /// <summary>
    /// MaxBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class MaxPriorityQ<TObject> : PriorityQ<TObject> where TObject : IEquatable<TObject>, IComparable<TObject>
    {
        #region Constructor
        /// <summary>
        /// MaxPriorityQ default constructor
        /// </summary>
        public MaxPriorityQ() : base(BinaryHeapType.MaxHeap) { }

        /// <summary>
        /// MaxPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MaxPriorityQ(int capacity) : base(BinaryHeapType.MaxHeap, capacity) { }

        #endregion
    }

    /// <summary>
    /// MaxBinary Priority Queue
    /// </summary>
    /// <typeparam name="TObject">Type of object to store implementing IEquatable interface</typeparam>
    /// <typeparam name="TValue">Type of value associated with TObject implementing IComparable</typeparam>
    public class MaxPriorityQ<TObject, TValue> : PriorityQ<TObject, TValue> where TObject : IEquatable<TObject> where TValue : IComparable
    {
        #region Constructor
        /// <summary>
        /// MaxPriorityQ default constructor
        /// </summary>
        public MaxPriorityQ() : base(BinaryHeapType.MaxHeap) { }

        /// <summary>
        /// MaxPriorityQ constructor with initial capacity.
        /// </summary>
        /// <param name="capacity">Initial capacity</param>
        public MaxPriorityQ(int capacity) : base(BinaryHeapType.MaxHeap, capacity) { }

        #endregion
    }


}
