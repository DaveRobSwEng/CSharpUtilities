#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// DataDictionaryTraceSource.cs
//  Implementation of the Class DataDictionaryTraceSource
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
    using System.Diagnostics;

    /// <summary>
    /// Wraps the static TraceSource object for this module
    /// </summary>
    public sealed class DataDictionaryTraceSource : TraceSource
    {
        /// <summary>
        /// Prevents a default instance of the DataDictionaryTraceSource class from being created 
        /// Initialises a new instance of the DataDictionaryTraceSource class
        /// Connects to the trace source defined in the app.config file
        /// </summary>
        private DataDictionaryTraceSource()
            : base("DataDictionaryTraceSource")
        {
        }

        /// <summary>
        /// Gets a reference to the single instance of the class
        /// </summary>
        public static TraceSource TraceSource
        {
            get { return m_Instance; }
        }

        /// <summary>
        /// Single instance of the class
        /// </summary>
        private static readonly TraceSource m_Instance = new DataDictionaryTraceSource();
    }
}