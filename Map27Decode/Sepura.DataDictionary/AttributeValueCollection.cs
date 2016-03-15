#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// AttributeValueCollection.cs
//  Implementation of the Class AttributeValueCollection
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
    /// Collection of AttributeValue objects
    /// </summary>
    public class AttributeValueCollection : IEnumerable<AttributeValue>
    {
        /// <summary>
        /// Gets the number of items in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return m_ValuesList.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueCollection"/> class.
        /// </summary>
        /// <param name="parentStructureName">Name of the parent structure.</param>
        public AttributeValueCollection(string parentStructureName)
        {
            m_ParentStructureName = parentStructureName;
        }

        /// <summary>
        /// Adds the specified the attribute value to the collection
        /// </summary>
        /// <param name="theAttributeValue">The attribute value.</param>
        public void Add(AttributeValue theAttributeValue)
        {
            m_Values.Add(theAttributeValue.Name, theAttributeValue);
            m_ValuesList.Add(theAttributeValue);
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.Value"/> with the specified attribute name.
        /// </summary>
        /// <param name="attributeName">Index by name into the attribute collection</param>
        /// <returns>Attribute specified by name</returns>
        public AttributeValue this[string attributeName]
        {
            get
            {
                AttributeValue theValue = null;
                if (!m_Values.TryGetValue(attributeName, out theValue))
                {
                    throw new DataDictionaryException("Structure {0} has no member named {1}", m_ParentStructureName, attributeName);
                }

                return theValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.Value"/> with the specified attribute index.
        /// </summary>
        /// <param name="attributeIndex">Index into the attribute collection</param>
        /// <returns>Attribute specified by index</returns>
        public AttributeValue this[int attributeIndex]
        {
            get
            {
                if (attributeIndex >= m_ValuesList.Count || attributeIndex < 0)
                {
                    throw new DataDictionaryException("Structure {0}: Index {1} is outside the range 0:{2}",
                            m_ParentStructureName, attributeIndex, m_ValuesList.Count - 1);
                }

                return m_ValuesList[attributeIndex];
            }
        }

        /// <summary>
        /// Attempts to get the <see cref="Sepura.DataDictionary.Value"/> with the specified attribute name.
        /// </summary>
        /// <param name="attributeName">Index by name into the attribute collection</param>
        /// <param name="theValue">The value.</param>
        /// <returns>
        /// Attribute specified by name
        /// </returns>
        public bool TryGetValue(string attributeName, out AttributeValue theValue)
        {
            return m_Values.TryGetValue(attributeName, out theValue);
        }

        /// <summary>
        /// Collection of the attribute values indexed by name
        /// </summary>
        private Dictionary<string, AttributeValue> m_Values = new Dictionary<string, AttributeValue>();

        /// <summary>
        /// Collection of the attribute values held in attribute order
        /// </summary>
        private List<AttributeValue> m_ValuesList = new List<AttributeValue>();

        /// <summary>
        /// The name of the parent structure
        /// </summary>
        private string m_ParentStructureName;

        #region IEnumerable<Value> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<AttributeValue> GetEnumerator()
        {
            return m_ValuesList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}