#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// EnumValue.cs
//  Implementation of the Class EnumValue
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System.ComponentModel;

    /// <summary>
    /// Holds the value of an item defined by EnumDefinition
    /// </summary>
    public class EnumValue : Value, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the integer value.
        /// </summary>
        /// <value>
        /// The integer value.
        /// </value>
        public int IntegerValue
        {
            get
            {
                return m_IntegerValue;
            }

            set
            {
                if (value != m_IntegerValue)
                {
                    m_EnumDefinition.ValidateIntegerValue(value);
                    m_IntegerValue = value;
                    m_StringValue = m_EnumDefinition.IntegerValueToString(m_IntegerValue);
                    RaisePropertyChanged("IntegerValue");
                    RaisePropertyChanged("StringValue");
                }
            }
        }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        /// <value>
        /// The string value.
        /// </value>
        public string StringValue
        {
            get 
            { 
                return m_StringValue; 
            }

            set
            {
                if (value != m_StringValue)
                {
                    m_EnumDefinition.ValidateStringValue(value);
                    m_StringValue = value;
                    m_IntegerValue = m_EnumDefinition.StringValueToInteger(m_StringValue);
                    RaisePropertyChanged("IntegerValue");
                    RaisePropertyChanged("StringValue");
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValue"/> class.
        /// </summary>
        /// <param name="theEnumDefinition">The enum definition.</param>
        /// <param name="parent">The parent.</param>
        public EnumValue(EnumDefinition theEnumDefinition, Value parent)
            : base(theEnumDefinition, parent)
        {
            m_EnumDefinition = theEnumDefinition;
            IntegerValue = m_EnumDefinition.Literals[0].Value;
            StringValue = m_EnumDefinition.Literals[0].Name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumValue"/> class by extracting the
        /// encoded value from the byte store
        /// </summary>
        /// <param name="theEnumDefinition">The enum definition.</param>
        /// <param name="theBytes">The bytes.</param>
        /// <param name="parent">The parent.</param>
        public EnumValue(EnumDefinition theEnumDefinition, ByteStore theBytes, Value parent)
            : this(theEnumDefinition, parent)
        {
            switch (theEnumDefinition.FixedSizeBytes)
            {
                case 1:
                    IntegerValue = (int)theBytes.GetByte();
                    break;
                case 2:
                    IntegerValue = (int)theBytes.GetUint16();
                    break;
                case 4:
                    IntegerValue = (int)theBytes.GetUint32();
                    break;
                default:
                    throw new DataDictionaryException(string.Format("Illegal byte size {0} for enum {1}", theEnumDefinition.FixedSizeBytes, m_EnumDefinition.Name));
            }

            StringValue = m_EnumDefinition.IntegerValueToString(IntegerValue);
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes"></param>
        public override void Encode(ByteStore theBytes)
        {
            switch (GetSizeBytes())
            {
                case 1:
                    theBytes.PutByte((byte) IntegerValue);
                    break;
                case 2:
                    theBytes.PutUint16((ushort) IntegerValue);
                    break;
                case 4:
                    theBytes.PutUint32((uint) IntegerValue);
                    break;
                default:
                    throw new DataDictionaryException(string.Format("Illegal byte size {0} for enum {1}", GetSizeBytes(), m_EnumDefinition.Name));
            }
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            return m_EnumDefinition.FixedSizeBytes.Value;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return m_StringValue;
        }

        /// <summary>
        /// The enum definition
        /// </summary>
        private EnumDefinition m_EnumDefinition;

        /// <summary>
        /// The string value
        /// </summary>
        private string m_StringValue;

        /// <summary>
        /// The integer value
        /// </summary>
        private int m_IntegerValue;

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
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}