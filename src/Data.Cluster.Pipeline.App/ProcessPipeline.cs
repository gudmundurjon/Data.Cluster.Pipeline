using Akka.Actor;
using Data.Cluster.Pipeline.App.Actors;
using Data.Cluster.Pipeline.Shared.Messages;
using Data.Cluster.Pipeline.Shared.Query;
using Data.Cluster.Pipeline.Shared.Query.Base;
using Serilog;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Topshelf.Logging;

namespace Data.Cluster.Pipeline.App
{
    public class ProcessPipeline
    {
        private readonly ActorSystem _system;

        private readonly IActorRef _txActor;

        private LogWriter _log;

        private IDisposable bufferSubscriber;

        public ProcessPipeline()
        {
            _log = HostLogger.Get<ProcessPipeline>();

            _log.Info("Cluster Starting...");

            _system = ActorSystem.Create(
                Constants.ActorSystemName,
                "akka { loglevel=INFO,  loggers=[\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]}");

            var txProps = Props.Create<TxActor>();
            _txActor = _system.ActorOf(txProps, Constants.TxName);

            var connection = new QueueObservable();
            connection.AddParameter(ConnectionPatternParamType.ConnectionString, Environment.MachineName);
            connection.AddParameter(ConnectionPatternParamType.ControlQueue, Constants.ControlQueueName);

            var messages = connection.Queue();

            this.bufferSubscriber = messages.Buffer(TimeSpan.FromSeconds(1)).Subscribe(c => this.Process(c));
        }

        public void Process(IEnumerable<TxItem> items)
        {
            _log.Info("Telling cluster to process records");
            _txActor.Tell(new TxActor.StartProcess(items));
        }
    }
}
