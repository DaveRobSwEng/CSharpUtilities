#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// StructureValue.cs
//  Implementation of the Class StructureValue
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    /// <summary>
    /// Holds the value of an item defined by StructureDefinition
    /// The value consists of a list of AttributeValue objects representing the structure attributes
    /// Each attribute value has a name (of the element in the structure) and a Value
    /// Attribute values may be retrieved by name, index or by enumerator over the collection
    /// </summary>
    public class StructureValue : Value
    {
        /// <summary>
        /// Gets the collection (ordered) of the structure members
        /// </summary>
        public AttributeValueCollection Attributes
        {
            get
            {
                return m_AttributeValues;
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
            get
            {
                return m_StructureDefinition.IsTlvStructure;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureValue"/> class.
        /// Values are populated with defaults
        /// </summary>
        /// <param name="theStructureDefinition">The structure definition.</param>
        /// <param name="parent">The parent.</param>
        public StructureValue(StructureDefinition theStructureDefinition, Value parent)
            : base(theStructureDefinition, parent)
        {
            m_StructureDefinition = theStructureDefinition;
            m_AttributeValues = new AttributeValueCollection(m_StructureDefinition.Name);

            foreach (var item in m_StructureDefinition.AttributeDefinitions)
            {
                AddAttributeValue(item, item.Type.Instantiate(this));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StructureValue"/> class.
        /// </summary>
        /// <param name="theBytes">The bytes.</param>
        /// <param name="theStructureDefinition">The structure definition.</param>
        /// <param name="parent">The parent.</param>
        public StructureValue(ByteStore theBytes, StructureDefinition theStructureDefinition, Value parent)
            : base(theStructureDefinition, parent)
        {
            m_StructureDefinition = theStructureDefinition;
            m_AttributeValues = new AttributeValueCollection(m_StructureDefinition.Name); 

            int startPosition = theBytes.ReadPosition;
            foreach (var attribute in m_StructureDefinition.AttributeDefinitions)
            {
                int localOffset = theBytes.ReadPosition - startPosition;

                // Adjust alignment within current structure 
                if (attribute.ByteOffset.HasValue)
                {
                    while (attribute.ByteOffset.Value > localOffset)
                    {
                        theBytes.GetByte();
                        localOffset++;
                    }
                }

                // Error handling - trap decode errors for structure attributes and replace the attribute
                // value with an error value
                try
                {
                    // Decode the value from the binary data
                    Value theValue = attribute.Type.Decode(theBytes, this);

                    // Values decoded from switch cases need to have their name tweaked
                    if (attribute.Type.TypeId == TypeId.SwitchType)
                    {
                        SwitchDefinition switchDefinition = attribute.Type as SwitchDefinition;
                        SwitchCaseDefinition theCase = switchDefinition.GetSwitchCaseDefinition(this);
                        AddAttributeValue(attribute, theCase.Name, theValue);
                    }
                    else
                    {
                        AddAttributeValue(attribute, theValue);
                    }
                }
                catch (DataDictionaryException ex)
                {
                    AddAttributeValue(attribute, new ErrorValue (string.Format ("{0} Error: {1}", InitialType.Name, ex.Message), this));
                    throw new PartialDecodeException(GetTopParent(), ex.Message);
                }
            }
        }

        /// <summary>
        /// Adds a new attribute value to the collection
        /// </summary>
        /// <param name="theDefinition">Definition of the attribute.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AddAttributeValue(AttributeDefinition theDefinition, Value attributeValue)
        {
            m_AttributeValues.Add(new AttributeValue(theDefinition, attributeValue, this));
        }

        /// <summary>
        /// Adds a new attribute value to the collection
        /// </summary>
        /// <param name="theDefinition">The definition.</param>
        /// <param name="theName">The name.</param>
        /// <param name="attributeValue">The attribute value.</param>
        public void AddAttributeValue(AttributeDefinition theDefinition, string theName, Value attributeValue)
        {
            m_AttributeValues.Add(new AttributeValue(theDefinition, theName, attributeValue, this));
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes"></param>
        public override void Encode(ByteStore theBytes)
        {
            int startPosition = theBytes.WritePosition;
            int localOffset;
            foreach (var item in m_StructureDefinition.AttributeDefinitions)
            {
                // Apply the attribute's byte offset first then encode the attribute
                localOffset = theBytes.WritePosition - startPosition;
                if (item.ByteOffset.HasValue)
                {
                    while (item.ByteOffset.Value > localOffset)
                    {
                        theBytes.PutByte(0);
                        ++localOffset;
                    }                    
                }

                Value theAttributeValue = m_AttributeValues[item.Name].Value;
                theAttributeValue.Encode(theBytes);
            }

            // Add padding bytes to pad to the correct structure length
            localOffset = theBytes.WritePosition - startPosition;
            if (m_StructureDefinition.FixedSizeBytes.HasValue)
            {
                while (localOffset < m_StructureDefinition.FixedSizeBytes.Value)
                {
                    theBytes.PutByte(0);
                    ++localOffset;
                }
            }
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            int size = 0;
            foreach (var item in m_AttributeValues)
            {
                size += item.Value.GetSizeBytes();
            }

            return size;
        }

        /// <summary>
        /// Gets the name of the attribute that specifies the TLV type.
        /// </summary>
        /// <returns>Name of the attribute that specifies the TLV type</returns>
        public string GetTlvTypeAttributeName()
        {
            return m_StructureDefinition.TlvTypeAttributeName();
        }

        /// <summary>
        /// Gets the name of the attribute that specifies the TLV length.
        /// </summary>
        /// <returns>Name of the attribute that specifies the TLV length</returns>
        public string GetTlvLengthAttributeName()
        {
            return m_StructureDefinition.TlvLengthAttributeName();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return m_StructureDefinition.Name;
        }

        /// <summary>
        /// The collection of attribute values
        /// </summary>
        private AttributeValueCollection m_AttributeValues;

        /// <summary>
        /// The structure definition
        /// </summary>
        private StructureDefinition m_StructureDefinition;
    }
}