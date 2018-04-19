using MAL_Synchronizer.Comparers;
using MAL_Synchronizer.Controllers;
using MAL_Synchronizer.Model;
using MAL_Synchronizer.WebsiteModel;
using Newtonsoft.Json;
using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MAL_Synchronizer.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FirefoxController Firefox;
        MALModel mal = new MALModel();
        ShindenModel shinden = new ShindenModel();
        private string CredentialsFilePath { get; } = "credentials.json";

        public MainWindow()
        {
            InitializeComponent();
            Firefox = new FirefoxController();
            Firefox.DriverBuilt += Firefox_DriverBuilt;
            Firefox.BrowserClosed += Firefox_BrowserClosed;
            Firefox.BrowserPathError += Firefox_BrowserPathError;
            Closing += MainWindow_Closing;
            Loaded += MainWindow_Loaded;
            shinden.ListDownloadCompleted += Shinden_ListDownloadCompleted;
            mal.ListDownloadCompleted += Mal_ListDownloadCompleted;
            ChangeCredentialsWindow.ChangedCredentials += ChangeCredentials_ChangedCredentials;
        }

        private void Firefox_BrowserPathError(object sender, EventArgs e)
        {
            //Console.WriteLine("xD");
            Dispatcher.Invoke(() => { new SetBrowserLocationWindow().ShowDialog(); });
            //new SetBrowserLocationWindow().ShowDialog();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(File.Exists(CredentialsFilePath));
            if (!File.Exists(CredentialsFilePath))
            {
                var ccw = new ChangeCredentialsWindow();
                ccw.ShowDialog();
            }
            else
            {
                using (StreamReader file = File.OpenText(CredentialsFilePath))
                {
                    JsonTextReader jsonReader = new JsonTextReader(file);
                    object output = new JsonSerializer().Deserialize(jsonReader);
                    if (output != null)
                    {
                        CredentialsEventArgs cea = JsonConvert.DeserializeObject<CredentialsEventArgs>(output.ToString());
                        mal.UserName = cea.MyAnimeListName;
                        shinden.SetUserNameAndID(cea.ShindenName, cea.ShindenID);
                        //shinden.UserName = cea.ShindenName;
                        //shinden.UserID = cea.ShindenID;
                    }
                }
            }
        }

        private void ChangeCredentials_ChangedCredentials(object sender, CredentialsEventArgs e)
        {
            mal.UserName = e.MyAnimeListName;
            shinden.SetUserNameAndID(e.ShindenName, e.ShindenID);
            //mal = new MALModel(e.MyAnimeListName);
            //shinden = new ShindenModel(e.ShindenName, e.ShindenID);
            using (StreamWriter file = File.CreateText(CredentialsFilePath))
            {
                CredentialsEventArgs cea = new CredentialsEventArgs { MyAnimeListName = mal.UserName, ShindenID = shinden.UserID, ShindenName = shinden.UserName };

                new JsonSerializer().Serialize(file, cea);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Firefox?.Driver?.Quit();
        }

        private void Firefox_BrowserClosed(object sender, EventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() => 
                {
                    buttonOpenBrowser.IsEnabled = true;
                    buttonGetMalAnimeList.IsEnabled = false;
                    buttonGetShindenAnimeList.IsEnabled = false;
                    buttonMergeLists.IsEnabled = false;
                });
            }
            catch {}
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Factory.StartNew(() => Firefox.BuildDriver());
        }
        private void GetMAL_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(mal.UserName))
                return;
            Console.WriteLine(mal.Url);
            Firefox.Driver.Navigate().GoToUrl(mal.Url);
            if (Firefox != null && Firefox.IsDriverBuild())
                Task.Factory.StartNew(() => 
                {
                    Dispatcher.Invoke(() => {
                        buttonGetMalAnimeList.IsEnabled = false;
                        buttonGetShindenAnimeList.IsEnabled = false;
                        buttonMergeLists.IsEnabled = false;
                        tbMalListCount.Text = "Retrieving data";
                    });
                    mal.GetAnimeList(Firefox);
                });
        }

        private void GetShinden_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(shinden.UserName) || string.IsNullOrEmpty(shinden.UserID))
                return;
            Console.WriteLine(shinden.Url);
            Firefox.Driver.Navigate().GoToUrl(shinden.Url);
            if (Firefox != null && Firefox.IsDriverBuild())
                Task.Factory.StartNew(() => 
                {
                    Dispatcher.Invoke(() => {
                        buttonGetMalAnimeList.IsEnabled = false;
                        buttonGetShindenAnimeList.IsEnabled = false;
                        buttonMergeLists.IsEnabled = false;
                        tbShindenListCount.Text = "Retrieving data";
                    });
                    shinden.GetAnimeList(Firefox);
                });
        }

        private void MergeLists_Click(object sender, RoutedEventArgs e)
        {
            if (mal?.IsListDownloaded == true && shinden?.IsListDownloaded == true)
            {
                var mergedList = shinden.AnimeList.Where(k => !mal.AnimeList.Contains(k, new AnimeComparer())).ToList();

                new MergedListsWindow(Firefox, new ObservableCollection<Anime>(mergedList)).ShowDialog();
            }
        }
        private void Shinden_ListDownloadCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("ListDownloadCompleted");
            var collection = new ObservableCollection<Anime>(shinden.AnimeList);
            Dispatcher.Invoke(() => 
            {
                listBoxShindenList.ItemsSource = collection;
                buttonGetMalAnimeList.IsEnabled = true;
                buttonGetShindenAnimeList.IsEnabled = true;
                if (mal?.IsListDownloaded == true && shinden?.IsListDownloaded == true)
                    buttonMergeLists.IsEnabled = true;
                tbShindenListCount.Text = $"{collection.Count} items";
            });
        }

        private void Firefox_DriverBuilt(object sender, EventArgs e)
        {
            Console.WriteLine("DriverBuilt");
            Dispatcher.Invoke(() => 
            {
                buttonOpenBrowser.IsEnabled = false;
                buttonGetMalAnimeList.IsEnabled = true;
                buttonGetShindenAnimeList.IsEnabled = true;
            } );
        }

        private void Mal_ListDownloadCompleted(object sender, EventArgs e)
        {
            Console.WriteLine("ListDownloadCompleted");
            var collection = new ObservableCollection<Anime>(mal.AnimeList);
            Dispatcher.Invoke(() => 
            {
                listBoxMalList.ItemsSource = collection;
                buttonGetMalAnimeList.IsEnabled = true;
                buttonGetShindenAnimeList.IsEnabled = true;
                if (mal?.IsListDownloaded == true && shinden?.IsListDownloaded == true)
                    buttonMergeLists.IsEnabled = true;
                tbMalListCount.Text = $"{collection.Count} items";
            });
            
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
                        if (!Firefox.Driver.Url.Equals("https://myanimelist.net/addtolist.php?hidenav=1"))
                            Firefox.Driver.Navigate().GoToUrl("https://myanimelist.net/addtolist.php?hidenav=1");

                        var canSearch = Firefox.WaitUntilElementClickable(By.CssSelector("#maSearchText"));
                        if (canSearch != null)
                        {
                            canSearch.Clear();
                            canSearch.SendKeys(anime.Title);
                            Firefox.ExecuteScript("performSearch();");
                        }
                    });
                }
        }

        private void listBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Anime anime = (Anime)((ListBox)sender).SelectedItem;
            Console.WriteLine(anime.Url);
            if (anime != null && !string.IsNullOrEmpty(anime.Url))
                if (Firefox != null && Firefox.IsDriverBuild())
                    Task.Factory.StartNew(() => Firefox.Driver.Navigate().GoToUrl(anime.Url));
        }

        private void MenuItemChangeNames_Click(object sender, RoutedEventArgs e)
        {
            new ChangeCredentialsWindow().ShowDialog();
        }
        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MenuItemChangeBrowserPath_Click(object sender, RoutedEventArgs e)
        {
            new SetBrowserLocationWindow().ShowDialog();
        }
    }
}
