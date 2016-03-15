#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// EnumDefinition.cs
//  Implementation of the Class EnumDefinition
//
// Copyright (c) 2010 Sepura Plc
// All Rights reserved.
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
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Xml.Linq;

    /// <summary>
    /// Class representing an enum definition
    /// </summary>
    public class EnumDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the collection of literal associated with this enumeration
        /// </summary>
        public ReadOnlyCollection<LiteralDefinition> Literals
        {
            get
            {
                return m_Literals;
            }
        }

        /// <summary>
        /// Gets the underlying integer (base) type.
        /// </summary>
        /// <value>
        /// The type of the base.
        /// </value>
        public TypeDefinition BaseType
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the EnumDefinition class
        /// Populates this object with information extracted from the XML
        /// </summary>
        /// <param name="theNode">XML node describing the Enum object</param>
        /// <param name="manager">The data dictionary manager constructing this type.</param>
        public EnumDefinition(XElement theNode, DictionaryManager manager)
            : base(theNode, TypeId.EnumType)
        {
            // ByteSize may be set either directly as an integer or by inference from the Type field.
            // If no Type field is present then the type is assumed to be 'int'
            if (theNode.Element("ByteSize") != null)
            {
                SetByteSize(theNode.Element("ByteSize").Value);
            }
            else
            {
                string baseTypeName = theNode.Element("Type") != null ? theNode.Element("Type").Value : "int";

                BaseType = manager.GetElementType(baseTypeName);
                BaseType = DictionaryManager.DereferenceTypeDef(BaseType);

                FixedSizeBytes = BaseType.FixedSizeBytes.Value;
            }

            // Common properties are parsed by the base class.
            // Remaining properties are the Name/Value Literal elements
            List<LiteralDefinition> theLiterals = new List<LiteralDefinition>();
            foreach (var literalNode in theNode.Elements("Literal"))
            {
                theLiterals.Add(new LiteralDefinition(literalNode));
            }

            m_Literals = new ReadOnlyCollection<LiteralDefinition>(theLiterals);

            // Check the name. If it's null then make one up
            if (theNode.Element("Name") == null)
            {
                Name = String.Format("Enum_{0}", Ref);
            }
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("enum {0}", Name);
        }

        /// <summary>
        /// Creates a new Value object, deriving the value by decoding the specified bytes.
        /// Each derived class creates its corresponding value type
        /// </summary>
        /// <param name="theBytes">The collection of bytes containing the value to be
        /// decoded</param>
        /// <param name="parent"></param>
        /// <returns>
        /// The decoded value object
        /// </returns>
        public override Value Decode(ByteStore theBytes, Value parent)
        {
            return new EnumValue(this, theBytes, parent);
        }

        /// <summary>
        /// Create a new instance of the type populated with default values.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>
        /// New instance of the type populated with default values
        /// </returns>
        public override Value Instantiate(Value parent)
        {
            return new EnumValue(this, parent);
        }

        /// <summary>
        /// Tries to get the literal string corresponding to the integer value
        /// </summary>
        /// <param name="integerValue">The integer value.</param>
        /// <param name="literal">The literal.</param>
        /// <returns>True if found, else false</returns>
        public bool TryGetLiteral(int integerValue, out string literal)
        {
            bool found = false;
            literal = string.Empty;
            foreach (var item in m_Literals)
            {
                if (item.Value == integerValue)
                {
                    literal = item.Name;
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Tries to get the literal string corresponding to the integer value
        /// </summary>
        /// <param name="literal">The literal.</param>
        /// <param name="integerValue">The integer value.</param>
        /// <returns>
        /// True if found, else false
        /// </returns>
        public bool TryGetInteger(string literal, out int integerValue)
        {
            bool found = false;
            integerValue = 0;
            foreach (var item in m_Literals)
            {
                if (item.Name == literal)
                {
                    integerValue = item.Value;
                    found = true;
                    break;
                }
            }

            return found;
        }

        /// <summary>
        /// Returns the string equivalent of the integer value. Throws if the integer value is not known.
        /// </summary>
        /// <param name="theIntegerValue">The integer value.</param>
        /// <returns>String equivalent of the integer value</returns>
        public string IntegerValueToString(int theIntegerValue)
        {
            string literal;
            if (TryGetLiteral(theIntegerValue, out literal))
            {
                return literal;
            }

            return string.Format("[{0}]", theIntegerValue);
        }

        /// <summary>
        /// Returns the integer equivalent of the string value. Throws if the string value is not known.
        /// </summary>
        /// <param name="theStringValue">The string value.</param>
        /// <returns>The integer equivalent of the string value</returns>
        public int StringValueToInteger(string theStringValue)
        {
            int intValue;
            if (TryGetInteger(theStringValue, out intValue))
            {
                return intValue;
            }

            throw new DataDictionaryException("Enum {0}: Illegal string value {1}", Name, theStringValue);
        }

        /// <summary>
        /// Validates the integer value is represented in the enumeration
        /// </summary>
        /// <param name="theIntegerValue">The integer value.</param>
        public void ValidateIntegerValue(int theIntegerValue)
        {
            IntegerValueToString(theIntegerValue);
        }

        /// <summary>
        /// Validates the string value is represented in the enumeration
        /// </summary>
        /// <param name="theStringValue">The string value.</param>
        public void ValidateStringValue(string theStringValue)
        {
            StringValueToInteger(theStringValue);
        }

        /// <summary>
        /// Collection of the literal values defined for this enum
        /// </summary>
        private ReadOnlyCollection<LiteralDefinition> m_Literals;
    }
}