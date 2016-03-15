#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// SwitchDefinition.cs
//  Implementation of the Class SwitchDefinition
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
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Sepura.Utilities;

    /// <summary>
    /// Placeholder for a type determined at run-time depending on the value of the
    /// switched variable. Corresponds to the Switch element in the data dictionary
    /// spec. Name may be null if the switch is embedded in the attribute definition.
    /// </summary>
    public class SwitchDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the enum used to define the range of values for the cases
        /// </summary>
        public EnumDefinition Use
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the collection of switch cases.
        /// </summary>
        public SwitchCaseCollection Cases
        {
            get
            {
                return m_SwitchCaseCollection;
            }
        }

        /// <summary>
        /// Gets or sets the attribute used in the associated switch statement whose
        /// value is the switch variable
        /// </summary>
        public AttributeDefinition Discriminator
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwitchDefinition"/> class.
        /// </summary>
        /// <param name="theNode">The node.</param>
        public SwitchDefinition(XElement theNode)
            : base(theNode, TypeId.SwitchType)
        {
        }

        /// <summary>
        /// Finishes construction of the object by populating properties of the object from
        /// the XML node
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theManager">The manager.</param>
        public override void FinishConstruction(XElement theNode, DictionaryManager theManager)
        {
            XElement useNode = theNode.Element("Use");
            Use = theManager.GetElementType(useNode.Value) as EnumDefinition;
            if (Use == null)
            {
                throw new DataDictionaryException("SwitchDefinition: {0} is not an enum type", useNode.Value);
            }

            m_SwitchCaseCollection = new SwitchCaseCollection(Use);

            // Note that each case node may have more than one value associated with it
            // Also a case node may have a nested type definition rather than a reference to a type
            var caseDefinitions = from caseNode in theNode.Elements("Case")
                                  from valueNode in caseNode.Elements("Value")
                                  select new SwitchCaseDefinition(caseNode, valueNode.Value, theManager);

            m_SwitchCaseCollection.AddRange(caseDefinitions);
            Name = theNode.Element("Name") != null ? theNode.Element("Name").Value : string.Format("switch_{0}", theManager.GetUniqueId());
        }

        /// <summary>
        /// Creates a new Value object, deriving the value by decoding the specified bytes.
        /// Each derived class creates its corresponding value type
        /// </summary>
        /// <param name="theBytes">The collection of bytes containing the value to be
        /// decoded</param>
        /// <param name="parent"></param>
        /// <returns>
        /// The decoded value object
        /// </returns>
        public override Value Decode(ByteStore theBytes, Value parent)
        {
            // Find the discriminator for the switch and look up its value
            StructureValue theParentStruct = parent as StructureValue;
            if (theParentStruct == null)
            {
                throw new DataDictionaryException("SwitchDefinition {0}: Parent {1} is not a struct value",
                    Name, parent.FundamentalType.Name);
            }

            SwitchCaseDefinition caseDefinition = GetSwitchCaseDefinition(theParentStruct);

            if (caseDefinition == null)
            {
                throw new DataDictionaryException("SwitchDefinition {0}: case value not found", Name);
            }

            TypeDefinition typeToDecode = caseDefinition.Type;
            return typeToDecode.Decode(theBytes, parent);
        }

        /// <summary>
        /// Gets the switch case definition corresponding to the value of the discriminator in the parent structure
        /// or null if there's no match
        /// </summary>
        /// <param name="theParentStruct">The parent structure</param>
        /// <returns>The switch case definition corresponding to the value of the discriminator in the parent structure</returns>
        public SwitchCaseDefinition GetSwitchCaseDefinition(StructureValue theParentStruct)
        {
            SwitchCaseDefinition theCaseDefinition = null;
            int discriminatorValue = GetDiscriminatorValue(theParentStruct);
            bool valueFound = Cases.TryGetValue(discriminatorValue, out theCaseDefinition);

            if (!valueFound)
            {
                throw new DataDictionaryException("Parent {2} Case {0}: value {1} not found",
                    this.Name, discriminatorValue, theParentStruct.FundamentalType.Name);
            }

            return theCaseDefinition;
        }

        /// <summary>
        /// Gets the discriminator value.
        /// </summary>
        /// <param name="theParentStruct">The parent struct.</param>
        /// <returns>Discriminator value</returns>
        public int GetDiscriminatorValue(StructureValue theParentStruct)
        {
            Value discriminatorValue = theParentStruct.Attributes[Discriminator.Name].Value;
            int discriminatorValueAsInteger = 0;
            if (discriminatorValue.FundamentalType.TypeId == TypeId.EnumType)
            {
                EnumValue discriminatorValueAsEnum = discriminatorValue as EnumValue;
                discriminatorValueAsInteger = discriminatorValueAsEnum.IntegerValue;
            }
            else if (discriminatorValue.FundamentalType.TypeId == TypeId.BaseType)
            {
                BaseTypeValue discriminatorValueAsBaseType = discriminatorValue as BaseTypeValue;
                discriminatorValueAsInteger = discriminatorValueAsBaseType.IsSigned ?
                    (int)discriminatorValueAsBaseType.SignedValue : (int)discriminatorValueAsBaseType.UnsignedValue;
            }
            else
            {
                throw new DataDictionaryException("SwitchDefinition {0}: DiscriminatorValue {1} is neither enum nor base type",
                    Name, discriminatorValue.FundamentalType.Name);
            }

            return discriminatorValueAsInteger;
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("switch {0}: {1})", Name, Use.Name);
        }

        /// <summary>
        /// Create a new instance of the type selected by the discriminator value, populated with default values,
        /// or null if the discriminator value is not recognised
        /// </summary>
        /// <param name="parent">The parent structure value</param>
        /// <returns>
        /// New instance of the type selected by the discriminator value, populated with default values, or null
        /// </returns>
        public override Value Instantiate(Value parent)
        {
            StructureValue theParentStruct = parent as StructureValue;
            SwitchCaseDefinition theCase = GetSwitchCaseDefinition(theParentStruct);

            Value theValue = null;
            if (theCase != null)
            {
                theValue = theCase.Type.Instantiate(parent);
            }

            return new SwitchValue(this, parent, theValue, GetDiscriminatorValue(theParentStruct));
        }

        /// <summary>
        /// The switch case collection
        /// </summary>
        private SwitchCaseCollection m_SwitchCaseCollection;
    }
}