﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using Aggrex.Network.Messages;
using Aggrex.Network.Requests;

namespace Aggrex.ConsensusProtocol.Messages.KeepAlive
{
    public class KeepAliveMessage : BaseMessage
    {
        public IPEndPoint[] Peers { get; set; } = new IPEndPoint[8];

        protected override void WriteProperties(BinaryWriter writer)
        {
            for (int i = 0; i < Peers.Length; i++)
            {
                if (Peers[i] != null)
                {
                    var bytes = Peers[i].Address.MapToIPv6().GetAddressBytes();
                    writer.Write(bytes);
                    writer.Write((short)Peers[i].Port);
                }
                else
                {
                    writer.Write(new byte[16]);
                    writer.Write((ushort)0);
                }
            }
        }

        protected override void ReadProperties(BinaryReader reader)
        {
            for (int i = 0; i < Peers.Length; i++)
            {
                var bytesRead = reader.ReadBytes(16);
                var port = reader.ReadUInt16();
                if (bytesRead.Any(b => b != 0))
                {
                    var ipAddres = IPAddress.Parse(BitConverter.ToString(bytesRead).Replace("-", ""));
                    Peers[i] = new IPEndPoint(ipAddres, port);
                }
            }
        }
    }
}