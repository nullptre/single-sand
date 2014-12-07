using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using SingleSand.Tasks;

namespace SingleSand.Samples.WinService
{
    public partial class SampleService : ServiceBase
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// CancellationTokenSource is used as global switch that tells to all pending tasks to terminate,
        /// for examplen on service shutdown
        /// </summary>
        private readonly CancellationTokenSource _appCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// A task taht has been elecuting last time.
        /// This field cannot be null so initial value is dummy.
        /// </summary>
        private Task _activeTask = Task.FromResult(0);

        private int _counter;

        public SampleService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //then we need to start the event loop
            //if should run on separate thread, pull it from the thread pool
            Task.Factory.StartNew(() => EventLoop.Run(OnServiceStart, false), TaskCreationOptions.LongRunning);
        }

        private Task OnServiceStart()
        {
            //Do some initializaion logic inside the event loop, for example start periodic task
            _activeTask = DoSomething(); //it is not needed to await it because it is a periodic task, let it run asynchronously

            return Task.FromResult(0);
        }

        private async Task DoSomething()
        {
            var cancellation = _appCancellationTokenSource.Token;

            try
            {
                //wait a bit
                await Task.Delay(TimeSpan.FromSeconds(_counter % 100 == 99 ? 200 : 1), cancellation);

                cancellation.ThrowIfCancellationRequested();

                using (var client = new HttpClient())
                {
                    var remoteResponse = await client.GetAsync(
                        string.Format("http://www.bing.com/search?q=what+does+{0}+mean", ++_counter), cancellation);
                    if (remoteResponse != null && remoteResponse.IsSuccessStatusCode
                        && (await remoteResponse.Content.ReadAsStringAsync())
                            .Contains("lucky"))
                    {
                        Log.Info("We have found a lucky number! It is {0}", _counter);
                    }
                }

                cancellation.ThrowIfCancellationRequested();
            }
            catch (TaskCanceledException)
            {
                Log.Info("Cannot check number {0} because is has been cancelled", _counter);
                return;
            }

            _activeTask = DoSomething(); //start it again in parallel
        }

        protected override void OnStop()
        {
            Log.Info("Service is shutting down");

            //notify all pending tasks about shutdown
            _appCancellationTokenSource.Cancel();

            //wait until current task finishes
            _activeTask.Wait();
        }
    }
}
