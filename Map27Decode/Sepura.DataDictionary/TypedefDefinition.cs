#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// TypeDefDefinition.cs
//  Implementation of the Class TypeDefDefinition
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
    using System.Xml.Linq;

    /// <summary>
    /// Object representing a typedef definition
    /// </summary>
    public class TypedefDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the type for which this typedef is an alias
        /// </summary>
        public TypeDefinition AliasedType
        {
            get
            {
                return m_AliasedType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the TypedefDefinition class
        /// Populates this object with information extracted from the XML
        /// </summary>
        /// <param name="theNode"></param>
        public TypedefDefinition(XElement theNode)
            : base(theNode, TypeId.TypeDefType)
        {
        }

        /// <summary>
        /// Finishes construction of the object by populating properties of the object from
        /// the XML node
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theManager"></param>
        public override void FinishConstruction(XElement theNode, DictionaryManager theManager)
        {
            // Type of the attribute is identified by a number that is the Ref field of a type somewhere
            // or the name of the aliased type
            m_AliasedType = theManager.GetElementType(theNode.Element("Type").Value);
            FixedSizeBytes = m_AliasedType.FixedSizeBytes;
        }

        /// <summary>
        /// Creates a new Value object, deriving the value by decoding the specified bytes.
        /// Each derived class creates its corresponding value type
        /// </summary>
        /// <param name="theBytes">The collection of bytes containing the value to be decoded</param>
        /// <param name="parent">The parent.</param>
        /// <returns>
        /// The decoded value object
        /// </returns>
        public override Value Decode(ByteStore theBytes, Value parent)
        {
            TypeDefinition baseType = DictionaryManager.DereferenceTypeDef(this);
            Value theValue = baseType.Decode(theBytes, parent);
            theValue.OverrideInitialType(this);

            return theValue;
        }

        /// <summary>
        /// Create a new instance of the type populated with default values.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns>
        /// New instance of the type populated with default values
        /// </returns>
        public override Value Instantiate(Value parent)
        {
            TypeDefinition baseType = DictionaryManager.DereferenceTypeDef(this);
            Value theValue = baseType.Instantiate(parent);
            if (theValue != null)
            {
                theValue.OverrideInitialType(this);
            }

            return theValue;
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("typedef {0} {1}", Name, AliasedType.Name);
        }

        /// <summary>
        /// Type for which this typedef is an alias
        /// </summary>
        private TypeDefinition m_AliasedType;
    }
}