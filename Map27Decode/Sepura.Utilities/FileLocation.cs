#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// FileLocation.cs
//  Implementation of the Class FileLocation
//
//  Original author: RobinsonD
//
// $Id:$
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    /// <summary>
    /// Helper class containing a specific location within a file, specified by file + line number
    /// </summary>
    public class FileLocation
    {
        /// <summary>
        /// Gets the name of the file
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the line number within the file
        /// </summary>
        public int FileLine
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the FileLocation class
        /// </summary>
        /// <param name="theFileName"></param>
        /// <param name="theFileLine"></param>
        public FileLocation(string theFileName, int theFileLine)
        {
            FileName = theFileName;
            FileLine = theFileLine;
        }
    }
}
