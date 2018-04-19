using MAL_Synchronizer.Model;
using MAL_Synchronizer.Properties;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MAL_Synchronizer.Controllers
{
    public class ErrorEventArgs : EventArgs
    {
        public byte Type { get; set; }
        public string Message { get; set; }
    }

    public class FirefoxController
    {
        Thread browserChecker;
        private string browserPath { get; } = "browserpath.json";

        #region Public Events
        /// <summary>
        /// Fires when driver connects with browser
        /// </summary>
        public event EventHandler DriverBuilt;
        protected virtual void OnDriverBuilt()
        {
            DriverBuilt?.Invoke(this, null);
            DriverBuild = true;
            browserChecker = new Thread(CheckBrowserHandle);
            browserChecker.Start();
        }
        /// <summary>
        /// Fires when driver couldn't connect to browser or error while connecting have been found
        /// </summary>
        public event EventHandler<ErrorEventArgs> DriverFoundError;
        protected virtual void OnDriverFoundError(byte type, string message)
        {
            DriverFoundError?.Invoke(this, new ErrorEventArgs() { Type = type, Message = message });
        }
        public event EventHandler BrowserPathError;
        protected virtual void OnBrowserPathError()
        {
            BrowserPathError?.Invoke(this, EventArgs.Empty);
        }
        /// <summary>
        /// Fires when browser have been closed
        /// </summary>
        public event EventHandler BrowserClosed;
        protected virtual void OnBrowserClosed()
        {
            DriverBuild = false;
            BrowserClosed?.Invoke(this, null);
            try
            {
                Driver.Quit();
            }
            catch { }
        }
        #endregion

        public FirefoxDriver Driver { get; set; }

        private bool DriverBuild { get; set; } = false;

        public bool IsDriverBuild() => DriverBuild;

        public void BuildDriver()
        {
            try
            {
                BrowserPath path = new BrowserPath { Path = "" };
                if (File.Exists(browserPath))
                    using (StreamReader file = File.OpenText(browserPath))
                    {
                        JsonTextReader jsonReader = new JsonTextReader(file);
                        path = new JsonSerializer().Deserialize<BrowserPath>(jsonReader);
                    }

                FirefoxDriverService ffService = FirefoxDriverService.CreateDefaultService("Driver"); // driver path
                ffService.FirefoxBinaryPath = path.Path;
                ffService.HideCommandPromptWindow = true;

                FirefoxOptions ffOpt = new FirefoxOptions();

                this.Driver = new FirefoxDriver(ffService, ffOpt, TimeSpan.FromSeconds(60.0));

                OnDriverBuilt();
            }
            catch (WebDriverException e) when (e.Message.Contains("Expected browser binary location"))
            {
                OnBrowserPathError();
                Console.WriteLine(e.ToString());
            }
            catch (InvalidOperationException e)
            {
                OnBrowserPathError();
                Console.WriteLine(e.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private void CheckBrowserHandle()
        {
            while (true)
            {
                if (DriverBuild)
                {
                    try
                    {
                        if (Driver.CurrentWindowHandle == null)
                            throw new Exception();
                    }
                    catch (Exception)
                    {
                        OnBrowserClosed();
                        break;
                    }

                    Thread.Sleep(500);
                }
                else
                    break;
            }
        }

        /// <summary>
        /// Tries to get specific element in web page
        /// </summary>
        /// <param name="By">Indicates, by which element's parameter, element should be searched</param>
        /// <returns>IWebElement if found, otherwise null</returns>
        public IWebElement TryGetElement(By By)
        {
            try
            {
                IWebElement element = Driver.FindElement(By);
                return element;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Inny typ błędu w TGE: " + e.GetType().ToString());
                return null;
            }
        }

        /// <summary>
        /// Tries to get specific elements in web page
        /// </summary>
        /// <param name="By">Indicates, by which element's parameter, elements should be searched</param>
        /// <returns>IWebElement[] if found, otherwise null</returns>
        public IWebElement[] TryGetElements(By By)
        {
            try
            {
                ReadOnlyCollection<IWebElement> elements = Driver.FindElements(By);
                Console.WriteLine(elements.ToString());
                IWebElement[] elementArray = new IWebElement[elements.Count];
                elements.CopyTo(elementArray, 0);
                return elementArray;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Inny typ błędu w TGE: " + e.GetType().ToString());
                return null;
            }
        }

        /// <summary>
        /// Tries to get specific element inside of Parent Element
        /// </summary>
        /// <param name="ParentElement">Element in which element should be searched for</param>
        /// <param name="By">Indicates, by which element's parameter, elements should be searched</param></param>
        /// <returns>IWebElement of Parent Element if found, otherwise null</returns>
        public IWebElement TryGetElementInElement(IWebElement ParentElement, By By)
        {
            try
            {
                IWebElement element = ParentElement.FindElement(By);
                return element;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Inny typ błędu w TGE: " + e.GetType().ToString());
                return null;
            }
        }

        /// <summary>
        /// Tries to get specific elements inside of Parent Element
        /// </summary>
        /// <param name="ParentElement">Element in which element should be searched for</param>
        /// <param name="By">Indicates, by which element's parameter, elements should be searched</param></param>
        /// <returns>IWebElement[] of Parent Element if found, otherwise null</returns>
        public IWebElement[] TryGetElementsInElement(IWebElement ParentElement, By By)
        {
            try
            {
                ReadOnlyCollection<IWebElement> elements = ParentElement.FindElements(By);
                Console.WriteLine(elements.ToString());
                IWebElement[] elementArray = new IWebElement[elements.Count];
                elements.CopyTo(elementArray, 0);
                return elementArray;
            }
            catch (NoSuchElementException e)
            {
                Console.WriteLine(e.ToString());
                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Inny typ błędu w TGE: " + e.GetType().ToString());
                return null;
            }
        }

        /// <summary>
        /// Wait until specific element in web page exist, is visible and clickable
        /// </summary>
        /// <param name="elementLocator">Indicates, by which element's parameter, elements should be searched </param>
        /// <param name="timeout">Time in seconds how long to wait for element visibility, default - 10s</param>
        /// <returns>IWebElement if after a specified time elapsed element exists, otherwise null</returns>
        public IWebElement WaitUntilElementClickable(By elementLocator, int timeout = 10)
        {
            try
            {
                var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeout));
                return wait.Until(ExpectedConditions.ElementToBeClickable(elementLocator));
                
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Element with locator: '" + elementLocator + "' was not found in current context page.");
                return null;
            }
        }

        /// <summary>
        /// Indicates if element is stale (if element in NOT attached to DOM)
        /// </summary>
        /// <param name="element">Which element should be checked</param>
        /// <returns>true if element is NOT attacked to DOM, false if element IS attached to DOM</returns>
        public bool IsElementStale(IWebElement element)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(10));
            bool elementIsStillAttachedToDOM = false;
            try
            {
                return elementIsStillAttachedToDOM = wait.Until(ExpectedConditions.StalenessOf(element));
            }
            catch (Exception)
            {
                return true;
            }
        }

        /// <summary>
        /// Executes JavaScript script
        /// </summary>
        /// <param name="function">Function body or name whose should be executed</param>
        /// <returns>true if script have been executed, false if error occurs</returns>
        public bool ExecuteScript(string function)
        {
            IJavaScriptExecutor js = this.Driver;
            try
            {
                js.ExecuteScript(function);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
