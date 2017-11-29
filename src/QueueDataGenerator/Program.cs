using Topshelf;

namespace QueueDataGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<DemoHostService>();
                x.RunAsLocalSystem();
                x.SetDescription("Demo Queue Test Data Generator Host");
                x.SetDisplayName("Demo Queue Test Data Generator");
            });
        }
    }
}
