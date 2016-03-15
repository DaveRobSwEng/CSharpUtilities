#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// ArrayTypeValueContainer.cs
//  Implementation of the Class ArrayTypeValueContainer
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Container for values held in an array. Implements IEnumerable and some extra
    /// facilities to support iterating over n-dimensional arrays
    /// </summary>
    public class ArrayValueContainer : IEnumerable<Value>
    {
        /// <summary>
        /// Gets the count of elements stored in the array
        /// </summary>
        public int Count
        {
            get
            {
                if (m_ValuesList != null)
                {
                    return m_ValuesList.Count;
                }
                else
                {
                    int count = 1;
                    foreach (var item in m_ArrayBounds)
                    {
                        count *= item;
                    }

                    return count;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayValueContainer"/> class.
        /// The containing array is allocated but contains only null references
        /// </summary>
        /// <param name="arrayBounds">The array bounds.</param>
        public ArrayValueContainer(int[] arrayBounds)
        {
            m_ArrayBounds = new int[arrayBounds.Length];
            arrayBounds.CopyTo(m_ArrayBounds, 0);

            AllocateArray();
        }

        #region IEnumerable<Value> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Value> GetEnumerator()
        {
            if (m_ValuesList != null)
            {
                return m_ValuesList.GetEnumerator();
            }
            else
            {
                return GetIndexEnumerator();
            }
        }

        /// <summary>
        /// Gets the enumerator that iterates over the set of indices
        /// </summary>
        /// <returns>Enumerator that iterates over the set of indices</returns>
        private IEnumerator<Value> GetIndexEnumerator()
        {
            int[] indices = new int[m_ArrayBounds.Length];

            do
            {
                yield return this[indices];
            }
            while (IncrementIndices(indices));
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

        /// <summary>
        /// Gets or sets the <see cref="Sepura.DataDictionary.Value"/> with the specified indices.
        /// The length of the indices array must match the rank of the type
        /// </summary>
        /// <param name="indices">Array containing an index for each dimension of the array</param>
        /// <returns>The value at the specified index</returns>
        public Value this[int[] indices]
        {
            get
            {
                Value theValue = null;
                if (indices.Length != m_ArrayBounds.Length && m_ArrayBounds.Length != 0)
                {
                    throw new DataDictionaryException(string.Format("Rank must be {0}", m_ArrayBounds.Length));
                }

                try
                {
                    switch (m_ArrayBounds.Length)
                    {
                        case 0:
                            theValue = m_ValuesList[indices[0]];
                            break;

                        case 1:
                            theValue = m_ValuesRank1[indices[0]];
                            break;

                        case 2:
                            theValue = m_ValuesRank2[indices[0], indices[1]];
                            break;

                        case 3:
                            theValue = m_ValuesRank3[indices[0], indices[1], indices[2]];
                            break;

                        default:
                            throw new DataDictionaryException(string.Format("Arrays of rank {0} not supported - maximum is 3",
                                m_ArrayBounds.Length));
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new DataDictionaryException("Attempt to access an array member that does not exist", ex);
                }

                return theValue;
            }

            set
            {
                if (indices.Length != m_ArrayBounds.Length)
                {
                    throw new DataDictionaryException(string.Format("Rank must be {0}", m_ArrayBounds.Length));
                }

                try
                {
                    switch (m_ArrayBounds.Length)
                    {
                        case 0:
                            m_ValuesList = new List<Value>();
                            break;
                        case 1:
                            m_ValuesRank1[indices[0]] = value;
                            break;

                        case 2:
                            m_ValuesRank2[indices[0], indices[1]] = value;
                            break;

                        case 3:
                            m_ValuesRank3[indices[0], indices[1], indices[2]] = value;
                            break;

                        default:
                            throw new DataDictionaryException(string.Format("Arrays of rank {0} not supported - maximum is 3",
                                m_ArrayBounds.Length));
                    }
                }
                catch (IndexOutOfRangeException ex)
                {
                    throw new DataDictionaryException("Attempt to access an array member that does not exist", ex);
                }
            }
        }

        /// <summary>
        /// Populates the array using specified populate function to create each member.
        /// </summary>
        /// <param name="thePopulateFunction">The populate function.</param>
        public void Populate(Func<Value> thePopulateFunction)
        {
            int[] indices = new int[m_ArrayBounds.Length];

            do
            {
                this[indices] = thePopulateFunction();
            }
            while (IncrementIndices(indices));
        }

        /// <summary>
        /// Iterates over the n-dimensional array of Values
        /// </summary>
        /// <param name="doItem">The action to do on each item.</param>
        public void Iterate(Action<Value, int[]> doItem)
        {
            int[] indices = new int[m_ArrayBounds.Length];

            do
            {
                doItem(this[indices], indices);
            }
            while (IncrementIndices(indices));
        }

        /// <summary>
        /// Increments the indices, rightmost is fastest varying
        /// </summary>
        /// <param name="indices">The indices.</param>
        /// <returns>True if the index set was successfully incremented, else false</returns>
        public bool IncrementIndices(int[] indices)
        {
            bool incrementedOk = false;

            int rank = m_ArrayBounds.Length - 1;

            // First increment the right-most index. If this exceeds the limit for that index then
            // set to zero and increment the next left. 
            while (!incrementedOk && rank >= 0)
            {
                if (++indices[rank] >= m_ArrayBounds[rank])
                {
                    indices[rank] = 0;
                    --rank;
                }
                else
                {
                    incrementedOk = true;
                }
            }

            return incrementedOk;
        }

        /// <summary>
        /// Adds the specified value to the list.
        /// Used for containers where the rank of the type is zero and therefore the collection
        /// container is grown dynamically
        /// </summary>
        /// <param name="theValue">The value.</param>
        public void Add(Value theValue)
        {
            m_ValuesList.Add(theValue);
        }

        /// <summary>
        /// Allocates the (empty) array according to the rank of the array type.
        /// </summary>
        private void AllocateArray()
        {
            switch (m_ArrayBounds.Length)
            {
                case 0:
                    // Array contents are added dynamically to the container 
                    m_ValuesList = new List<Value>();
                    break;

                case 1:
                    m_ValuesRank1 = new Value[m_ArrayBounds[0]];
                    break;

                case 2:
                    m_ValuesRank2 = new Value[m_ArrayBounds[0], m_ArrayBounds[1]];
                    break;

                case 3:
                    m_ValuesRank3 = new Value[m_ArrayBounds[0], m_ArrayBounds[1], m_ArrayBounds[2]];
                    break;

                default:
                    throw new DataDictionaryException(string.Format("Arrays of rank {0} not supported - maximum is 3",
                        m_ArrayBounds.Length));
            }
        }

        /// <summary>
        /// The array bounds - one element in the array for each dimension
        /// </summary>
        private int[] m_ArrayBounds = new int[0];

        /// <summary>
        /// The values list - used for arrays of undefined size (but rank assumed to be 1)
        /// </summary>
        private List<Value> m_ValuesList;

        /// <summary>
        /// The store for values in arrays of rank 1
        /// </summary>
        private Value[] m_ValuesRank1;

        /// <summary>
        /// The store for values in arrays of rank 2
        /// </summary>
        private Value[,] m_ValuesRank2;

        /// <summary>
        /// The store for values in arrays of rank 3
        /// </summary>
        private Value[,,] m_ValuesRank3;
    }
}