using Akka.Actor;
using Data.Cluster.Pipeline.Shared.Messages;
using Data.Cluster.Pipeline.Shared.Query.Base;
using System.Collections.Generic;
using Data.Cluster.Pipeline.App.Observable2;
using System.Linq;
using System.Reactive.Linq;

namespace Data.Cluster.Pipeline.App.Actors
{
    public class TxActor : UntypedActor
    {
        private const string connectionString =
                    "Persist Security Info=False;Integrated Security=true;Initial Catalog=DemoMicrobatch;server=(local)";

        private readonly DataProcessObservable connection;

        public TxActor()
        {
            connection = new DataProcessObservable();
            connection.AddParameter(ConnectionPatternParamType.ConnectionString, connectionString);
        }

        public class StartProcess
        {
            public StartProcess(IEnumerable<TxItem> items)
            {
                Items = items;
            }
            public IEnumerable<TxItem> Items { get; set; }

            public Tx Get()
            {
                var tx = new Tx
                {
                    Items = Items.ToList()
                };
                return tx;
            }
        }
        protected override void OnReceive(object message)
        {
            if (message is StartProcess msg)
            {
                try
                {
                    var microBatch = connection.Update(msg.Get()).Wait();
                }
                catch
                {
                }
                
            }
        }
    }
}
