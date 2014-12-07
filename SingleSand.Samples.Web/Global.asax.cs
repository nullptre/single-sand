using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using NLog;
using SingleSand.Tasks;

namespace SingleSand.Samples.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// CancellationTokenSource is used as global switch that allows to terminate all pending request handlers,
        /// for examplen on app shutdown
        /// </summary>
        private readonly CancellationTokenSource _appCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// TaskScheduler that posts new tasks to the evnet loop
        /// </summary>
        private TaskScheduler _eventLoopScheduler;

        /// <summary>
        /// A counters collection that is shared across all requests. Note that it is not thread-safe, there is not need for thread safety.
        /// </summary>
        private readonly IDictionary<IPAddress, int> _requestsCount = new Dictionary<IPAddress, int>();

        /// <summary>
        /// The only correct way to get access to the app instance - a static field
        /// </summary>
        public static MvcApplication ActiveAppInstance { get; private set; }

        public IDictionary<IPAddress, int> RequestsCount
        {
            get { return _requestsCount; }
        }

        protected void Application_Start()
        {
            ActiveAppInstance = this;

            //regular initialization
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //then we need to start the event loop
            //if should run on separate thread, pull it from the thread pool
            Task.Factory.StartNew(() => EventLoop.Run(OnAppStart, false), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// This method is the first one that runs inside the event loop
        /// </summary>
        /// <returns></returns>
        private Task OnAppStart()
        {
            _eventLoopScheduler = TaskScheduler.FromCurrentSynchronizationContext();

            //initialize the requests counters, for example add some initial values to it
            _requestsCount.Add(new IPAddress(new byte[] { 127, 0, 0, 1 }), 10);

            return Task.FromResult(0);
        }

        protected void Application_End(object sender, EventArgs e)
        {
            Log.Info("Web app is shutting down");

            _appCancellationTokenSource.Cancel();
        }

        /// <summary>
        /// It posts given actionHandler to the event loop and returns a correspondig task
        /// </summary>
        /// <returns>Task that completes when actionHandler and contains a result</returns>
        public async Task<T> RunOnEventLoop<T>(Func<CancellationToken, Task<T>> actionHandler)
        {
            return await await Task.Factory.StartNew(
                async () => await actionHandler(_appCancellationTokenSource.Token),
                _appCancellationTokenSource.Token, TaskCreationOptions.None, _eventLoopScheduler);
        }
    }
}