#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// Value.cs
//  Implementation of the Class Value
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
    /// Base class for all DD type values
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Gets the type definition associated with this value
        /// This is the fundamental type having followed all typedefs
        /// </summary>
        public TypeDefinition FundamentalType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the initial type before any typedefs followed
        /// </summary>
        public TypeDefinition InitialType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the parent. May be null if this is the root value
        /// </summary>
        public Value Parent
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Value"/> class.
        /// </summary>
        /// <param name="theTypeDefinition">The type definition.</param>
        /// <param name="parent">The parent.</param>
        public Value(TypeDefinition theTypeDefinition, Value parent)
        {
            FundamentalType = InitialType = theTypeDefinition;
            Parent = parent;
        }

        /// <summary>
        /// Overrides the initial type.
        /// </summary>
        /// <param name="theInitialType">The initial type.</param>
        public void OverrideInitialType (TypeDefinition theInitialType)
        {
            InitialType = theInitialType;
        }

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes">The bytes.</param>
        public virtual void Encode(ByteStore theBytes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Value for {0}", FundamentalType.Name);
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public virtual int GetSizeBytes ()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the Value at the top of the nested structure hierarchy
        /// </summary>
        /// <returns>Returns the Value at the top of the nested structure hierarchy</returns>
        public Value GetTopParent ()
        {
            Value topParent = this;
            while (topParent.Parent != null)
            {
                topParent = topParent.Parent;
            }

            return topParent;
        }
    }
}