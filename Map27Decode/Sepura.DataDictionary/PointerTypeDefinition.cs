#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// PointerTypeDefinition.cs
//  Implementation of the Class PointerTypeDefinition
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
    /// Holds the definition of a pointer type
    /// </summary>
    public class PointerTypeDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the reference to the type to which this is a pointer
        /// If the initial type is a typedef then this holds a reference to the aliased type
        /// </summary>
        public TypeDefinition Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the type to which this is a pointer
        /// If the initial type is a typedef then this holds the name of the typedef, not the aliased type
        /// </summary>
        public string TypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the PointerTypeDefinition class
        /// Populates this object with information extracted from the XML
        /// </summary>
        /// <param name="theNode"></param>
        public PointerTypeDefinition(XElement theNode)
            : base(theNode, TypeId.PointerType)
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
            // Find out what this points to and use this to form the name of the type
            // Type of the attribute is identified by a number that is the Ref field of a type somewhere
            // or a string that is the name of a type somewhere
            TypeDefinition theType = theManager.GetElementType(theNode.Element("Type").Value);
            if (theType == null)
            {
                throw new DataDictionaryException("{0} : cannot find the type for the pointer", theNode.ToString());    
            }

            TypeName = theType.Name;

            // Follow aliases of typedefs so that the underlying type is stored
            while (theType is TypedefDefinition)
            {
                theType = ((TypedefDefinition)theType).AliasedType;
            }

            Type = theType;

            // Finally set the name for the type
            Name = String.Format("*{0}", TypeName);
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("pointer type {0}", Name);
        }
    }
}