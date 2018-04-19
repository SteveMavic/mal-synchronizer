using MAL_Synchronizer.Controllers;
using MAL_Synchronizer.Model;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Synchronizer.WebsiteModel
{
    public class MALModel : WebsiteModel
    {
        private By listContainer = By.ClassName("list-item");
        private string _url = "https://myanimelist.net/animelist/";

        public override string UserName { get => base.UserName;
            set
            {
                base.UserName = value;
                Url = $"{_url}{value}";
            }
        }

        public MALModel()
        {
            Url = "https://myanimelist.net/animelist/";
        }
        public MALModel(string userName)
        {
            Url = "https://myanimelist.net/animelist/";
            UserName = userName;
        }

        public override void GetAnimeList(FirefoxController firefox)
        {
            base.GetAnimeList(firefox);
            var animeList = firefox.TryGetElements(listContainer);
            AnimeList.Clear();
            foreach (var item in animeList)
            {
                Anime anime = new Anime();
                var statusElement = firefox.TryGetElementInElement(item, By.CssSelector("tr > td:nth-child(1)"));

                string status = statusElement?.GetAttribute("class");

                if (!string.IsNullOrEmpty(status))
                {
                    Console.WriteLine(status);
                    if (status.Contains("watching"))
                        anime.Status = AnimeStatus.Watching;
                    else if (status.Contains("completed"))
                        anime.Status = AnimeStatus.Completed;
                    else if (status.Contains("dropped"))
                        anime.Status = AnimeStatus.Dropped;
                    else if (status.Contains("plantowatch"))
                        anime.Status = AnimeStatus.PlanToWatch;
                    else if (status.Contains("onhold"))
                        anime.Status = AnimeStatus.OnHold;
                }

                var titleElement = firefox.TryGetElementInElement(item, By.CssSelector(".data.title.clearfix > .link.sort"));

                string url = titleElement?.GetAttribute("href");
                string title = titleElement?.GetAttribute("innerHTML");
                if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(url))
                {
                    anime.Url = url;
                    anime.Title = title;
                }

                var scoreElement = firefox.TryGetElementInElement(item, By.CssSelector(".data.score > .link"));
                string scoreStr = scoreElement?.GetAttribute("innerHTML");

                bool parsedScore = int.TryParse(scoreStr, out int score);
                anime.OverallScore = parsedScore ? score : -1;
                
                if (anime.Status != AnimeStatus.Completed)
                {
                    var progressElement = firefox.TryGetElementInElement(item, By.CssSelector(".data.progress > div:nth-child(1) > span:nth-child(1) > a:nth-child(1)"));
                    bool parsedProgress = int.TryParse(progressElement?.Text ?? "0", out int progress);
                    anime.Progress = parsedProgress ? progress : 0;
                    Console.WriteLine($"Progress: {progress}");
                }

                AnimeList.Add(anime);
            }
            OnListDownloadCompleted();
        }
    }
}
