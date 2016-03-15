#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// BaseTypeValue.cs
//  Implementation of the Class BaseTypeValue
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// Holds the value of an item defined by BaseTypeDefinition
    /// </summary>
    public class BaseTypeValue : Value, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the signed value.
        /// </summary>
        /// <value>
        /// The signed value.
        /// </value>
        public long SignedValue
        {
            get
            {
                return m_SignedValue;
            }

            set
            {
                if (value != m_SignedValue)
                {
                    m_SignedValue = value;
                    RaisePropertyChanged("SignedValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the unsigned value.
        /// </summary>
        /// <value>
        /// The unsigned value.
        /// </value>
        public ulong UnsignedValue
        {
            get
            {
                return m_UnsignedValue;
            }

            set
            {
                if (value != m_UnsignedValue)
                {
                    m_UnsignedValue = value;
                    RaisePropertyChanged("UnsignedValue");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is signed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is signed; otherwise, <c>false</c>.
        /// </value>
        public bool IsSigned
        {
            get
            {
                return m_BaseTypeDefinition.IsSigned;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeValue"/> class.
        /// </summary>
        /// <param name="theValue">The value.</param>
        /// <param name="theBaseTypeDefinition">The base type definition.</param>
        /// <param name="parent">The parent.</param>
        public BaseTypeValue(long theValue, BaseTypeDefinition theBaseTypeDefinition, Value parent)
            : base(theBaseTypeDefinition, parent)
        {
            SignedValue = theValue;
            m_BaseTypeDefinition = theBaseTypeDefinition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeValue"/> class
        /// with default values
        /// </summary>
        /// <param name="theBaseTypeDefinition">The base type definition.</param>
        /// <param name="parent">The parent.</param>
        public BaseTypeValue(BaseTypeDefinition theBaseTypeDefinition, Value parent)
            : base(theBaseTypeDefinition, parent)
        {
            m_BaseTypeDefinition = theBaseTypeDefinition;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseTypeValue"/> class with
        /// values extracted from the byte store
        /// </summary>
        /// <param name="baseTypeDefinition">The base type definition.</param>
        /// <param name="theBytes">The bytes.</param>
        /// <param name="parent">The parent.</param>
        public BaseTypeValue(BaseTypeDefinition baseTypeDefinition, ByteStore theBytes, Value parent)
            : this(baseTypeDefinition, parent)
        {
            ulong tempValue = 0;
            switch (m_BaseTypeDefinition.FixedSizeBytes.Value)
            {
                case 0:
                    // Special case for end-markers for lists of elements
                    break;
                case 1:
                    tempValue = theBytes.GetByte();
                    break;
                case 2:
                    tempValue = theBytes.GetUint16();
                    break;
                case 4:
                    tempValue = theBytes.GetUint32();
                    break;
                case 8:
                    tempValue = theBytes.GetUint64();
                    break;
                default:
                    throw new DataDictionaryException(String.Format("{0} Illegal ByteSize {1}",
                        m_BaseTypeDefinition.Name,
                        m_BaseTypeDefinition.FixedSizeBytes.Value));
            }

            if (m_BaseTypeDefinition.IsSigned)
            {
                SignedValue = (long)tempValue;
            }
            else
            {
                UnsignedValue = tempValue;
            }
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes"></param>
        public override void Encode(ByteStore theBytes)
        {
            ulong tempValue = 0;
            tempValue = m_BaseTypeDefinition.IsSigned ? (ulong)SignedValue : UnsignedValue;

            switch (GetSizeBytes())
            {
                case 0:
                    // 0-size end marker
                    break;
                case 1:
                    theBytes.PutByte((byte)tempValue);
                    break;
                case 2:
                    theBytes.PutUint16((ushort)tempValue);
                    break;
                case 4:
                    theBytes.PutUint32((uint)tempValue);
                    break;
                case 8:
                    theBytes.PutUint64(tempValue);
                    break;
                default:
                    throw new DataDictionaryException(String.Format("{0} Illegal ByteSize {1}",
                        m_BaseTypeDefinition.Name,
                        GetSizeBytes()));
            }
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            return m_BaseTypeDefinition.FixedSizeBytes.Value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (m_BaseTypeDefinition.IsSigned)
            {
                return string.Format("{0} (0x{0:x})", SignedValue);
            }
            else
            {
                return string.Format("{0} (0x{0:x})", UnsignedValue);
            }
        }

        /// <summary>
        /// The base type definition
        /// </summary>
        private BaseTypeDefinition m_BaseTypeDefinition;

        /// <summary>
        /// The signed value
        /// </summary>
        private long m_SignedValue;

        /// <summary>
        /// The unsigned value
        /// </summary>
        private ulong m_UnsignedValue;

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs (propertyName));
            }
        }

        #endregion
    }
}