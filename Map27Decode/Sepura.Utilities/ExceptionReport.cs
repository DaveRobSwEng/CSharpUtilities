#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// ExceptionReport.cs
//  Implementation of the Class ExceptionReport
//
//  Original author: RobinsonD
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities 
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Class containing information about an exception
    /// </summary>
    public class ExceptionReport 
    {
        /// <summary>
        /// Gets text describing the exception
        /// </summary>
        public string ExceptionText
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets text describing the stack trace
        /// </summary>
        public string ExceptionStack
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the ExceptionReport class.
        /// Extracts information from the exception object
        /// </summary>
        /// <param name="theException"></param>
        public ExceptionReport(Exception theException)
        {
            ExceptionText = theException.Message;
            Trace.TraceError(ExceptionText);
            ExceptionStack = theException.StackTrace;
            Trace.TraceError(ExceptionStack);
        }
    }
}