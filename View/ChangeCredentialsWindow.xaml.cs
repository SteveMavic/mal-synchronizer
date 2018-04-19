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
    public class CredentialsEventArgs : EventArgs
    {
        public string MyAnimeListName { get; set; }
        public string ShindenName { get; set; }
        public string ShindenID { get; set; }
    }
    /// <summary>
    /// Interaction logic for ChangeCredentials.xaml
    /// </summary>
    public partial class ChangeCredentialsWindow : Window
    {
        public static event EventHandler<CredentialsEventArgs> ChangedCredentials;
        protected virtual void OnChangedCredentials(string malName, string shindenName, string shindenID)
        {
            ChangedCredentials?.Invoke(this, new CredentialsEventArgs { MyAnimeListName = malName, ShindenID = shindenID, ShindenName = shindenName });
        }

        private string CredentialsFilePath { get; } = "credentials.json";

        public ChangeCredentialsWindow()
        {
            InitializeComponent();

            if (File.Exists(CredentialsFilePath))
                using (StreamReader file = File.OpenText(CredentialsFilePath))
                {
                    JsonTextReader jsonReader = new JsonTextReader(file);
                    object output = new JsonSerializer().Deserialize(jsonReader);
                    if (output != null)
                    {
                        CredentialsEventArgs cea = JsonConvert.DeserializeObject<CredentialsEventArgs>(output.ToString());
                        malName.Text = cea.MyAnimeListName;
                        shindenName.Text = cea.ShindenName;
                        shindenID.Text = cea.ShindenID;
                    }
                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string myAnimeListName = malName.Text.Trim();
            string shName = shindenName.Text.Trim();
            string shID = shindenID.Text.Trim();
            if (!string.IsNullOrEmpty(myAnimeListName) && !string.IsNullOrEmpty(shName) && !string.IsNullOrEmpty(shID))
            {
                OnChangedCredentials(myAnimeListName, shName, shID);
                Close();
            }
        }
    }
}
