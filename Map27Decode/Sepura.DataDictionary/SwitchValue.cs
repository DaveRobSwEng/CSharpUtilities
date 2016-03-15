#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// SwitchValue.cs
//  Implementation of the Class SwitchValue
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

    /// <summary>
    /// Value of an instantiated SwitchDefinition
    /// </summary>
    public class SwitchValue : Value
    {
        /// <summary>
        /// Gets the discriminator value.
        /// </summary>
        public int DiscriminatorValue
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the wrapped value.
        /// </summary>
        public Value WrappedValue
        {
            get
            {
                return m_WrappedValue;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchValue"/> class.
        /// </summary>
        /// <param name="theSwitchDefinition">The switch definition.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="wrappedValue">The wrapped value.</param>
        /// <param name="theDiscriminatorValue">The discriminator value.</param>
        public SwitchValue(SwitchDefinition theSwitchDefinition, Value parent, Value wrappedValue, int theDiscriminatorValue)
            : base(theSwitchDefinition, parent)
        {
            DiscriminatorValue = theDiscriminatorValue;
            m_WrappedValue = wrappedValue;
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes">The bytes.</param>
        public override void Encode(ByteStore theBytes)
        {
            m_WrappedValue.Encode(theBytes);
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            return m_WrappedValue.GetSizeBytes();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format ("Case {0}: {1}", DiscriminatorValue, m_WrappedValue);
        }

        /// <summary>
        /// The wrapped value
        /// </summary>
        private readonly Value m_WrappedValue;
    }
}