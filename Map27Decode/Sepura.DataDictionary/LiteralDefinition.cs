#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// LiteralDefinition.cs
//  Implementation of the Class LiteralDefinition
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
    /// Class representing the definition of a Literal element - name/value pair
    /// </summary>
    public class LiteralDefinition 
    {
        /// <summary>
        /// Gets the text string associated with the literal
        /// </summary>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the integer value associated with the literal
        /// </summary>
        public int Value
        {
            get { return m_Value; }
        }

        /// <summary>
        /// Initializes a new instance of the LiteralDefinition class
        /// <font color="#008000">Constructor populates this object with information
        /// extracted from the XML</font>
        /// </summary>
        /// <param name="theNode"></param>
        public LiteralDefinition(XElement theNode)
        {
            Name = theNode.Element("Name").Value;

            if (!int.TryParse(theNode.Element("Value").Value, out m_Value))
            {
                throw new DataDictionaryException("{0} : cannot find the type for the pointer", theNode.ToString());                    
            }
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("literal {0} {1}", Name, Value);
        }

        /// <summary>
        /// Integer value associated with the literal
        /// </summary>
        private int m_Value;
    }
}