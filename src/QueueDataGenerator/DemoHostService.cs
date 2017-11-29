using Data.Cluster.Pipeline.Shared.Extensions;
using Data.Cluster.Pipeline.Shared.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Messaging;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Topshelf;

namespace QueueDataGenerator
{
    public class DemoHostService : ServiceControl
    {
        private readonly Stopwatch stopwatch;

        private IDisposable sourceSubscriber;

        public DemoHostService()
        {
            this.stopwatch = new Stopwatch();
        }

        public bool Start(HostControl hostControl)
        {
            this.stopwatch.Start();
            Console.WriteLine(@"Starting the queue data generator");

            var observer = Msmq();

            // Data stream
            var integerSequence = Observable.Interval(TimeSpan.FromMilliseconds(2));

            var source = (from n in integerSequence select TxItem.Demo(n));
            this.sourceSubscriber = source.Subscribe(observer);

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            this.sourceSubscriber.Dispose();

            this.stopwatch.Stop();
            return true;
        }

        public static IObserver<TxItem> Msmq()
        {
            var q = string.Format("{0}\\Private$\\{1}", Environment.MachineName, "ControlQueue");
            var queue = (MessageQueue.Exists(q)) ? new MessageQueue(q) : MessageQueue.Create(q);

            var format = new BinaryMessageFormatter();
            queue.Formatter = format;

            var incoming = Observable.Create<string>(observer =>
            {
                return NewThreadScheduler.Default.ScheduleLongRunning(cancel =>
                {
                    while (!cancel.IsDisposed)
                    {
                        var msg = queue.Receive();
                        observer.OnNext((string)msg.Body);
                    }
                });
            });

            var sub = new ReplaySubject<TxItem>();

            var map = new Dictionary<string, IDisposable>();

            incoming.Subscribe(clientQueueName =>
            {
                var command = clientQueueName[0];
                var target = clientQueueName.Substring(2);

                switch (command)
                {
                    case 'S':
                        {
                            var clientQueueFormatter = new BinaryMessageFormatter();
                            var clientQueue = new MessageQueue(target) {
                                Formatter = clientQueueFormatter };

                            map[target] = sub.Subscribe(pt =>
                            {
                                clientQueue.Send(pt.SerializeToByteArray());
                            });
                        }
                        break;
                    case 'D':
                        {
                            var d = default(IDisposable);
                            if (map.TryGetValue(target, out d))
                                d.Dispose();
                        }
                        break;
                    default:
                        throw new Exception("Unknow command");
                }
            });

            return sub;
        }
    }
}
