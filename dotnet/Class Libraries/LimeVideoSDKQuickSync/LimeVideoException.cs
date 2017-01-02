// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory

using LimeVideoSDK.QuickSyncTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LimeVideoSDK.QuickSync
{
    /// <summary>
    /// Exception type specifically for this SDK
    /// </summary>
    public class QuickSyncException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public QuickSyncException(string message)
            : base(message)
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sts"></param>
        public QuickSyncException(string message, mfxStatus sts)
            : base(sts.ToString() + ":" + message)
        {

        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sts"></param>
        public unsafe QuickSyncException(sbyte* message, mfxStatus sts)
            : base(sts.ToString() + ":" + new string(message))
        {

        }
    }
}
