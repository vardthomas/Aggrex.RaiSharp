﻿using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aggrex.Configuration;
using Aggrex.Network.Messages;
using Aggrex.Network.Messages.KeepAlive;
using Aggrex.Network.Packets;
using Aggrex.Network.Requests;
using Microsoft.Extensions.Logging;

namespace Aggrex.Network
{
    /// <summary>
    /// Represents a client node in the system. 
    /// </summary>
    internal class LocalNode : ILocalNode
    {
        private readonly INetworkListenerLoop _networkListenerLoop;
        private readonly IUPnPPortForwarder _uPnPPortForwarder;
        private readonly IPeerTracker _peerTracker;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly ILogger<LocalNode> _logger;
        private readonly ClientSettings _clientSettings;
        private RemoteNode.Factory _remoteNodeFactory { get; set; }
        private Timer _keepAliveTimer;
        private IPacketSender _packetSender;
        private MessageHeader _messageHeader;

        public LocalNode(
            INetworkListenerLoop networkListenerLoop,
            IUPnPPortForwarder portForwarder,
            ILocalIpAddressDiscoverer localIpAddressDiscoverer,
            RemoteNode.Factory remoteNodeFactory,
            IPeerTracker peerTracker,
            IPacketSender packetSender,
            ILoggerFactory loggerFactory,
            IMessageDispatcher messageDispatcher,
            ClientSettings clientSettings)
        {
            _logger = loggerFactory.CreateLogger<LocalNode>();

            _uPnPPortForwarder = portForwarder;
            _peerTracker = peerTracker;

            _packetSender = packetSender;

            _networkListenerLoop = networkListenerLoop;
            //_networkListenerLoop.TcpConnectionEstablished += HandleConnectionEstablished;
            _networkListenerLoop.DatagramReceived += HandleDatagramReceived;

            _messageDispatcher = messageDispatcher;

            _clientSettings = clientSettings;
            _remoteNodeFactory = remoteNodeFactory;

            int port = _clientSettings.BlockChainNetSettings.TcpPort;
            LocalAddress = new IPEndPoint(IPAddress.Parse(localIpAddressDiscoverer.GetLocalIpAddress()), port);

            _logger.LogInformation($"Started Listening on {LocalAddress.Address}:{LocalAddress.Port}");

            _keepAliveTimer = new Timer(BroadcastKeepAliveMessages, null, 0, _clientSettings.KeepAliveTimeout * 1000);


            _messageHeader = new MessageHeader();
            _messageHeader.Extensions = new byte[2];
            _messageHeader.VersionMax = 5;
            _messageHeader.VersionMin = 1;
            _messageHeader.VersionUsing = 5;
            _messageHeader.Type = MessageType.Keepalive;
        }

        private async void BroadcastKeepAliveMessages(object state)
        {
            foreach (var peer in _peerTracker.GetAllTrackedPeers())
            {
                var randomSetOfPeers = _peerTracker.GetRandomSetOfTrackedPeers(8);
                var keepAliveMessage = new KeepAliveMessage(_messageHeader);
                keepAliveMessage.Peers = randomSetOfPeers.Select(x => x.IpEndPoint).ToArray();

                await _packetSender.SendPacket(keepAliveMessage, peer.IpEndPoint);
            }
        }

        private void HandleDatagramReceived(object sender, DataGramReceivedArgs args)
        {
            var messageHeader = new MessageHeader();
            using (var reader = new BinaryReader(new MemoryStream(args.Data)))
            {
                if (messageHeader.ReadFromStream(reader))
                {
                    _logger.LogDebug("Recieved {PACKET} from {SENDER}", messageHeader.Type.ToString(), args.Sender.Address.ToString());
                    _messageDispatcher.DispatchDatagramMessage(messageHeader, reader, args.Sender);
                }
            }
        }

        public IPEndPoint LocalAddress { get; private set; }

        public void Start()
        {
            Task.Run(async () =>
            {
                await _uPnPPortForwarder.ForwardPortIfNatFound();
                Task.Run(() => _networkListenerLoop.ExecuteTcpListenerLoop());
                Task.Run(() => _networkListenerLoop.ExecuteUdpListenerLoop());
            });
        }

        //private void ConnectToPeer(IPEndPoint endpoint)
        //{
        //    TcpClient client = new TcpClient();
        //    client.Connect(endpoint);

        //    if (client.Connected)
        //    {
        //        OnNodeConnectionEstablished(client);
        //    }
        //}

        //private void OnNodeConnectionEstablished(TcpClient client)
        //{
        //    IRemoteNode newNode = _remoteNodeFactory.Invoke(client);
        //    newNode.ExecuteProtocolLoop();
        //}

        //private void HandleConnectionEstablished(object sender, TcpClient client)
        //{
        //    OnNodeConnectionEstablished(client);
        //}
    }
}