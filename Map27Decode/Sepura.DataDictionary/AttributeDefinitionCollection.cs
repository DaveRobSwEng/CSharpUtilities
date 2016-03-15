#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// AttributeDefinitionCollection.cs
//  Implementation of the Class AttributeDefinitionCollection
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
    /// Collection of AttributeDefinitions indexed by name or index within the list
    /// </summary>
    public class AttributeDefinitionCollection : IEnumerable<AttributeDefinition>
    {           
        /// <summary>
        /// Gets the number of items in the collection
        /// </summary>
        public int Count
        {
            get
            {
                return m_DefinitionsList.Count;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeDefinitionCollection"/> class.
        /// </summary>
        /// <param name="parentStructureName">Name of the parent structure.</param>
        public AttributeDefinitionCollection(string parentStructureName)
        {
            m_ParentStructureName = parentStructureName;
        }

        /// <summary>
        /// Adds the specified the attribute value to the collection
        /// </summary>
        /// <param name="theAttributeDefinition">The attribute value.</param>
        public void Add (AttributeDefinition theAttributeDefinition)
        {
            m_Definitions.Add(theAttributeDefinition.Name, theAttributeDefinition);
            m_DefinitionsList.Add(theAttributeDefinition);
        }
        
        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.Value"/> with the specified attribute name.
        /// </summary>
        /// <param name="attributeName">Index by name into the attribute collection</param>
        /// <returns>Attribute specified by name</returns>
        public AttributeDefinition this[string attributeName]
        {
            get
            {
                AttributeDefinition theValue = null;
                if (!m_Definitions.TryGetValue(attributeName, out theValue))
                {
                    throw new DataDictionaryException("Structure {0} has no member named {1}",  m_ParentStructureName, attributeName);
                }

                return theValue;
            }
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.Value"/> with the specified attribute index.
        /// </summary>
        /// <param name="attributeIndex">Index into the attribute collection</param>
        /// <returns>Attribute specified by index</returns>
        public AttributeDefinition this[int attributeIndex]
        {
            get
            {
                if (attributeIndex >= m_DefinitionsList.Count || attributeIndex < 0)
                {
                    throw new DataDictionaryException("Structure {0}: Index {1} is outside the range 0:{2}", 
                            m_ParentStructureName, attributeIndex, m_DefinitionsList.Count - 1);
                }

                return m_DefinitionsList[attributeIndex];
            }
        }

        /// <summary>
        /// Collection of the attribute definitions indexed by name
        /// </summary>
        private Dictionary<string, AttributeDefinition> m_Definitions = new Dictionary<string, AttributeDefinition>();

        /// <summary>
        /// Collection of the attribute definitions held in attribute order
        /// </summary>
        private List<AttributeDefinition> m_DefinitionsList = new List<AttributeDefinition>();

        /// <summary>
        /// The name of the parent structure
        /// </summary>
        private string m_ParentStructureName;

        #region IEnumerable<AttributeDefinition> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<AttributeDefinition> GetEnumerator()
        {
            return m_DefinitionsList.GetEnumerator();
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