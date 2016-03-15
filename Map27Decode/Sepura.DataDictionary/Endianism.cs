#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// Endianism.cs
//  Implementation of the enum Endianism
//
// Copyright (c) 2011 Sepura Plc
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
    /// Set of values indicating the endianism of the system
    /// </summary>
    public enum Endianism
    {
        /// <summary>
        /// The system is little endian
        /// </summary>
        LittleEndian,

        /// <summary>
        /// The system is big endian
        /// </summary>
        BigEndian,
    }
}
