#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// DepthFirstArrayIterator.cs
//  Implementation of the Class DepthFirstArrayIterator
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary 
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Class implementing a depth-first iterator for an N-dimensional array
    /// </summary>
    public class DepthFirstArrayIterator : IEnumerable<int []>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DepthFirstArrayIterator"/> class.
        /// </summary>
        /// <param name="arrayDimensions">The array dimensions.</param>
        public DepthFirstArrayIterator(int [] arrayDimensions)
        {
            m_ArrayDimensions = new int[arrayDimensions.Length];
            arrayDimensions.CopyTo (m_ArrayDimensions, 0);
        }

        #region IEnumerable<int> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<int []> GetEnumerator()
        {
            int currentDepth = 0;
            int []workingArray = new int[m_ArrayDimensions.Length];

            bool done = false;

            while (!done)
            {
                // Depth first through each
                while (currentDepth < m_ArrayDimensions.Length)
                {
                    int[] array = new int[currentDepth + 1];
                    for (int i = 0; i <= currentDepth; i++)
                    {
                        array[i] = workingArray[i];
                    }

                    yield return array;
                    ++currentDepth;
                }

                // Reached a leaf node - backtrack and increment the value in the array
                bool incrementOk = false;
                --currentDepth;
                while (!incrementOk && currentDepth >= 0)
                {
                    workingArray[currentDepth] += 1;
                    if (workingArray[currentDepth] >= m_ArrayDimensions[currentDepth])
                    {
                        workingArray[currentDepth] = 0;
                        --currentDepth;
                    }
                    else
                    {
                        incrementOk = true;
                    }
                }

                done |= currentDepth < 0;
            }
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
        /// The array dimensions
        /// </summary>
        private int[] m_ArrayDimensions;
    }
}