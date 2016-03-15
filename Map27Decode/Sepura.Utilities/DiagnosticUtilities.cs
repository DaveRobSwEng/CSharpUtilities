#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// DiagnosticUtilities.cs
//  Implementation of the Class DiagnosticUtilities
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities 
{
    using System.Diagnostics;
    
    /// <summary>
    /// Diagnostic utility functions
    /// </summary>
    public sealed class DiagnosticUtilities 
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="DiagnosticUtilities"/> class from being created.
        /// </summary>
        private DiagnosticUtilities()
        {
        }

        /// <summary>
        /// Gets the name of the caller class.
        /// </summary>
        /// <returns>String containing the name of the calling function</returns>
        public static string GetCallerClass()
        {
            StackFrame frame = new StackFrame(1, true);
            return frame.GetMethod().DeclaringType.Name;
        }

        /// <summary>
        /// Gets the name of the caller function.
        /// </summary>
        /// <returns>String containing the name of the calling function</returns>
        public static string GetCallerFunction ()
        {
            StackFrame frame = new StackFrame(1, true);
            return frame.GetMethod().Name;
        }
    }
}