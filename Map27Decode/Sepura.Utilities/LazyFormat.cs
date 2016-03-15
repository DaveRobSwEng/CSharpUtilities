#region Header
// ---------------------------------------------------------------------------
// Sepura - Commercially Confidential.
// 
// LazyLogger.cs
//  Implementation of the Class LazyLogger
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
    /// Class responsible for lazy string format evaluation to avoid calling string.format until it is required
    /// e.g. LazyFormat lf = new LazyFormat(() => String.Format("Script length:{0}", mFileData.Length.ToString()))
    /// </summary>
    public class LazyFormat 
    {
        /// <summary>
        /// Initializes a new instance of the LazyFormat class
        /// Stores the reference to the lazy evaluation function delegate
        /// </summary>
        /// <param name="theEvaluationFunction"></param>
        public LazyFormat(Func<string> theEvaluationFunction)
        {
            m_EvaluationFunction = theEvaluationFunction;
        }

        /// <summary>
        /// Calls the stored evaluation function and returns the resulting string
        /// </summary>
        /// <returns>String result of evaluating the stored function</returns>
        public override string ToString()
        {
            return m_EvaluationFunction();
        }

        /// <summary>
        /// Delegate called to evaluate the ToString operation when required.
        /// </summary>
        private readonly Func<string> m_EvaluationFunction;
    }
}