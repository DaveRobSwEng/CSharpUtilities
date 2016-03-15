#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// SignalDescription.cs
//  Implementation of the Class SignalDescription
//
//  Original author: RobinsonD
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;

    /// <summary>
    /// Class responsible for representing a signal description extracted from a data dictionary
    /// </summary>
    public class SignalDescription
    {
        /// <summary>
        /// Gets the name of the signal
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the integer value of the signal ID
        /// </summary>
        public uint Value { get; private set; }

        /// <summary>
        /// Gets the name of the type containing the data content of the signal
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the SignalDescription class
        /// </summary>
        /// <param name="theName"></param>
        /// <param name="theValue"></param>
        /// <param name="theType"></param>
        public SignalDescription(string theName, uint theValue, string theType)
        {
            Name = theName;
            Value = theValue;
            TypeName = theType;
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("signal {0} (0x{1:2x}): {1}", Name, Value, TypeName);
        }
    }
}
