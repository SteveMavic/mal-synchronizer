using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MAL_Synchronizer.Controllers;
using MAL_Synchronizer.Model;
using OpenQA.Selenium;

namespace MAL_Synchronizer.WebsiteModel
{
    public class ShindenModel : WebsiteModel
    {
        private string _userID;

        private By listContainer = By.ClassName("list-content");

        public string UserID
        {
            get { return _userID; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                   _userID = value;
            }
        }
        public override string UserName { get => base.UserName; set { base.UserName = value; } }

        public void SetUserNameAndID(string userName, string userID)
        {
            UserID = userID;
            UserName = userName;
            Url = $"https://shinden.pl/animelist/{UserID}-{UserName}/all";
        }

        public ShindenModel()
        {
            Url = "https://shinden.pl/animelist/";
        }
        public ShindenModel(string userName, string userID)  
        {
            UserName = userName;
            UserID = userID;
            Url = string.Concat("https://shinden.pl/animelist/", UserID, "-", UserName, "/all");
        }

        public override void GetAnimeList(FirefoxController firefox)
        {
            base.GetAnimeList(firefox);
            //Thread.Sleep(2000);
            var listContentElement = firefox.WaitUntilElementClickable(listContainer);
            AnimeList.Clear();
            if (listContentElement != null)
            {
                var tables = firefox.TryGetElements(By.XPath("//table[contains(@id,'ver-zebra')]")).ToList();
                Console.WriteLine(tables.Count);
                foreach (var table in tables)
                {
                    var listItemElements = firefox.TryGetElementsInElement(table, By.CssSelector("tbody:nth-child(3) > tr")).ToList();
                    Console.WriteLine(listItemElements.Count);
                    foreach (var item in listItemElements)
                    {
                        var anime = new Anime();
                        var titleElement = firefox.TryGetElementInElement(item, By.CssSelector(".title-tooltip.title-col > a"));

                        string url = titleElement?.GetAttribute("href");
                        string title = titleElement?.GetAttribute("innerHTML");

                        anime.Title = title;
                        anime.Url = url;

                        var scoreElement = firefox.TryGetElementInElement(item, By.CssSelector(".rate.total"));
                        bool parsedScore = int.TryParse(scoreElement.GetAttribute("innerHTML"), out int score);
                        anime.OverallScore = parsedScore ? score : -1;

                        var statusElement = firefox.TryGetElementInElement(item, By.CssSelector("td:nth-child(4)"));
                        string status = statusElement?.GetAttribute("innerHTML");
                        if (status.Equals("Oglądam"))
                            anime.Status = AnimeStatus.Watching;
                        else if (status.Equals("Obejrzane"))
                            anime.Status = AnimeStatus.Completed;
                        else if (status.Equals("Wstrzymane"))
                            anime.Status = AnimeStatus.OnHold;
                        else if (status.Equals("Porzucone"))
                            anime.Status = AnimeStatus.Dropped;
                        else if (status.Equals("Planuję"))
                            anime.Status = AnimeStatus.PlanToWatch;

                        var progressElement = firefox.TryGetElementInElement(item, By.XPath(".//span[contains(@id, 'episodes_counter_title_')]"));
                        bool parsedProgress = int.TryParse(progressElement?.Text ?? "0", out int progress);
                        anime.Progress = parsedProgress ? progress : 0;
                        //Console.WriteLine($"Progress {progress}");

                        AnimeList.Add(anime);
                    }
                }
            }
            OnListDownloadCompleted();
        }
    }
}
