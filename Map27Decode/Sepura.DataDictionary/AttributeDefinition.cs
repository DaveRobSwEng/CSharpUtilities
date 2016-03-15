#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// AttributeDefinition.cs
//  Implementation of the Class AttributeDefinition
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
    /// Class containing a definition of an attribute of a structure
    /// </summary>
    public class AttributeDefinition
    {
        /// <summary>
        /// Gets the name of the attribute
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the offset to the start of the attribute within the structure (in bytes)
        /// May be null in which case the attribute follows immediately after the preceding one
        /// </summary>
        public int? ByteOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the reference to the type holding the value for the attribute
        /// If the initial type is a typedef then this holds a reference to the aliased type
        /// </summary>
        public TypeDefinition Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the attribute for which this is the discriminator
        /// </summary>
        /// <value>
        /// Attribute for which this is the discriminator.
        /// </value>
        public AttributeDefinition DiscriminatorFor
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a Discriminator.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has Discriminator; otherwise, <c>false</c>.
        /// </value>
        public bool HasDiscriminator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the element used in the associated switch statement whose value is the switch variable
        /// </summary>
        public string Discriminator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a length indicator.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has length indicator; otherwise, <c>false</c>.
        /// </value>
        public bool HasLengthIndicator
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the length indicator - name of the sibling attribute that holds the length of this attribute
        /// This is 'ByteSize' in the XML
        /// </summary>
        public string LengthIndicator
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the AttributeDefinition class
        /// Constructor. Create a new object by reading contents from the XML node.
        /// </summary>
        /// <param name="attributeNode">The attribute node.</param>
        /// <param name="theManager">The manager.</param>
        /// <param name="parentType">The parent structure.</param>
        public AttributeDefinition(XElement attributeNode, DictionaryManager theManager, TypeDefinition parentType)
        {
            Name = attributeNode.Element("Name").Value;

            if (attributeNode.Element("Discriminator") == null)
            {
                Discriminator = string.Empty;
                HasDiscriminator = false;
            }
            else
            {
                Discriminator = attributeNode.Element("Discriminator").Value;
                HasDiscriminator = true;
            }

            if (attributeNode.Element("ByteOffset") != null)
            {
                int byteOffset;

                if (!int.TryParse(attributeNode.Element("ByteOffset").Value, out byteOffset))
                {
                    throw new DataDictionaryException("{0} : failed to parse ByteOffset as an integer", attributeNode.ToString());
                }

                ByteOffset = byteOffset;
            }

            HasLengthIndicator = false;
            if (attributeNode.Element("ByteSize") != null)
            {
                LengthIndicator = attributeNode.Element("ByteSize").Value;
                HasLengthIndicator = true;
            }

            // Type of the attribute may identified by a number that is the Ref field of a type somewhere
            // or by the name of the type 
            // or by the XML element nested within the attribute definition
            if (attributeNode.Element("Type") != null)
            {
                Type = theManager.GetElementType(attributeNode.Element("Type").Value);
            }
            else
            {
                // Nested type
                Type = theManager.GetNestedTypeDefinition(attributeNode);
            }

            // Follow aliases of typedefs so that the underlying type is stored
            TypeDefinition theType = Type;
            while (theType is TypedefDefinition)
            {
                theType = ((TypedefDefinition)theType).AliasedType;
            }

            // Finally, if we've just created a Switch type set the discriminator
            if (theType != null && theType.TypeId == TypeId.SwitchType)
            {
                SwitchDefinition theSwitchDefinition = Type as SwitchDefinition;

                if (string.IsNullOrEmpty(Discriminator))
                {
                    throw new DataDictionaryException("Attribute {0}: no discriminator supplied for Switch {1}", Name, Type.Name);
                }

                StructureDefinition structureDefinition = (StructureDefinition)parentType;
                theSwitchDefinition.Discriminator = structureDefinition.AttributeDefinitions[Discriminator];
                theSwitchDefinition.Discriminator.DiscriminatorFor = this;
            }
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("attribute {0}: {1} ({2})", Name, Type.Name, DictionaryManager.DereferenceTypeDef(Type).Name);
        }
    }
}