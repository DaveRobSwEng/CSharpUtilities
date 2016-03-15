#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// Clipboard.cs
//  Implementation of the Class Clipboard
//
//  Original author: RobinsonD
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System.Runtime.InteropServices;
    using System.Windows;

    /// <summary>
    /// Wraps the basic .NET clipboard operations with some exception handlers and
    /// retries to cope with its fragile operation
    /// </summary>
    public static class Clipboard 
    {
        /// <summary>
        /// Stores the specified data on the Clipboard in the specified format.
        /// </summary>
        /// <param name="format">A string that specifies the format to use to store the data. 
        /// See the System.Windows.DataFormats class for a set of predefined data formats.
        /// </param>
        /// <param name="data">An object representing the data to store on the Clipboard.</param>
        public static void SetData(string format, object data)
        {
            try
            {
                System.Windows.Clipboard.SetData(format, data);
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Stores System.Windows.DataFormats.UnicodeText data on the Clipboard.
        /// </summary>
        /// <param name="text">A string that contains the System.Windows.DataFormats.UnicodeText data to store on the Clipboard.</param>
        /// <exception cref="System.ArgumentNullException">The text is null</exception>
        public static void SetText(string text)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
            }
            catch (COMException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}