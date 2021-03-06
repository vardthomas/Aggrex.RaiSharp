﻿using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Aggrex.Common.BitSharp;
using Blake2Sharp;

namespace Aggrex.Network.Security
{
    public class SendHashables
    {
        public SendHashables(UInt256 destination, UInt256 previous, UInt128 balance)
        {
            Destination = destination;
            Previous = previous;
            Balance = balance;
        }

        public UInt256 Destination { get; set; }
        public UInt256 Previous { get; set; }
        public UInt128 Balance { get; set; }
        public UInt256 Hash()
        {
            var hasher = Blake2B.Create(new Blake2BConfig()
            {
                OutputSizeInBytes = 64
            });

            hasher.Init();
            hasher.Update(Previous.ToByteArray());
            hasher.Update(Destination.ToByteArray());
            hasher.Update(Balance.ToByteArray());

            return new UInt256(hasher.Finish());
        }
    }
}