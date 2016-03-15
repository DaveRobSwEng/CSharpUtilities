#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// ObservableList.cs
//  Implementation of the Class ObservableList
//
// Copyright (c) 2011 Sepura Plc
// All Rights reserved.
//
//  Original author: RobinsonD
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// List class that raises CollectionChanged when items are added or removed
    /// Differs from ObservableCollection in that multiple elements may be added simultaneously
    /// but raising only one event
    /// </summary>
    /// <typeparam name="T">Type of the item to store in the list</typeparam>
    public class ObservableList<T>
        : INotifyCollectionChanged, 
        INotifyPropertyChanged, 
        IList<T>,
        IList
    {
        /// <summary>
        /// Removes all entries from the list and raises the CollectionChanged event
        /// </summary>
        public void Clear()
        {
            m_List = new List<T>();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds the range of items to the list and raises the CollectionChanged event
        /// </summary>
        /// <param name="range">The range.</param>
        public void AddRange(List<T> range)
        {
            m_List.AddRange(range);

            // Have to raise Reset event because CollectionView classes don't support notifications of ranges of 
            // items being changed.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Replaces the contents of the list with the new list and raises the CollectionChanged event
        /// </summary>
        /// <param name="range">The range.</param>
        public void SetContents(List<T> range)
        {
            m_List = range;

            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");

            // Have to raise Reset event because CollectionView classes don't support notifications of ranges of 
            // items being changed.
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Adds the specified item to the collection and raises the CollectionChanged event
        /// </summary>
        /// <param name="theItem">The item.</param>
        public void Add(T theItem)
        {
            int insertionIndex = m_List.Count;
            m_List.Add(theItem);
            OnPropertyChanged("Count");
            OnPropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, theItem, insertionIndex));
        }

        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        /// <param name="args">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var handler = CollectionChanged;

            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// The list of items
        /// </summary>
        private List<T> m_List = new List<T>();

        #region INotifyCollectionChanged Members

        /// <summary>
        /// Occurs when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IEnumerable<T> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return m_List.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T item)
        {
            return m_List.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void Insert(int index, T item)
        {
            m_List.Insert(index, item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void RemoveAt(int index)
        {
            m_List.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">Index into the list</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        public T this[int index]
        {
            get
            {
                return m_List[index];
            }

            set
            {
                m_List[index] = value;
            }
        }

        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// Returns true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains(T item)
        {
            return m_List.Contains(item);
        }

        /// <summary>
        /// Copies to.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            m_List.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count
        {
            get { return m_List.Count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only; otherwise, false.</returns>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// Returns true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove(T item)
        {
            return m_List.Remove(item);
        }

        #endregion

        #region IList Members

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The object to add to the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The position into which the new element was inserted, or -1 to indicate that the item was not inserted into the collection,
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public int Add(object value)
        {
            m_List.Add((T)value);
            return m_List.Count - 1;
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.IList"/> contains a specific value.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// Returns true if the <see cref="T:System.Object"/> is found in the <see cref="T:System.Collections.IList"/>; otherwise, false.
        /// </returns>
        public bool Contains(object value)
        {
            return m_List.Contains((T)value);
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The object to locate in the <see cref="T:System.Collections.IList"/>.</param>
        /// <returns>
        /// The index of <paramref name="value"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(object value)
        {
            // Newer version of .NET PresentationFramework.dll seen sometimes to try to get index of ItemInfo object 
            // rather than the contained Item
            if (value is T)
            {
                return m_List.IndexOf((T)value);                
            }

            return -1;
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.IList"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="value"/> should be inserted.</param>
        /// <param name="value">The object to insert into the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.IList"/>. </exception>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        /// <exception cref="T:System.NullReferenceException">
        /// <paramref name="value"/> is null reference in the <see cref="T:System.Collections.IList"/>.</exception>
        public void Insert(int index, object value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.IList"/> has a fixed size.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Collections.IList"/> has a fixed size; otherwise, false.</returns>
        public bool IsFixedSize
        {
            get { throw new System.NotImplementedException(); }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.IList"/>.
        /// </summary>
        /// <param name="value">The object to remove from the <see cref="T:System.Collections.IList"/>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.IList"/> is read-only.-or- The <see cref="T:System.Collections.IList"/> has a fixed size. </exception>
        public void Remove(object value)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">Index into the list</param>
        /// <returns>The element at the specified index.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">Index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        /// <exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        object IList.this[int index]
        {
            get
            {
                return m_List[index];
            }

            set
            {
                throw new System.NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.ICollection"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   <paramref name="array"/> is null. </exception>
        ///   <para/>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        ///   <paramref name="index"/> is less than zero. </exception>
        /// <exception cref="T:System.ArgumentException">
        ///   <paramref name="array"/> is multidimensional.-or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is greater than the available space from <paramref name="index"/> to the end of the destination <paramref name="array"/>. </exception>
        /// <exception cref="T:System.ArgumentException">The type of the source <see cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the destination <paramref name="array"/>. </exception>
        public void CopyTo(System.Array array, int index)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe).
        /// </summary>
        /// <returns>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized (thread safe); otherwise, false.</returns>
        public bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.
        /// </summary>
        /// <returns>An object that can be used to synchronize access to the <see cref="T:System.Collections.ICollection"/>.</returns>
        public object SyncRoot
        {
            get { return m_SyncRoot; }
        }

        /// <summary>
        /// The sync root
        /// </summary>
        private object m_SyncRoot = new object();

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
