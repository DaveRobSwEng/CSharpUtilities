// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without
// written consent of the copyright owner.
//
// EnumValue.cs
//  Implementation of the Class EnumValue
//
//  Original author: robinsond
//
// $Id:$
// ---------------------------------------------------------------------------

namespace Sepura.DataDictionary
{
    /// <summary>
    /// Placeholder for an error report encountered when decoding a value
    /// </summary>
    public class ErrorValue : Value
    {
        /// <summary>
        /// Gets the error report.
        /// </summary>
        public string ErrorReport
        {
            get;
            private set;
        }

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorValue" /> class.
        /// </summary>
        /// <param name="errorReport">The error report.</param>
        /// <param name="parent">The parent.</param>
        public ErrorValue(string errorReport, Value parent)
            : base(new StringDefinition(), parent)
        {
            ErrorReport = errorReport;
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>
        /// Encodes the value into the list of bytes
        /// </summary>
        /// <param name="theBytes">The bytes.</param>
        public override void Encode(ByteStore theBytes)
        {
            throw new DataDictionaryException("Not implemented");
        }

        /// <summary>
        /// Gets the size in bytes of the value
        /// </summary>
        /// <returns>Size in bytes of the value</returns>
        public override int GetSizeBytes()
        {
            throw new DataDictionaryException("Not implemented");
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return ErrorReport;
        }

        #endregion Public Methods
    }
}