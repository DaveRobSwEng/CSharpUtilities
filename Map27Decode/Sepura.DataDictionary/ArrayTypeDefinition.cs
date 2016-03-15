#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// ArrayTypeDefinition.cs
//  Implementation of the Class ArrayTypeDefinition
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
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// Class holding a definition of an array type
    /// </summary>
    public class ArrayTypeDefinition : TypeDefinition
    {
        /// <summary>
        /// Gets the number of elements in the array
        /// UpperBound [0] is the left-most dimension (slowest varying) so int[a][b][c] would have
        /// UpperBound [0] = a, UpperBound [1] = b, UpperBound [2] = c, 
        /// Arrays may also be dynamically allocated so UpperBound [] is zero-length
        /// </summary>
        public int[] UpperBound
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the variable that defines the upper bound
        /// May be empty if unused
        /// </summary>
        public string UpperBoundVariable
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the rank of the array - how many dimensions
        /// </summary>
        public int Rank
        {
            get { return UpperBound.Length; }
        }

        /// <summary>
        /// Gets the reference to the type of each element that this array holds.If the initial type is a
        /// typedef then this holds a reference to the aliased type
        /// </summary>
        public TypeDefinition ElementType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the name of the type of each element that this array holds. If the initial type is a
        /// typedef then this holds the name of the typedef, not the aliased type
        /// </summary>
        public string ElementTypeName
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ArrayTypeDefinition class
        /// </summary>
        /// <param name="theNode"></param>
        public ArrayTypeDefinition(XElement theNode)
            : base(theNode, TypeId.ArrayType)
        {
        }

        /// <summary>
        /// Finishes construction of the object by populating properties of the object from
        /// the XML node
        /// </summary>
        /// <param name="theNode"></param>
        /// <param name="theManager"></param>
        public override void FinishConstruction(XElement theNode, DictionaryManager theManager)
        {
            try
            {
                // Find out what type this holds and use this to form the name of the type
                TypeDefinition theType = theManager.GetElementType(theNode.Element("Type").Value);

                // Set the type name before following any typedefs
                ElementTypeName = theType.Name;

                // Follow aliases of typedefs so that the underlying type is stored
                while (theType is TypedefDefinition)
                {
                    theType = ((TypedefDefinition)theType).AliasedType;
                }

                ElementType = theType;

                if (theNode.Element("UpperBound") != null && !string.IsNullOrEmpty(theNode.Element("UpperBound").Value))
                {
                    // Read the size of the array
                    int thisUpperBound = 0;
                    if (Sepura.Utilities.Formatting.TryGetInt(theNode.Element("UpperBound").Value, out thisUpperBound))
                    {
                        // Handle nested (multi-dimensional) arrays
                        if (theType.GetType() == typeof(ArrayTypeDefinition))
                        {
                            ArrayTypeDefinition nestedArray = theType as ArrayTypeDefinition;

                            // Propagate the name of the underlying type (determined before following typedefs)
                            ElementTypeName = nestedArray.ElementTypeName;
                            ElementType = nestedArray.ElementType;

                            UpperBound = new int[nestedArray.UpperBound.Length + 1];
                            for (int i = 0; i < nestedArray.UpperBound.Length; i++)
                            {
                                UpperBound[i + 1] = nestedArray.UpperBound[i];
                            }

                            UpperBound[0] = thisUpperBound;
                        }
                        else
                        {
                            UpperBound = new int[1];
                            UpperBound[0] = thisUpperBound;
                        }

                        // Finally set the name for the type
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0} ", ElementTypeName);
                        for (int i = 0; i < UpperBound.Length; i++)
                        {
                            sb.AppendFormat("[{0}]", UpperBound[i]);
                        }

                        Name = sb.ToString();
                    }
                    else
                    {
                        UpperBoundVariable = theNode.Element("UpperBound").Value;

                        // Length of the array is defined at run-time
                        UpperBound = new int[0];
                        Name = string.Format("{0} []", ElementTypeName);
                    }
                }
                else
                {
                    // Length of the array is undefined - this gets treated as a list whose length is determined at run-time
                    UpperBound = new int[0];
                    Name = string.Format("{0} []", ElementTypeName);
                }
            }
            catch (DataDictionaryException ex)
            {
                throw new DataDictionaryException("Error constructing definition for Array. XML:\n{0}\nException: {1}", theNode, ex.Message);
            }
            catch (XmlException ex)
            {
                throw new DataDictionaryException("Error constructing definition for Array. XML:\n{0}\nException: {1}", theNode, ex.Message);
            }
        }

        /// <summary>
        /// Return a string describing the object
        /// </summary>
        /// <returns>String describing the object</returns>
        public override string ToString()
        {
            return String.Format("Array type {0}", Name);
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
            return new ArrayTypeValue(this, theBytes, parent);
        }

        /// <summary>
        /// Create a new instance of the type populated with default values.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns>
        /// New instance of the type populated with default values
        /// </returns>
        public override Value Instantiate(Value parent)
        {
            return new ArrayTypeValue(this, parent);
        }
    }
}