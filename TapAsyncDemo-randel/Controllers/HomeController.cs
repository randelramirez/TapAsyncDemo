using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TapAsyncDemo_randel.Models;

namespace TapAsyncDemo_randel.Controllers
{
    public class HomeController : Controller
    {
        #region Data sources

        // Could be loaded from a database (asynchronously!)
        private ICollection<NewsSource> _newsSources = new List<NewsSource> {
            //new NewsSource { Name = "Bill Gates' Twitter feed",   DataFormat = DataFormat.TwitterJson, Url = "http://twitter.com/status/user_timeline/billgates.json?count=10" },
            new NewsSource { Name = "Randel's Facebook Notifications RSS feed",      DataFormat = DataFormat.Rss, Url = "https://www.facebook.com/feeds/notifications.php?id=1428651539&viewer=1428651539&key=AWgS2swuguj8_9H7&format=rss20" },
            new NewsSource { Name = "ScottGu's blog RSS feed",    DataFormat = DataFormat.Rss,         Url = "http://weblogs.asp.net/scottgu/rss.aspx" },
            new NewsSource { Name = "Dan Wahlin's blog RSS feed", DataFormat = DataFormat.Rss,         Url = "https://weblogs.asp.net/dwahlin/rss?containerid=13" },
        };

        #endregion

        public ActionResult FetchSynchronously()
        {

            #region fetch first only
            //var resultItems = new List<Headline>();
            //NewsSource first = this._newsSources.First();
            //string rawData = new WebClient().DownloadString(first.Url);
            //resultItems.AddRange(ParserUtils.ExtractHeadlines(first, rawData));
            //return View("results", resultItems);
            #endregion

            var resultItems = new List<Headline>();

            foreach (var newsSource in _newsSources)
            {
                string rawData =  new WebClient().DownloadString(newsSource.Url);
                resultItems.AddRange(ParserUtils.ExtractHeadlines(newsSource, rawData));
            }
            return View("results", resultItems);

        }

        public async Task<ActionResult> FetchAsynchronously()
        {
            var resultItems = new List<Headline>();

            foreach (var newsSource in _newsSources)
            {
                string rawData = await new WebClient().DownloadStringTaskAsync(newsSource.Url);
                resultItems.AddRange(ParserUtils.ExtractHeadlines(newsSource, rawData));
            }

            return View("results", resultItems);
        }


        public async Task<ActionResult> FetchAsynchronouslyParallel()
        {
            //Task<Headline[]>[] allTasks = (from newSource in this._newsSources
            //                               select this.FetchHeadlinesTaskAsync(newSource)).ToArray();

            var allTasks = from newsSource in _newsSources
                           select FetchHeadlinesTaskAsync(newsSource);
            //headline[headline[], headline[], headline[]] => array of Headline[] *array of headline arays*
            //WhenAll, wait for all them to finish
            Headline[][] results = await Task.WhenAll(allTasks);

            return View("results", results.SelectMany(x => x));
        }

        public async Task<ActionResult> FetchAsynchronouslyParallelFirstToFinish()
        {
            var allTasks = from newsSource in _newsSources
                           select FetchHeadlinesTaskAsync(newsSource);
            //headline[headline[], headline[], headline[]] => array of Headline[] *array of headline arays*
            //WhenAll, wait for all them to finish
            // returns Task<Headline>, returns an instance of the task that completed so that it knows which task was finished
            Task<Headline[]> results = await Task.WhenAny(allTasks);

            return View("results", results.Result);
        }

        public async Task<ActionResult> FetchFirstAsynchronouslyParallel()
        {
            var resultItems = new List<Headline>();

            var allTasks = from newsSource in _newsSources
                           select FetchHeadlinesTaskAsync(newsSource);
            Task<Headline[]> firstCompleted = await Task.WhenAny(allTasks);

            return View("results", firstCompleted.Result);
        }

        private async Task<Headline[]> FetchHeadlinesTaskAsync(NewsSource newsSource)
        {
            string rawData = await new WebClient().DownloadStringTaskAsync(newsSource.Url);
            return ParserUtils.ExtractHeadlines(newsSource, rawData);
        }

        private Task<string> FetchStringEventually()
        {
            var completionSource = new TaskCompletionSource<string>();
            return completionSource.Task;

            // Later..
            completionSource.SetResult("Some string");
        }
    }
}
