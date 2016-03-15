#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// FileUtilities.cs
//  Implementation of the Class FileUtilities
//
//  Original author: RobinsonD
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Utilities class supplying various functions relating to files and file names
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Attempts to convert an absolute file path to a relative path (relative to the base directory)
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <param name="baseDirectory"></param>
        /// <param name="relativePath"></param>
        /// <returns>True if conversion performed</returns>
        public static bool TryConvertAbsolutePathToRelative (string absolutePath, string baseDirectory, out string relativePath)
        {
            bool conversionPerformed = false;

            string[] baseDirectoryElements = baseDirectory.Split(new char[] { '\\' });
            string[] fileDirectoryElements = absolutePath.Split(new char[] { '\\' });

            // Scan along the absolute and base directory paths while elements of the paths match
            // e.g.
            // file:   c:\path1\path2\path3\path4\script.lua", 
            // dir:    c:\path1\path2\path3
            // result:                    .\path4\script.lua
            //
            // file:   c:\path1\path2\path5\path4\script.lua", 
            // dir:    c:\path1\path2\path3
            // result:             ..\path5\path4\script.lua

            // Index points to first non-matching path element (0 if none found)
            int index = 0;
            while (index < baseDirectoryElements.Length &&
                    index < fileDirectoryElements.Length &&
                    string.Equals(baseDirectoryElements[index], fileDirectoryElements[index], StringComparison.OrdinalIgnoreCase))
            {
                ++index;
            }

            if (index == 0)
            {
                // No match found
                relativePath = absolutePath;
            }
            else if (index == baseDirectoryElements.Length)
            {
                // All elements of directory path matched
                StringBuilder sb = new StringBuilder();
                sb.Append(".");
                while (index < fileDirectoryElements.Length)
                {
                    sb.AppendFormat("\\{0}", fileDirectoryElements[index]);
                    ++index;
                }

                relativePath = sb.ToString();
                conversionPerformed = true;
            }
            else
            {
                // Partial match for directory path
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < (baseDirectoryElements.Length - index); i++)
                {
                    if (i == 0)
                    {
                        sb.Append("..");
                    }
                    else
                    {
                        sb.Append("\\..");
                    }
                }

                while (index < fileDirectoryElements.Length)
                {
                    sb.AppendFormat("\\{0}", fileDirectoryElements[index]);
                    ++index;
                }

                relativePath = sb.ToString();
                conversionPerformed = true;
            }

            return conversionPerformed;
        }

        /// <summary>
        /// Returns true if the path starts with &lt;letter&gt;:\
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if the path starts with letter then colon then slash</returns>
        public static bool IsAbsolutePath (string path)
        {
            return MatchStartFullPath.IsMatch(path);
        }

        /// <summary>
        /// Attempts to convert a relative path to an absolute path, given the base directory
        /// Validates that the file exists
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="baseDirectory"></param>
        /// <param name="absolutePath"></param>
        /// <returns>True if the path is successfully converted</returns>
        public static bool TryConvertRelativePathToAbsolute(string relativePath, string baseDirectory, out string absolutePath)
        {
            if (IsAbsolutePath (relativePath))
            {
                absolutePath = relativePath;
                return true;
            }

            string tempPath = String.Empty;
            if (relativePath.StartsWith(".\\", StringComparison.Ordinal) || relativePath.StartsWith("..\\", StringComparison.Ordinal))
            {
                tempPath = baseDirectory + relativePath;
            }
            else
            {
                tempPath = baseDirectory + "\\" + relativePath;
            }

            bool conversionPerformed = File.Exists(tempPath);
            absolutePath = conversionPerformed ? tempPath : relativePath;
            return conversionPerformed;
        }

        /// <summary>
        /// Attempts to convert a relative path to an absolute path, given the base directory
        /// Does not validate that the file exists
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="baseDirectory"></param>
        /// <returns>New absolute file path</returns>
        public static string ConvertRelativePathToAbsolute(string relativePath, string baseDirectory)
        {
            string absolutePath = String.Empty;
            if (relativePath.StartsWith(".\\", StringComparison.Ordinal) || relativePath.StartsWith("..\\", StringComparison.Ordinal))
            {
                absolutePath = baseDirectory + relativePath;
            }
            else
            {
                absolutePath = baseDirectory + "\\" + relativePath;
            }

            return absolutePath;
        }

        /// <summary>
        /// Converts the name of a file to a name suitable for naming an object.
        /// Used for diagnostic purposes when assigning a name to a control that represents a file
        /// so that the name is accessible to test tools such as TestComplete
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>Name of the file converted to an object name</returns>
        public static string ConvertFileNameToObjectName(string fileName)
        {
            string validNameChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
            StringBuilder sb = new StringBuilder("Object_");
            foreach (var c in fileName)
            {
                if (validNameChars.IndexOf (c) >= 0)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Regular expression to match the start of a full path name (absolute path)
        /// </summary>
        private static Regex MatchStartFullPath = new Regex(@"[a-zA-Z]:\\");
    }
}
