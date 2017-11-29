namespace Data.Cluster.Pipeline.App.Services
{
    using Akka.Actor;
    using System;
    using System.Threading;
    using Topshelf;
    using Topshelf.Logging;


    class AkkaService :
        ServiceControl,
        ServiceSuspend,
        ServiceShutdown,
        ServiceSessionChange,
        ServiceCustomCommand
    {
        LogWriter _log;
        ProcessPipeline _pipeline;

        public bool Start(HostControl hostControl)
        {
            _log = HostLogger.Get<AkkaService>();

            _log.Info("Service Starting...");

            hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(10));

            Thread.Sleep(1000);
            /*
            ThreadPool.QueueUserWorkItem(x =>
            {
                
                Thread.Sleep(3000);
                _log.Info("Requesting a restart!!!");
                hostControl.Restart();
                _log.Info("Dying an ungraceful death");
                throw new InvalidOperationException("Oh, what a world.");
            });
            */
            _log.Info("AkkaService Started");

            _pipeline = new ProcessPipeline();

            // Lets subscribe to Data-Stream


            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _log.Info("SampleService Stopped");

            return true;
        }

        public void SessionChange(HostControl hostControl, SessionChangedArguments changedArguments)
        {
            _log.Info("Service session changed");
        }

        public void Shutdown(HostControl hostControl)
        {
            _log.Info("Service is being shutdown, bye!");
        }

        public bool Pause(HostControl hostControl)
        {
            _log.Info("SampleService Paused");

            return true;
        }

        public bool Continue(HostControl hostControl)
        {
            _log.Info("SampleService Continued");

            return true;
        }

        public void CustomCommand(HostControl hostControl, int command)
        {
            _log.Info(string.Format("Custom command received: {0}", command));
        }
    }
}
