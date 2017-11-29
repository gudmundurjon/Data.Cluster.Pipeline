using Data.Cluster.Pipeline.App.Query.Base;
using Data.Cluster.Pipeline.Shared.Extensions;
using Data.Cluster.Pipeline.Shared.Messages;
using Data.Cluster.Pipeline.Shared.Query.Base;
using System;
using System.Messaging;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Data.Cluster.Pipeline.Shared.Query
{
    public class QueueObservable : BaseConnectionPattern, IConnectionPattern<TxItem>
    {
        public IObservable<TxItem> Queue()
        {
            string connection = this.GetParameterValue(ConnectionPatternParamType.ConnectionString);
            //Guard.ArgumentNotNullOrEmpty(connection, "ConnectionString");

            string controlQueue = this.GetParameterValue(ConnectionPatternParamType.ControlQueue);
            //Guard.ArgumentNotNullOrEmpty(controlQueue, "Control Queue");

            return FromQueue(string.Format("{0}\\Private$\\{1}", connection, controlQueue));
        }

        private static IObservable<TxItem> FromQueue(string serverQueueName)
        {
            return Observable.Create<TxItem>(
                observer =>
                {
                    var responseQueue = string.Format(
                        "{0}\\Private$\\{1}",
                        Environment.MachineName,
                        Guid.NewGuid().ToString());
                    var queue = MessageQueue.Create(responseQueue);

                    var binaryFormatter = new BinaryMessageFormatter();
                    var serverQueue = new MessageQueue(serverQueueName) { Formatter = binaryFormatter };
                    queue.Formatter = binaryFormatter;

                    serverQueue.Send("S " + responseQueue);

                    var loop = NewThreadScheduler.Default.ScheduleLongRunning(
                        cancel =>
                        {
                            while (!cancel.IsDisposed)
                            {
                                var msg = queue.Receive();
                                if (msg != null)
                                {
                                    var byteMsg = (byte[])msg.Body;
                                    var m = byteMsg.Deserialize<TxItem>();
                                    observer.OnNext(m);
                                }
                                    //TODO: handle null messages with dignity                                        
                                }
                        });

                    return new CompositeDisposable(loop, Disposable.Create(() => { serverQueue.Send("D " + responseQueue); }));
                });
        }
    }
}
