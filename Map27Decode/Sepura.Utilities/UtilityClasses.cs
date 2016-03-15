#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// UtilityClasses.cs
//  Implementation of various minor utility classes
//
// Copyright (c) 2010 Sepura Plc
// All Rights reserved.
//
//  Original author: robinsond
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System;

    /// <summary>
    /// Trivial class used to hold a string when firing an event
    /// </summary>
    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the text string
        /// </summary>
        public string Text
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the StringEventArgs class
        /// </summary>
        /// <param name="theText"></param>
        public StringEventArgs(string theText)
        {
            Text = theText;
        }
    }
}
