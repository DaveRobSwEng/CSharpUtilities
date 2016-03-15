#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// DataDictionaryException.cs
//  Implementation of the Class DataDictionaryException
//
//  Original author: robinsond
//
// $Id:$ 
// ---------------------------------------------------------------------------
#endregion

namespace Sepura.DataDictionary
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception class raised by Data Dictionary when an irrecoverable internal error
    /// is detected
    /// </summary>
    [Serializable]
    public class DataDictionaryException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataDictionaryException"/> class.
        /// </summary>
        public DataDictionaryException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDictionaryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public DataDictionaryException(string message)
            : base (message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDictionaryException"/> class.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="data">The data items to be formatted</param>
        public DataDictionaryException(string format, params object [] data)
            : base(string.Format (format, data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDictionaryException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataDictionaryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataDictionaryException"/> class.
        /// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object 
        /// data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual 
        /// information about the source or destination.</param>
        protected DataDictionaryException(SerializationInfo info, StreamingContext context) 
            : base (info, context)
        {
        }
    }
}