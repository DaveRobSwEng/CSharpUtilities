#region Header
// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// PartialDecodeException.cs
//  Implementation of the Class PartialDecodeException
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
    /// Exception class raised by Data Dictionary when a signal is only partially decoded
    /// </summary>
    [Serializable]
    public class PartialDecodeException : DataDictionaryException
    {
        /// <summary>
        /// Gets the value being decoded when the exception was caught
        /// </summary>
        public Value ValueBeingDecoded
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialDecodeException" /> class.
        /// </summary>
        /// <param name="valueBeingDecoded">The value being decoded.</param>
        /// <param name="message">The exception message.</param>
        public PartialDecodeException(Value valueBeingDecoded, string message)
            : base(message)
        {
            ValueBeingDecoded = valueBeingDecoded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialDecodeException"/> class.
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="data">The data items to be formatted</param>
        public PartialDecodeException(string format, params object[] data)
            : base(string.Format(format, data))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialDecodeException"/> class.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="innerException">The inner exception.</param>
        public PartialDecodeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartialDecodeException"/> class.
        /// A constructor is needed for serialization when an exception propagates from a remoting server to the client. 
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object 
        /// data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual 
        /// information about the source or destination.</param>
        protected PartialDecodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}