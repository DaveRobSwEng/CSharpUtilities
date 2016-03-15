#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// TypeDefinition.cs
//  Implementation of the Class TypeDefinition
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
    using System.Globalization;
    using System.Xml.Linq;
    using Sepura.Utilities;

    /// <summary>
    /// Enumeration defining what sort of  DD type this is
    /// </summary>
    public enum TypeId
    {
        /// <summary>
        /// Structure containing set of attributes
        /// </summary>
        StructType,

        /// <summary>
        /// Base type: primitive type such as Uint16 etc.
        /// </summary>
        BaseType,

        /// <summary>
        /// Enumeration value
        /// </summary>
        EnumType,

        /// <summary>
        /// Typedef - alias for another type
        /// </summary>
        TypeDefType,

        /// <summary>
        /// Union value
        /// </summary>
        UnionType,

        /// <summary>
        /// Pointer to another type
        /// </summary>
        PointerType,

        /// <summary>
        /// Array value
        /// </summary>
        ArrayType,

        /// <summary>
        /// Subroutine value
        /// </summary>
        SubroutineType,

        /// <summary>
        /// Switch value
        /// </summary>
        SwitchType,

        /// <summary>
        /// Built-in type such as String
        /// </summary>
        BuiltInType,
    }

    /// <summary>
    /// Base class for any defined types
    /// </summary>
    public class TypeDefinition
    {
        /// <summary>
        /// Gets or sets the name of the type
        /// </summary>
        public string Name
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the (unique) ref ID by which this type is known
        /// This item is optional
        /// </summary>
        public int? Ref
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the size of the type (in bytes)
        /// This item is optional
        /// </summary>
        /// <value>
        /// The fixed size bytes.
        /// </value>
        public int? FixedSizeBytes
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets a value that identifies what sort of DD type this is
        /// </summary>
        public TypeId TypeId
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes a new instance of the TypeDefinition class. Construction is
        /// completed using FinishConstruction
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theTypeId"></param>
        public TypeDefinition(XElement theNode, TypeId theTypeId)
        {
            if (theNode.Element("Name") != null)
            {
                Name = theNode.Element("Name").Value;
            }

            if (theNode.Element("Ref") != null)
            {
                Ref = Formatting.ParseInt(theNode.Element("Ref").Value);
            }

            TypeId = theTypeId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeDefinition"/> class.
        /// </summary>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="theTypeId">The type identifier.</param>
        public TypeDefinition(string typeName, TypeId theTypeId)
        {
            Name = typeName;
            TypeId = theTypeId;
        }

        /// <summary>
        /// Finishes construction of the object by populating properties of the object from
        /// the XML node
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theManager"></param>
        public virtual void FinishConstruction(XElement theNode, DictionaryManager theManager)
        {
            // No further action required for this class
        }

        /// <summary>
        /// Factory pattern to create a new derived object from the supplied XML node
        /// </summary>
        /// <param name="theManager"></param>
        /// <param name="theNode"></param>
        /// <returns>New type definition</returns>
        public static TypeDefinition Create(DictionaryManager theManager, XElement theNode)
        {
            string typeName = theNode.Name.ToString();
            if (!m_FactoryMap.ContainsKey(typeName))
            {
                throw new DataDictionaryException("Unknown DD type {0}", typeName);
            }

            return m_FactoryMap[typeName](theNode, theManager);
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
        public virtual Value Decode(ByteStore theBytes, Value parent)
        {
            throw new DataDictionaryException("Cannot decode type {0}", Name);
        }

        /// <summary>
        /// Create a new instance of the type populated with default values.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <returns>
        /// New instance of the type populated with default values
        /// </returns>
        public virtual Value Instantiate(Value parent)
        {
            return null;
        }

        /// <summary>
        /// Sets the size in bytes of the type. This may be static, defined as a decimal or hexadecimal integer,
        /// or a string containing the name of a variable within the parent structure that defines the size
        /// </summary>
        /// <param name="byteSizeString">The byte size string.</param>
        protected void SetByteSize(string byteSizeString)
        {
            int byteSize = 0;
            if (Formatting.TryGetInt(byteSizeString, out byteSize))
            {
                FixedSizeBytes = byteSize;
            }
            else
            {
                m_ByteSizeVariableName = byteSizeString;
            }
        }

        /// <summary>
        /// The name of the variable specifying the byte size
        /// </summary>
        private string m_ByteSizeVariableName;

        /// <summary>
        /// Initializes static members of the TypeDefinition class
        /// Sets up the factory map
        /// </summary>
        static TypeDefinition()
        {
            m_FactoryMap["Struct"] = (XElement node, DictionaryManager manager) => new StructureDefinition(node);
            m_FactoryMap["BaseType"] = (XElement node, DictionaryManager manager) => new BaseTypeDefinition(node);
            m_FactoryMap["Enum"] = (XElement node, DictionaryManager manager) => new EnumDefinition(node, manager);
            m_FactoryMap["TypeDef"] = (XElement node, DictionaryManager manager) => new TypedefDefinition(node);
            m_FactoryMap["Union"] = (XElement node, DictionaryManager manager) => new UnionDefinition(node);
            m_FactoryMap["PointerType"] = (XElement node, DictionaryManager manager) => new PointerTypeDefinition(node);
            m_FactoryMap["ArrayType"] = (XElement node, DictionaryManager manager) => new ArrayTypeDefinition(node);
            m_FactoryMap["SubroutineType"] = (XElement node, DictionaryManager manager) => new SubroutineTypeDefinition(node);
            m_FactoryMap["Switch"] = (XElement node, DictionaryManager manager) => new SwitchDefinition(node);
        }

        /// <summary>
        /// Collection mapping the name of the element to an anonymous function that returns a new object
        /// constructed from the XML representation of the element
        /// Some, but not all, constructors require a pointer to the DictionaryManager because they recurse
        /// into nested structures and need to avoid re-creating types already parsed
        /// </summary>
        private static readonly Dictionary<string, Func<XElement, DictionaryManager, TypeDefinition>> m_FactoryMap =
            new Dictionary<string, Func<XElement, DictionaryManager, TypeDefinition>>();
    }
}