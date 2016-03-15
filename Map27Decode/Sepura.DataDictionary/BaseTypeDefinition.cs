#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// BaseTypeDefinition.cs
//  Implementation of the Class BaseTypeDefinition
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
    /// Class representing the definition of a base type, e.g. int
    /// </summary>
    public class BaseTypeDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="BaseTypeDefinition"/> is signed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if signed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigned
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the BaseTypeDefinition class
        /// </summary>
        /// <param name="theNode"></param>
        public BaseTypeDefinition(XElement theNode)
            : base(theNode, TypeId.BaseType)
        {
            SetByteSize(theNode.Element("ByteSize").Value);
            string name = theNode.Element("Name").Value;
            IsSigned = !name.Contains("unsigned");
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("base type {0}", Name);
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
            return new BaseTypeValue(this, theBytes, parent);
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
            return new BaseTypeValue(this, parent);
        }
    }
}