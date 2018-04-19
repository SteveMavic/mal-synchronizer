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
    public class WebsiteModel
    {
        private string _userName;
        private List<Anime> _animeList = new List<Anime>();
        #region Properties
        public virtual List<Anime> AnimeList
        {
            get { return _animeList; }
            set { _animeList = value; }
        }
        public virtual string Url { get; set; }
        public bool IsListDownloaded { get; set; } = false;

        public virtual string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
            }
        }

        #endregion

        #region Events
        public event EventHandler ListDownloadCompleted;
        protected virtual void OnListDownloadCompleted()
        {
            IsListDownloaded = true;
            ListDownloadCompleted?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Methods
        public WebsiteModel()
        {

        }
        public WebsiteModel(string url)
        {
            Url = url;
        }
        public WebsiteModel(string url, string userName) : this(url)
        {
            UserName = userName;
        }
        public virtual void GetAnimeList(FirefoxController firefox) { }
        #endregion
    }
}
