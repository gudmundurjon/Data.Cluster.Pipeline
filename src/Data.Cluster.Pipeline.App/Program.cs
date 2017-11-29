using Data.Cluster.Pipeline.App.Services;
using Serilog;
using Topshelf;

namespace Data.Cluster.Pipeline.App
{
    class Program
    {
        static int Main(string[] args)
        {
            ILogger configuration = new LoggerConfiguration()
                .WriteTo.ColoredConsole()
                .WriteTo.Seq("http://localhost:5341/")
                .CreateLogger();

            return (int)HostFactory.Run(x =>
            {
                // run as me
                x.RunAsPrompt();

                x.UseSerilog(configuration);
                x.Service<AkkaService>();
                // this will enable Pause and Continue, may break this out into a separate interface to 
                // reduce the footprint on the service code if pause and continue are not supported.
                x.EnablePauseAndContinue();

                x.EnableServiceRecovery(rc =>
                {
                    rc.RestartService(1);
                    //rc.RestartSystem(1, "System is restarting!"); 
                    rc.RunProgram(1, "notepad.exe"); 
                    rc.SetResetPeriod(1); 
                });
            });
        }
    }
}
