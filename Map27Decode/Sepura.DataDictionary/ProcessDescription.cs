#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// ProcessDescription.cs
//  Implementation of the Class ProcessDescription
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
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Class responsible for representing a process description extracted from 
    /// a data dictionary
    /// </summary>
    public class ProcessDescription
    {
        /// <summary>
        /// Gets the name of the process
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the integer value of the process ID
        /// </summary>
        public uint Value { get; private set; }

        /// <summary>
        /// Gets the long name of the process
        /// </summary>
        public string LongName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ProcessDescription class
        /// </summary>
        /// <param name="theName"></param>
        /// <param name="theValue"></param>
        /// <param name="theLongName"></param>
        public ProcessDescription(string theName, uint theValue, string theLongName)
        {
            Name = theName;
            Value = theValue;
            LongName = theLongName;
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("Process {0}: {1}", Name, LongName);
        }
    }   
}
