#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// StructureDefinition.cs
//  Implementation of the Class StructureDefinition
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
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Class containing the definition of a structure within the data dictionary
    /// </summary>
    public class StructureDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the collection (ordered) of the structure members
        /// </summary>
        public AttributeDefinitionCollection AttributeDefinitions
        {
            get
            {
                return m_AttributeDefinitions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a TLV structure.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is TLV structure; otherwise, <c>false</c>.
        /// </value>
        public bool IsTlvStructure
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the StructureDefinition class
        /// Populates properties of the object from the XML node
        /// </summary>
        /// <param name="theNode"></param>
        public StructureDefinition(XElement theNode)
            : base(theNode, TypeId.StructType)
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
            try
            {
                if (theNode.Element("ByteSize") != null)
                {
                    SetByteSize(theNode.Element("ByteSize").Value);
                }

                IsTlvStructure = false;

                // Parse each of the attribute nodes in turn - this may well get recursive
                m_AttributeDefinitions = new AttributeDefinitionCollection(Name);
                foreach (var attributeNode in theNode.Elements("Attribute"))
                {
                    AttributeDefinition theAttribute = new AttributeDefinition(attributeNode, theManager, this);
                    m_AttributeDefinitions.Add(theAttribute);
                    if (theAttribute.HasDiscriminator && theAttribute.HasLengthIndicator)
                    {
                        IsTlvStructure = true;
                        m_TlvControlAttribute = theAttribute;
                    }
                }

                // Check the name. If it's null then make one up
                if (theNode.Element("Name") == null)
                {
                    Name = String.Format("XXStruct_{0}", Ref);
                }
            }
            catch (DataDictionaryException ex)
            {
                throw new DataDictionaryException("Error constructing definition for Struct. XML:\n{0}\nException: {1}", theNode, ex.Message);
            }
            catch (XmlException ex)
            {
                throw new DataDictionaryException("Error constructing definition for Struct. XML:\n{0}\nException: {1}", theNode, ex.Message);
            }
        }

        /// <summary>
        /// Gets the name of the attribute that specifies the TLV type.
        /// </summary>
        /// <returns>Name of the attribute that specifies the TLV type</returns>
        public string TlvTypeAttributeName()
        {
            if (m_TlvControlAttribute == null || !m_TlvControlAttribute.HasDiscriminator)
            {
                throw new DataDictionaryException("Struct {0} is not a TLV structure - no attribute specifies the TLV type", Name);
            }

            return m_TlvControlAttribute.Discriminator;
        }

        /// <summary>
        /// Gets the name of the attribute that specifies the TLV length.
        /// </summary>
        /// <returns>Name of the attribute that specifies the TLV length</returns>
        public string TlvLengthAttributeName()
        {
            if (m_TlvControlAttribute == null || !m_TlvControlAttribute.HasLengthIndicator)
            {
                throw new DataDictionaryException("Struct {0} is not a TLV structure - no attribute specifies the TLV length", Name);
            }

            return m_TlvControlAttribute.LengthIndicator;
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("struct {0}", Name);
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
            return new StructureValue(theBytes, this, parent);
        }

        /// <summary>
        /// Create a new instance of the type defined by the structure definition populated
        /// with default values.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>
        /// New instance of the type populated with default values
        /// </returns>
        public override Value Instantiate(Value parent)
        {
            return new StructureValue(this, parent);
        }

        /// <summary>
        ///  Collection (ordered) of the structure members
        /// </summary>
        private AttributeDefinitionCollection m_AttributeDefinitions;

        /// <summary>
        /// The TLV control attribute
        /// </summary>
        private AttributeDefinition m_TlvControlAttribute;
    }
}
