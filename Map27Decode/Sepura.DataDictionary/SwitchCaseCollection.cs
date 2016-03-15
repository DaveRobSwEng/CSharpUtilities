#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// SwitchCaseCollection.cs
//  Implementation of the Class SwitchCaseCollection
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Sepura.Utilities;

    /// <summary>
    /// Container for a collection of SwitchCaseDefinition values. Indexable by integer
    /// value or by enum literal
    /// </summary>
    public class SwitchCaseCollection : IEnumerable<SwitchCaseDefinition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchCaseCollection"/> class.
        /// </summary>
        /// <param name="theSelectionEnum">The selection enum - may be null</param>
        public SwitchCaseCollection(EnumDefinition theSelectionEnum)
        {
            m_SelectionEnum = theSelectionEnum;
        }

        /// <summary>
        /// Adds the specified the switch case definition.
        /// </summary>
        /// <param name="theSwitchCaseDefinition">The switch case definition.</param>
        public void Add(SwitchCaseDefinition theSwitchCaseDefinition)
        {
            // case may be identified by enum literal name or by integer value
            int caseIntegerValue = 0;
            if (Formatting.TryGetInt(theSwitchCaseDefinition.Value, out caseIntegerValue))
            {
                // Case value specified as an integer
                if (m_SelectionEnum != null)
                {
                    string enumLiteral = string.Empty;
                    if (!m_SelectionEnum.TryGetLiteral(caseIntegerValue, out enumLiteral))
                    {
                        throw new DataDictionaryException("Enum {0} contains no literal for {1}", m_SelectionEnum.Name, caseIntegerValue);
                    }

                    m_SwitchCaseDefinitionsByName[enumLiteral] = theSwitchCaseDefinition;
                }

                m_SwitchCaseDefinitionsByValue[caseIntegerValue] = theSwitchCaseDefinition;                
            }
            else
            {
                // Case value specified as an enum literal
                if (!m_SelectionEnum.TryGetInteger(theSwitchCaseDefinition.Value, out caseIntegerValue))
                {
                    throw new DataDictionaryException("Enum {0} contains no literal {1}", m_SelectionEnum.Name, theSwitchCaseDefinition.Value);
                }

                m_SwitchCaseDefinitionsByName[theSwitchCaseDefinition.Value] = theSwitchCaseDefinition;
                m_SwitchCaseDefinitionsByValue[caseIntegerValue] = theSwitchCaseDefinition;
            }
        }

        /// <summary>
        /// Adds the range of SwitchCaseDefinitions
        /// </summary>
        /// <param name="theCaseDefinitions">The case definitions.</param>
        public void AddRange(IEnumerable<SwitchCaseDefinition> theCaseDefinitions)
        {
            foreach (var caseDefinition in theCaseDefinitions)
            {
                Add(caseDefinition);
            }
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.SwitchCaseDefinition"/> with the specified literal.
        /// Throws if the value is not found
        /// </summary>
        /// <param name="literal">Index as enum literal value</param>
        /// <returns>SwitchCaseDefinition matching the literal</returns>
        public SwitchCaseDefinition this[string literal]
        {
            get
            {
                SwitchCaseDefinition theCase = null;
                if (m_SwitchCaseDefinitionsByName.TryGetValue(literal, out theCase))
                {
                    return theCase;
                }

                throw new DataDictionaryException("Unrecognised case value: {1}", m_SelectionEnum.Name, literal);
            }
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.SwitchCaseDefinition"/> with the specified integer value.
        /// Throws if the value is not found
        /// </summary>
        /// <param name="intValue">Index as integer value</param>
        /// <returns>SwitchCaseDefinition matching the integer value</returns>
        public SwitchCaseDefinition this[int intValue]
        {
            get
            {
                SwitchCaseDefinition theCase = null;
                if (m_SwitchCaseDefinitionsByValue.TryGetValue(intValue, out theCase))
                {
                    return theCase;
                }

                throw new DataDictionaryException("Unrecognised case value: {1}", m_SelectionEnum.Name, intValue);
            }
        }

        /// <summary>
        /// Attempts to get the <see cref="Sepura.DataDictionary.SwitchCaseDefinition"/> with the specified literal.
        /// </summary>
        /// <param name="literal">Index as enum literal value</param>
        /// <param name="theCase">The case.</param>
        /// <returns>
        /// True if the value is found, else false
        /// </returns>
        public bool TryGetValue(string literal, out SwitchCaseDefinition theCase)
        {
            theCase = null;
            return m_SwitchCaseDefinitionsByName.TryGetValue(literal, out theCase);
        }

        /// <summary>
        /// Gets the <see cref="Sepura.DataDictionary.SwitchCaseDefinition"/> with the specified integer value.
        /// Throws if the value is not found
        /// </summary>
        /// <param name="intValue">Index as integer value</param>
        /// <param name="theCase">The case.</param>
        /// <returns>
        /// SwitchCaseDefinition matching the integer value
        /// </returns>
        public bool TryGetValue(int intValue, out SwitchCaseDefinition theCase)
        {
            theCase = null;
            return m_SwitchCaseDefinitionsByValue.TryGetValue(intValue, out theCase);
        }

        /// <summary>
        /// The enum used to control the selection
        /// </summary>
        private EnumDefinition m_SelectionEnum;

        /// <summary>
        /// The switch case definitions indexed by discriminator name
        /// </summary>
        private Dictionary<string, SwitchCaseDefinition> m_SwitchCaseDefinitionsByName = new Dictionary<string, SwitchCaseDefinition>();

        /// <summary>
        /// The switch case definitions indexed by discriminator value
        /// </summary>
        private Dictionary<int, SwitchCaseDefinition> m_SwitchCaseDefinitionsByValue = new Dictionary<int, SwitchCaseDefinition>();

        #region IEnumerable<SwitchCaseDefinition> Members

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<SwitchCaseDefinition> GetEnumerator()
        {
            return m_SwitchCaseDefinitionsByValue.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}