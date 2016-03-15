#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// SubroutineTypeDefinition.cs
//  Implementation of the Class SubroutineTypeDefinition
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
    /// Class representing a subroutine definition in the data dictionary
    /// </summary>
    public class SubroutineTypeDefinition : TypeDefinition
    {
        /// <summary>
        /// Initializes a new instance of the SubroutineTypeDefinition class
        /// </summary>
        /// <param name="theNode"></param>
        public SubroutineTypeDefinition(XElement theNode)
            : base(theNode, TypeId.SubroutineType)
        {
            // Make up a default name for the type
            Name = String.Format("Subroutine_{0}", Ref);
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("Subroutine {0}", Name);
        }
    }
}