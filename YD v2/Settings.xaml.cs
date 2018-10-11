using Microsoft.WindowsAPICodePack.Dialogs;
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
using System.Xml;

namespace YD_v2
{
    /// <summary>
    /// Logika interakcji dla klasy Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveSettings()
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                if(File.Exists(Path.Text))
                  xml.Load(Path.Text);
                else
                {
                    XmlNode docNode = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xml.AppendChild(docNode);

                    XmlNode playlistsNode = xml.CreateElement("Playlists");
                    //XmlNode settingsNode = xml.CreateElement("Settings");
                    //xml.AppendChild(settingsNode);
                    xml.AppendChild(playlistsNode);

                    xml.Save(@"C:/info.xml");
                    XmlNode settingsNode = xml.CreateElement("Settings");

                }
            }
            catch
            {

            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "My Title";
            dlg.IsFolderPicker = true;
            dlg.InitialDirectory = Directory.GetCurrentDirectory();

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.DefaultDirectory = Directory.GetCurrentDirectory();
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Path.Text = dlg.FileName;
                const string REGISTRY_KEY = @"HKEY_CURRENT_USER\YouTubeDownloader";
                const string REGISTY_VALUE = "YouTubeDownloader";
                SaveSettings();
                if (Convert.ToInt32(Microsoft.Win32.Registry.GetValue(REGISTRY_KEY, REGISTY_VALUE, 0)) == 0)
                {
                    Microsoft.Win32.Registry.SetValue(REGISTRY_KEY, REGISTY_VALUE, Path.Text, Microsoft.Win32.RegistryValueKind.String);
                }
            }

            this.Activate();
        }
    }
}
