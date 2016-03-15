#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// SwitchCaseDefinition.cs
//  Implementation of the Class SwitchCaseDefinition
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
    using System.Xml.Linq;
    
    /// <summary>
    /// Definition of a case statement used within a switch
    /// </summary>
    public class SwitchCaseDefinition 
    {
        /// <summary>
        /// Gets the name of the attribute that this case statement introduces into the owning
        /// structure
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of the attribute that this case statement introduces into the owning
        /// structure
        /// </summary>
        public TypeDefinition Type
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the type of the attribute that this case statement introduces into the owning structure
        /// If the initial type is a typedef then this holds the name of the typedef, not the aliased type
        /// </summary>
        public string TypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the value of the discriminator associated with this case statement
        /// </summary>
        public string Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchCaseDefinition"/> class.
        /// </summary>
        /// <param name="theCaseNode">The XML element defining this switch case</param>
        /// <param name="caseValue">The case value.</param>
        /// <param name="theManager">The data dictionary manager.</param>
        public SwitchCaseDefinition(XElement theCaseNode, string caseValue, DictionaryManager theManager)
        {
            Value = caseValue;
            Name = (theCaseNode.Element("Name") != null) ? theCaseNode.Element("Name").Value : string.Empty;

            if (theCaseNode.Element("Type") != null)
            {
                TypeDefinition caseType = theManager.GetElementType(theCaseNode.Element("Type").Value);
                TypeName = caseType.Name;
                Type = DictionaryManager.DereferenceTypeDef (caseType);
            }
            else
            {
                Type = theManager.GetNestedTypeDefinition(theCaseNode);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format ("case {0}: {1} (Type: {2})", Name, Value, Type);
        }
    }
}