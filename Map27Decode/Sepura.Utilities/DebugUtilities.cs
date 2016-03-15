#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// Debug.cs
//  Implementation of the Class Debug
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.Utilities
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Debug utilities class
    /// </summary>
    public sealed class DebugUtilities
    {
        /// <summary>
        /// Prevents a default instance of the <see cref="DebugUtilities"/> class from being created.
        /// </summary>
        private DebugUtilities()
        {
        }

        [DllImport("Dbghelp.dll")]
        private static extern bool MiniDumpWriteDump(IntPtr process, uint processId, IntPtr file, int dumpType, ref MINIDUMP_EXCEPTION_INFORMATION ExceptionParam, IntPtr UserStreamParam, IntPtr CallbackParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        /// <summary>
        /// Struct mapping to MINIDUMP_EXCEPTION_INFORMATION for Win32 API
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct MINIDUMP_EXCEPTION_INFORMATION
        {
            /// <summary>
            /// The thread id
            /// </summary>
            public uint ThreadId;

            /// <summary>
            /// The exception pointers
            /// </summary>
            public IntPtr ExceptionPointers;

            /// <summary>
            /// The client pointers
            /// </summary>
            public int ClientPointers;
        }

        /// <summary>
        /// Call back function that will be called when an exception has occurred. This function will create
        /// a full memory dump of the application
        /// </summary>
        /// <returns>Name of the generated dump file</returns>
        public static string GenerateMemoryDump()
        {
            string assemblyPath = Assembly.GetEntryAssembly().Location;
            string dumpFileName = assemblyPath + "_" + DateTime.Now.ToString("dd.MM.yyyy.HH.mm.ss") + ".dmp";
            FileStream file = new FileStream(dumpFileName, FileMode.Create);
            MINIDUMP_EXCEPTION_INFORMATION info = new MINIDUMP_EXCEPTION_INFORMATION();
            info.ClientPointers = 1;
            info.ExceptionPointers = Marshal.GetExceptionPointers();
            info.ThreadId = GetCurrentThreadId();

            // A full memory dump is necessary in the case of a managed application, other wise no information
            // regarding the managed code will be available
            MiniDumpWriteDump(GetCurrentProcess(), GetCurrentProcessId(), file.SafeFileHandle.DangerousGetHandle(), MiniDumpWithFullMemory, ref info, IntPtr.Zero, IntPtr.Zero);
            file.Close();

            return dumpFileName;
        }

        /// <summary>
        /// The mini dump with full memory
        /// </summary>
        private const int MiniDumpWithFullMemory = 2;
    }
}