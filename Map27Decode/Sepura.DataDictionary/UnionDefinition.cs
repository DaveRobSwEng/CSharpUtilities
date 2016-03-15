#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// UnionDefinition.cs
//  Implementation of the Class UnionDefinition
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
    using System.Xml.Linq;

    /// <summary>
    /// Class representing the definition of a union
    /// </summary>
    public class UnionDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the collection (ordered) of the structure members
        /// </summary>
        public ReadOnlyCollection<AttributeDefinition> AttributeDefinitions
        {
            get
            {
                return m_AttributeDefinitions;
            }
        }

        /// <summary>
        /// Initializes a new instance of the UnionDefinition class
        /// Populates this object with information extracted from the XML
        /// </summary>
        /// <param name="theNode"></param>
        public UnionDefinition(XElement theNode)
            : base(theNode, TypeId.UnionType)
        {
            SetByteSize(theNode.Element("ByteSize").Value);

            // Check the name. If it's null then make one up
            if (theNode.Element("Name") == null)
            {
                Name = String.Format("Union_{0}", Ref);
            }
        }

        /// <summary>
        /// Finishes construction of the object by populating properties of the object from
        /// the XML node
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theManager"></param>
        public override void FinishConstruction(XElement theNode, DictionaryManager theManager)
        {
            // Parse each of the attribute nodes in turn - this may well get recursive
            List<AttributeDefinition> attributes = new List<AttributeDefinition>();
            foreach (var attributeNode in theNode.Elements("Attribute"))
            {
                attributes.Add(new AttributeDefinition(attributeNode, theManager, this));
            }

            m_AttributeDefinitions = new ReadOnlyCollection<AttributeDefinition>(attributes);
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("union {0}", Name);
        }

        /// <summary>
        /// Collection (ordered) of the structure members
        /// </summary>
        private ReadOnlyCollection<AttributeDefinition> m_AttributeDefinitions;
    }
}