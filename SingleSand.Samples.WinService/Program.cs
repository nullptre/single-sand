using System.ServiceProcess;

namespace SingleSand.Samples.WinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] 
            { 
                new SampleService() 
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
