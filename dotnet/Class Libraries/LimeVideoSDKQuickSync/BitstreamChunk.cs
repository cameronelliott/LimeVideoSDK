// Copyright 2012-2016 Cameron Elliott  http://cameronelliott.com
// BSD License terms
// See file LICENSE.txt in the top-level directory



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimeVideoSDKQuickSync
{
    /// <summary>
    /// Simple holder for Encoder output
    /// </summary>
    public class BitStreamChunk
    {
        /// <summary>The number of valid bytes in bitstream</summary>
        public int bytesAvailable;
        /// <summary>The bitstream data</summary>
        public byte[] bitstream;
    }
}
