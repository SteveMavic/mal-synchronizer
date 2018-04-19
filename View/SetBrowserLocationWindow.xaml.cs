using MAL_Synchronizer.Model;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MAL_Synchronizer.View
{
    /// <summary>
    /// Interaction logic for SetBrowserLocationWindow.xaml
    /// </summary>
    public partial class SetBrowserLocationWindow : Window
    {
        private string browserPath { get; } = "browserpath.json";

        public SetBrowserLocationWindow()
        {
            InitializeComponent();
            if (File.Exists(browserPath))
                using (StreamReader file = File.OpenText(browserPath))
                {
                    JsonTextReader jsonReader = new JsonTextReader(file);
                    var path = new JsonSerializer().Deserialize<BrowserPath>(jsonReader);
                    tbFirefoxpath.Text = path.Path;
                }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string path = tbFirefoxpath.Text.Trim();
            if (!string.IsNullOrEmpty(path))
            {
                var x = new BrowserPath { Path = path };
                using (StreamWriter file = File.CreateText(browserPath))
                {
                    new JsonSerializer().Serialize(file, x);
                }
            }
            Close();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            fileDialog.Filter = "Exe File(*.exe)|*.exe";

            bool? result = fileDialog.ShowDialog();

            if (result == true)
            {
                string filename = fileDialog.FileName;
                tbFirefoxpath.Text = filename;
            }
        }
    }
}
