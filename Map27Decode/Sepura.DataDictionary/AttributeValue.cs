#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// AttributeValue.cs
//  Implementation of the Class AttributeValue
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
    /// Holds the value of an attribute in a StructureValue
    /// </summary>
    public class AttributeValue 
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public Value Value
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the type definition of the value
        /// </summary>
        public AttributeDefinition Definition
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parent struct.
        /// </summary>
        public StructureValue ParentStruct
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValue"/> class.
        /// </summary>
        /// <param name="theDefinition">The definition.</param>
        /// <param name="theValue">The value.</param>
        /// <param name="theParentStruct">The parent struct.</param>
        public AttributeValue(AttributeDefinition theDefinition, Value theValue, StructureValue theParentStruct)
        {
            Definition = theDefinition;
            Name = theDefinition.Name;
            Value = theValue;
            ParentStruct = theParentStruct;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValue"/> class.
        /// This constructor allows the caller to override the default name that would be taken from
        /// the AttributeDefinition
        /// </summary>
        /// <param name="theDefinition">The definition.</param>
        /// <param name="theName">The name.</param>
        /// <param name="theValue">The value.</param>
        /// <param name="theParentStruct">The parent struct.</param>
        public AttributeValue(AttributeDefinition theDefinition, string theName, Value theValue, StructureValue theParentStruct)
        {
            Definition = theDefinition;
            Name = theName;
            Value = theValue;
            ParentStruct = theParentStruct;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format ("{0}: {1}", Name, Value.ToString ());
        }
    }
}