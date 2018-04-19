using MAL_Synchronizer.Controllers;
using MAL_Synchronizer.Model;
using NHotkey;
using NHotkey.Wpf;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace MAL_Synchronizer.View
{
    /// <summary>
    /// Interaction logic for MergedListsWindow.xaml
    /// </summary>
    public partial class MergedListsWindow : Window
    {
        public FirefoxController Firefox { get; set; }
        public Anime CurrentAnime { get; set; } = null;

        public MergedListsWindow(FirefoxController firefox, ObservableCollection<Anime> collection)
        {
            InitializeComponent();
            Firefox = firefox;
            listBoxMerged.ItemsSource = collection;
            tbMergedListCount.Text = $"{collection.Count} items";
            HotkeyManager.Current.AddOrReplace("LoadInfo", Key.Space, ModifierKeys.Control, LoadInfo);
        }

        private void LoadInfo(object sender, HotkeyEventArgs e)
        {
            if (Firefox.Driver.Url.Equals("https://myanimelist.net/addtolist.php?hidenav=1") && CurrentAnime != null)
            {
                // select the drop down list
                var container = Firefox.TryGetElements(By.CssSelector(".borderClass.quickAdd-result-column"));
                if (container != null)
                {
                    IWebElement openedDetails = container.Where(n => 
                    {
                        string display = n.GetCssValue("display");
                        return !display.Equals("none");
                    }).FirstOrDefault();


                    if (openedDetails != null)
                    {
                        var status = Firefox.TryGetElementInElement(openedDetails, By.XPath("//select[contains(@id,'status')]"));
                        if (status != null)
                        {
                            var selectElement = new SelectElement(status);

                            string value = "0";
                            switch (CurrentAnime.Status)
                            {
                                case AnimeStatus.Watching:
                                    value = "1";
                                    break;
                                case AnimeStatus.Completed:
                                    value = "2";
                                    break;
                                case AnimeStatus.OnHold:
                                    value = "3";
                                    break;
                                case AnimeStatus.Dropped:
                                    value = "4";
                                    break;
                                case AnimeStatus.PlanToWatch:
                                    value = "6";
                                    break;
                                default:
                                    value = "0";
                                    break;
                            }
                            selectElement.SelectByValue(value);
                        }

                        var score = Firefox.TryGetElementInElement(openedDetails, By.XPath("//select[contains(@id,'score')]"));
                        if (score != null)
                        {
                            var selectElement = new SelectElement(score);
                            if (CurrentAnime.OverallScore != -1)
                                selectElement.SelectByValue(CurrentAnime.OverallScore.ToString());
                        }

                        if (CurrentAnime.Status != AnimeStatus.Completed)
                        {
                            var progress = Firefox.TryGetElementInElement(openedDetails, By.XPath("//input[contains(@id,'epsWatched')]"));
                            progress?.Clear();
                            progress?.SendKeys(CurrentAnime.Progress.ToString());
                        }
                    }
                }
            }
        }

        private void listBoxMerged_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Anime anime = (sender as ListBox).SelectedItem as Anime;
            Console.WriteLine(anime.Url);
            if (anime != null)
                if (Firefox != null && Firefox.IsDriverBuild())
                {
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            if (!Firefox.Driver.Url.Equals("https://myanimelist.net/addtolist.php?hidenav=1"))
                                Firefox.Driver.Navigate().GoToUrl("https://myanimelist.net/addtolist.php?hidenav=1");

                            var canSearch = Firefox.WaitUntilElementClickable(By.CssSelector("#maSearchText"));
                            if (canSearch != null)
                            {
                                canSearch.Clear();
                                canSearch.SendKeys(anime.Title);
                                Firefox.ExecuteScript("performSearch();");
                                CurrentAnime = anime;
                            }
                        }
                        catch { }
                    });
                }
        }
    }
}
