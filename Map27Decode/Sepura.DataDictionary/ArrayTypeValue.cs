#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// ArrayTypeValue.cs
//  Implementation of the Class ArrayTypeValue
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
    using System.Text;

    /// <summary>
    /// Holds the value of an item defined by ArrayTypeDefinition
    /// </summary>
    public class ArrayTypeValue : Value
    {
        /// <summary>
        /// Gets an iterator over the set of values.
        /// </summary>
        public ArrayValueContainer Values
        {
            get
            {
                return m_ArrayValues;
            }
        }

        /// <summary>
        /// Gets the type of each element of the array
        /// </summary>
        /// <value>
        /// The type of each element.
        /// </value>
        public TypeDefinition ElementType
        {
            get
            {
                return m_ArrayTypeDefinition.ElementType;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeValue"/> class.
        /// Values are default values
        /// </summary>
        /// <param name="theArrayTypeDefinition">The array type definition.</param>
        /// <param name="parent">The parent.</param>
        public ArrayTypeValue(ArrayTypeDefinition theArrayTypeDefinition, Value parent)
            : base(theArrayTypeDefinition, parent)
        {
            m_ArrayTypeDefinition = theArrayTypeDefinition;

            m_ArrayValues = new ArrayValueContainer(m_ArrayTypeDefinition.UpperBound);

            if (m_ArrayTypeDefinition.Rank > 0)
            {
                // Fixed-size arrays - populate with instantiate elements
                // Variable-size arrays are populated on demand
                m_ArrayValues.Populate(() => m_ArrayTypeDefinition.ElementType.Instantiate(this));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayTypeValue"/> class.
        /// Values are decoded from the byte store
        /// </summary>
        /// <param name="theArrayTypeDefinition">The array type definition.</param>
        /// <param name="theBytes">The bytes.</param>
        /// <param name="parent">The parent.</param>
        public ArrayTypeValue(ArrayTypeDefinition theArrayTypeDefinition, ByteStore theBytes, Value parent)
            : base(theArrayTypeDefinition, parent)
        {
            m_ArrayTypeDefinition = theArrayTypeDefinition;

            m_ArrayValues = new ArrayValueContainer(m_ArrayTypeDefinition.UpperBound);

            if (m_ArrayTypeDefinition.Rank > 0)
            {
                // Fixed-size arrays - populate with decoded elements of the base type
                m_ArrayValues.Populate(() => m_ArrayTypeDefinition.ElementType.Decode(theBytes, this));
            }
            else if (!string.IsNullOrEmpty(m_ArrayTypeDefinition.UpperBoundVariable))
            {
                // Variable size array. Locate the discriminator if there is one
                // TODO
            }
            else
            {
                // Variable size array - number of elements limited by the number of bytes available or an end marker being discovered
                bool done = false;

                do
                {
                    Value theValue = m_ArrayTypeDefinition.ElementType.Decode(theBytes, this);
                    m_ArrayValues.Add(theValue);

                    if (theValue.FundamentalType.TypeId == TypeId.StructType)
                    {
                        done = IsEndMarker(theValue as StructureValue);
                    }
                }
                while (!done);
            }
        }

        /// <summary>
        /// Determines whether the specified value is the end marker
        /// </summary>
        /// <param name="theStructValue">The struct value.</param>
        /// <returns>
        ///   <c>true</c> if the specified value is the end marker; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsEndMarker(StructureValue theStructValue)
        {
            if (!theStructValue.IsTlvStructure)
            {
                return false;
            }

            // End marker for a sequence of TLV items signalled by length and type attributes being zero
            bool isEndMarker = true;

            AttributeValue typeAttribute = theStructValue.Attributes[theStructValue.GetTlvTypeAttributeName()];
            if (typeAttribute.Value.FundamentalType.TypeId == TypeId.EnumType)
            {
                EnumValue enumValue = typeAttribute.Value as EnumValue;
                if (enumValue.IntegerValue != 0)
                {
                    isEndMarker = false;
                }
            }

            AttributeValue lengthAttribute = theStructValue.Attributes[theStructValue.GetTlvLengthAttributeName()];
            if (lengthAttribute.Value.FundamentalType.TypeId == TypeId.BaseType)
            {
                BaseTypeValue baseValue = lengthAttribute.Value as BaseTypeValue;
                int length = baseValue.IsSigned ? (int)baseValue.SignedValue : (int)baseValue.UnsignedValue;
                if (length != 0)
                {
                    isEndMarker = false;
                }
            }

            return isEndMarker;
        }

        /// <summary>
        /// Gets or sets the <see cref="Sepura.DataDictionary.Value"/> with the specified indices.
        /// The length of the indices array must match the rank of the type
        /// </summary>
        /// <param name="indices">Array containing an index for each dimension of the array</param>
        /// <returns>The value at the specified index</returns>
        public Value this[int[] indices]
        {
            get
            {
                return m_ArrayValues[indices];
            }

            set
            {
                m_ArrayValues[indices] = value;
            }
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes"></param>
        public override void Encode(ByteStore theBytes)
        {
            foreach (var arrayItem in m_ArrayValues)
            {
                arrayItem.Encode(theBytes);
            }
        }

        /// <summary>
        /// Increments the indices, rightmost is fastest varying
        /// </summary>
        /// <param name="indices">The indices.</param>
        /// <returns>True if the index set was successfully incremented, else false</returns>
        public bool IncrementIndices(int[] indices)
        {
            return m_ArrayValues.IncrementIndices(indices);
        }

        /// <summary>
        /// Iterates over the n-dimensional array of Values
        /// </summary>
        /// <param name="doItem">The action to do on each item.</param>
        public void Iterate(Action<Value, int[]> doItem)
        {
            m_ArrayValues.Iterate(doItem);
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            int size = 0;
            foreach (var arrayItem in m_ArrayValues)
            {
                size += arrayItem.GetSizeBytes();
            }

            return size;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("[");
            bool first = true;
            foreach (var item in m_ArrayTypeDefinition.UpperBound)
            {
                if (!first)
                {
                    sb.Append(",");
                }

                sb.Append(item);
            }

            sb.Append("]");

            return string.Format("{0} {1}", m_ArrayTypeDefinition.ElementTypeName, sb.ToString());
        }

        /// <summary>
        /// The container holding the array values
        /// </summary>
        private ArrayValueContainer m_ArrayValues;

        /// <summary>
        /// The type definition for this array value type
        /// </summary>
        private ArrayTypeDefinition m_ArrayTypeDefinition;
    }
}