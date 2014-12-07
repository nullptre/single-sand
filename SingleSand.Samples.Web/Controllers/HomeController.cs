using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SingleSand.Samples.Web.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public async Task<ActionResult> Index()
        {
            var app = MvcApplication.ActiveAppInstance;

            //delegate action handling to the event loop
            return await app.RunOnEventLoop(
                async cancellation => await IndexAction(app.RequestsCount, HttpContext.Request.UserHostAddress, cancellation));
        }

        /// <summary>
        /// This method executes inside the event loop
        /// </summary>
        private async Task<ActionResult> IndexAction(IDictionary<IPAddress, int> requestsCount, string userHostAddress, CancellationToken cancellation)
        {
            IPAddress clientIp;
            if (!IPAddress.TryParse(userHostAddress, out clientIp))
                return Content("Client IP is not recognized");

            using (var client = new HttpClient())
            {
                var remoteResponse = await client.GetAsync(string.Format("http://www.bing.com/search?q=who+i+am+{0}", clientIp), cancellation);
                if (remoteResponse != null && remoteResponse.IsSuccessStatusCode
                    && (await remoteResponse.Content.ReadAsStringAsync())
                            .Contains("attack"))
                {
                    requestsCount.Remove(clientIp);
                    return Content("Your IP is not trusted");
                }
            }
            cancellation.ThrowIfCancellationRequested();

            //there is no need for lock(), it's safe to access the requestsCount directly
            int count;
            if (!requestsCount.TryGetValue(clientIp, out count))
                requestsCount.Add(clientIp, count = 1);
            else
                requestsCount[clientIp] = ++count;

            return Content(string.Format("Congratulations, your IP is trusted! Count of your requests: {0}, Handled by thread {1}", count, Thread.CurrentThread.ManagedThreadId));
        }
    }
}
