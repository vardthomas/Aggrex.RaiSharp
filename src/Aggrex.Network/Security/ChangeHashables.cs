﻿using System.Collections.Generic;
using Aggrex.Common.BitSharp;
using Blake2Sharp;

namespace Aggrex.Network.Security
{
    public class ChangeHashables
    {
        public ChangeHashables(UInt256 previous, UInt256 representative)
        {
            Previous = previous;
            Representative = representative;
        }

        public UInt256 Previous { get; set; }
        public UInt256 Representative { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public UInt256 Hash()
        {
            var hasher = Blake2B.Create(new Blake2BConfig()
            {
                OutputSizeInBytes = 64
            });

            hasher.Init();
            hasher.Update(Previous.ToByteArray());
            hasher.Update(Representative.ToByteArray());

            return new UInt256(hasher.Finish());
        }
    }
}