using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace YD_v2
{
    public partial class Home : Window
    {
        private string playlistId;
        private Downloader downloaderClient;
        private string[] ViewItems;
        private System.Windows.Forms.NotifyIcon ni;
        private ContextMenuStrip playViewContextMenu;

        public Home()
        {
            InitializeComponent();
            CreateContextMenuOnListView();
            downloaderClient = new Downloader();
            AddItemToListView();
            UpdateLabels();
            GifImage.Visibility = Visibility.Hidden;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void UpdateLabels()
        {
            ItemOnList item = new ItemOnList();
            List<string> channelsList = new List<string>();
            bool canAddItemToListBox = false;
            bool playlistsUpToDate = true;
            if (PlaylistsListView.Items.Count > 0)
            {
                item = (ItemOnList)PlaylistsListView.Items[0];
                channelsList.Add(item.ChannelName);
                if (item.StatusColor == System.Windows.Media.Brushes.Red)
                {
                    playlistsUpToDate = false;
                }
            }
            for (int i = 1; i < PlaylistsListView.Items.Count; i++)
            {
                canAddItemToListBox = true;
                item = (ItemOnList)PlaylistsListView.Items[i];
                if (item.StatusColor == System.Windows.Media.Brushes.Red)
                {
                    playlistsUpToDate = false;
                }
                for (int j = 0; j < channelsList.Count; j++)
                {
                    if (item.ChannelName == channelsList[j])
                    {
                        canAddItemToListBox = false;
                    }
                }

                if (canAddItemToListBox)
                    channelsList.Add(item.ChannelName);
            }

            playlistCountTextBox.Text = PlaylistsListView.Items.Count.ToString();
            channeltext.Text = channelsList.Count.ToString();
            songsCountTextBox.Text = downloaderClient.SongsCount.ToString();

            if (PlaylistsListView.Items.Count == 0)
            {
                UpdateLabel.Content = "You don't follow any playlist";
                UpdateVisual.Fill = new VisualBrush() { Visual = (Visual)FindResource("NoPlaylist") };

            }
            else
            {
                if (playlistsUpToDate)
                {
                    UpdateLabel.Content = "All playlists are up-to-date";
                    UpdateVisual.Fill = new VisualBrush() { Visual = (Visual)FindResource("UpdatedSongs") };
                }
                else
                {
                    UpdateLabel.Content = "The playlists are not updated";
                    UpdateVisual.Fill = new VisualBrush() { Visual = (Visual)FindResource("NotUpdatedSongs") };
                }
            }
        }

        private void AddItemToListView()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(downloaderClient.XmlInfoPath);

            XmlNodeList nodeList = doc.SelectNodes("//Playlist");
            List<ItemOnList> items = new List<ItemOnList>();

            items.Clear();

            for (int i = 0; i < nodeList.Count; i++)
            {
                ItemOnList item = new ItemOnList
                {
                    Id = nodeList[i].Attributes["Id"].Value,
                    Name = nodeList[i].ChildNodes[0].InnerText,
                    SongsCount = nodeList[i].ChildNodes[1].InnerText,
                    ChannelName = nodeList[i].ChildNodes[2].InnerText,
                    LastUpdate = nodeList[i].ChildNodes[3].InnerText,
                    Path = nodeList[i].ChildNodes[4].InnerText
                };

                items.Add(item);

            }

            PlaylistsListView.ItemsSource = items;
        }

        private void UrlBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBox.Text == "Paste YouTube URL here")
                UrlBox.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
            }
        }

        private void UrlBoxPlaylist_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBoxPlaylist.Text == "Paste YouTube URL here")
                UrlBoxPlaylist.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
            }
        }

        private async Task ShowVideoInfoFromUrl(string url)
        {
            var bc = new BrushConverter();
            VideoPanel.Background = (Brush)bc.ConvertFrom("#3D3D3D");
            SongName.Text = "";
            Duration.Text = "";
            Author.Text = "";
            Thumbnail.Source = null;
            VideoInfo info = new VideoInfo();
            await info.GetInfo(UrlBox.Text);

            SongName.Text = info.Title;
            Duration.Text = info.Duration;
            Author.Text = info.Author;
            Thumbnail.Source = info.Image;
            if (info.Title != "" && GifImage != null)
            {
                VideoPanel.Background = (Brush)bc.ConvertFrom("#C8C8C8");
                GifImage.Visibility = Visibility.Hidden;
            }

        }
        private void UrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UrlBox.Text != "")
            {
                if (GifImage != null)
                    GifImage.Visibility = Visibility.Visible;
                ShowVideoInfoFromUrl(UrlBox.Text);
            }
            else
            {
                if (GifImage != null)
                    GifImage.Visibility = Visibility.Hidden;

                SongName.Text = "";
            }

        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            playlistId = downloaderClient.ParseplaylistId(UrlBoxPlaylist.Text);
            if (playlistId == "")
            {
                System.Windows.MessageBox.Show("Wrong URL");
            }
            else
            { //TODO : poprawa poniższego, tak aby jedna metoda była downloadera

                SelectSongsToDownload(playlistId);

                //tymczasowo komentuje do testów
                //downloaderClient.ParseVideosURLsAsync(playlistId);
                //downloaderClient.DownloadAllVideos();
                //AddItemToListView();
                //UpdateLabels();
            }
        }

        private async Task SelectSongsToDownload(string id)
        {
            await downloaderClient.ParseVideosURLsAsync(id);
            SelectSongs selectSongsDialog = new SelectSongs();
            selectSongsDialog.PlaylistID = id;
            selectSongsDialog.FillListView(downloaderClient.playlistSongs);
            selectSongsDialog.ShowDialog();
        }

        private void UrlBoxPlaylists_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBox.Text == "Paste YouTube URL here")
                UrlBox.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
                this.SearchBtn_Click(sender, eventArgs);
            }
        }

        private void PlayView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectSongs songs = new SelectSongs();
            ItemOnList item = (ItemOnList)PlaylistsListView.SelectedItem;
            Process.Start(item.Path);
        }

        private void CreateContextMenuOnListView()
        {
            playViewContextMenu = new ContextMenuStrip();
            ToolStripItem itemDelete = playViewContextMenu.Items.Add("Delete playlist");
            itemDelete.Click += new EventHandler(PlayviewmenuDelete_click);

            ToolStripItem itemUpdate = playViewContextMenu.Items.Add("Update");
            itemUpdate.Click += new EventHandler(PlayviewmenuUpdate_clickAsync);

            ToolStripItem itemCopyURL = playViewContextMenu.Items.Add("Copy URL");
            itemCopyURL.Click += new EventHandler(PlayviewmenuCopyURL_click);

        }

        private void PlayviewmenuCopyURL_click(object sender, EventArgs e)
        {
            ItemOnList onList = (ItemOnList)PlaylistsListView.SelectedItem;
            string text = "https://www.youtube.com/playlist?list=" + onList.Id;
            System.Windows.Clipboard.SetText(text);
        }

        private void SetUpdateStatus(string name, string channel)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(downloaderClient.XmlInfoPath);

            XmlNodeList nodeList = doc.SelectNodes("//Playlist");
            List<ItemOnList> items = new List<ItemOnList>();

            items.Clear();

            for (int i = 0; i < nodeList.Count; i++)
            {
                ItemOnList item = new ItemOnList
                {
                    Id = nodeList[i].Attributes["Id"].Value,
                    Name = nodeList[i].ChildNodes[0].InnerText,
                    ChannelName = nodeList[i].ChildNodes[2].InnerText,
                    SongsCount = nodeList[i].ChildNodes[1].InnerText
                };
                if (item.Name == name && item.ChannelName == channel)
                    item.LastUpdate = "Updating ...";
                else
                    item.LastUpdate = nodeList[i].ChildNodes[3].InnerText;

                item.Path = nodeList[i].ChildNodes[4].InnerText;
                items.Add(item);
            }

            PlaylistsListView.ItemsSource = items;
        }

        private async void PlayviewmenuUpdate_clickAsync(object sender, EventArgs e)
        {
            ItemOnList onList = (ItemOnList)PlaylistsListView.SelectedItem;
            SetUpdateStatus(onList.Name, onList.ChannelName);
            await downloaderClient.ParseVideosURLsAsync(onList.Id);
            downloaderClient.DownloadAllVideos(true);
            AddItemToListView();
            UpdateLabels();
        }

        private void PlayviewmenuDelete_click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(downloaderClient.Rootpath + "/info.xml");
            ItemOnList onList = (ItemOnList)PlaylistsListView.SelectedItem;
            XmlNode node = doc.SelectSingleNode("//Playlist[@Id='" + onList.Id + "']");
            node.ParentNode.RemoveChild(node);
            doc.Save(downloaderClient.Rootpath + "/info.xml");

            AddItemToListView();
            UpdateLabels();
        }

        private void PlayView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as System.Windows.Controls.ListView).SelectedItem;
            if (item != null)
            {
                playViewContextMenu.Show(System.Windows.Forms.Cursor.Position);
            }
        }

        private void UrlBoxPlaylist_TextChanged(object sender, TextChangedEventArgs e)
        {


        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SongName.Text != "")
            {
                bool onlyAudio = OnlyAudioCheckBox.IsChecked == true;
                bool SaveToDefaultPath = SaveToDefaultPathCheckBox.IsChecked == true;
                string DownloadPath = Properties.Settings.Default.DefaultPath;
                if (!SaveToDefaultPath)
                {
                    Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                    dlg.FileName = SongName.Text;

                    // Show save file dialog box
                    Nullable<bool> result = dlg.ShowDialog();

                    // Process save file dialog box results
                    if (result == true)
                    {
                        // Save document
                        string filename = dlg.FileName;
                        DownloadPath = dlg.FileName.Replace(SongName.Text, "");
                    }
                }
                downloaderClient.DownloadByYouTubeExplode(UrlBox.Text, DownloadPath, onlyAudio);
            }
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                InitialDirectory = System.Environment.CurrentDirectory,
                IsFolderPicker = true
            };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Properties.Settings.Default.DefaultPath = dialog.FileName;
                Properties.Settings.Default.Save();
            }
        }
    }
}
