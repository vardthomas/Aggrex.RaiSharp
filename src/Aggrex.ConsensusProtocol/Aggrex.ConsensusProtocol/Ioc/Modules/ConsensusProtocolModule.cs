﻿using Aggrex.ConsensusProtocol.Messages;
using Aggrex.ConsensusProtocol.TransactionProcessors;
using Aggrex.ConsensusProtocol.Transactions;
using Aggrex.ConsensusProtocol.Transactions.Dispatcher;
using Aggrex.Framework;
using Aggrex.Network;
using Aggrex.Network.HandShakes;
using Aggrex.Network.Messages;
using Aggrex.Network.Messages.MessageProcessor;
using Aggrex.Network.Requests;
using Autofac;

namespace Aggrex.ConsensusProtocol.Ioc.Modules
{
    public class ConsensusProtocolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LocalNode>()
                .As<ILocalNode>()
                .SingleInstance();

            builder.RegisterType<MessageDispatcher>()
                .As<IMessageDispatcher>()
                .SingleInstance();

            builder.RegisterType<ActiveNodeSet>()
                .As<IActiveNodeSet>()
                .SingleInstance();

            builder.RegisterType<TransferTransactionProcessor>()
                .SingleInstance().Keyed<ITransactionProcessor>(TransactionType.TransferTransaction);

            builder.RegisterType<TransactionDispatcher>()
                .As<ITransactionDispatcher>()
                .SingleInstance();
        }
    }
}

