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
    /// <summary>
    /// Logika interakcji dla klasy Home.xaml
    /// </summary>
    public partial class Home : Window
    {
        private const String APP_ID = "Microsoft.Samples.DesktopToastsSample";
        private string playlistId;
        private Downloader dwn;
        private string[] ViewItems;
        private System.Windows.Forms.NotifyIcon ni;
        private ContextMenuStrip playviewmenu;

        public Home()
        {
            InitializeComponent();
            CreateContextMenuOnListView();
            dwn = new Downloader();
            Downloader.SetStartup();
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
            ViewItems = dwn.Directories();
            pltext.Text = (ViewItems.Length - 1).ToString();
            sngtext.Text = dwn.SongsCount.ToString();
            ItemOnList item = new ItemOnList();
            List<string> chlist = new List<string>();
            bool canadd = false;
            bool updated = true;
            if (PlayView.Items.Count > 0)
            {
                item = (ItemOnList)PlayView.Items[0];
                chlist.Add(item.ChannelName);
                if (item.StatusColor == System.Windows.Media.Brushes.Red)
                {
                    updated = false;
                }
            }
            for (int i = 1; i < PlayView.Items.Count; i++)
            {
                canadd = true;
                item = (ItemOnList)PlayView.Items[i];
                if (item.StatusColor == System.Windows.Media.Brushes.Red)
                {
                    updated = false;
                }
                for (int j = 0; j < chlist.Count; j++)
                {
                    if (item.ChannelName == chlist[j])
                    {
                        canadd = false;
                    }
                }

                if (canadd)
                    chlist.Add(item.ChannelName);
            }
            channeltext.Text = chlist.Count.ToString();
            if (updated)
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
        private void AddItemToListView()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("c:/YDv2/info.xml");

            XmlNodeList nodeList = doc.SelectNodes("//Playlist");
            List<ItemOnList> items = new List<ItemOnList>();

            items.Clear();

            for (int i = 0; i < nodeList.Count; i++)
            {
                ItemOnList item = new ItemOnList();
                item.Id = nodeList[i].Attributes["Id"].Value;
                item.Name = nodeList[i].ChildNodes[0].InnerText;
                item.SongsCount = nodeList[i].ChildNodes[1].InnerText;
                item.ChannelName = nodeList[i].ChildNodes[2].InnerText;
                item.LastUpdate = nodeList[i].ChildNodes[3].InnerText;
                item.Path = nodeList[i].ChildNodes[4].InnerText;

                items.Add(item);

            }

            PlayView.ItemsSource = items;
        }

        private void UrlBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBox.Text == "Paste YouTube URL here")
                UrlBox.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
                //Download_Click(sender, eventArgs);
            }
        }
        private void UrlBoxPlaylist_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBoxPlaylist.Text == "Paste YouTube URL here")
                UrlBoxPlaylist.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
                //Download_Click(sender, eventArgs);
            }
        }

        private async Task SetInfo(string url)
        {
            var bc = new BrushConverter();
            VideoPanel.Background = (Brush)bc.ConvertFrom("#3D3D3D");
            SongName.Text = "";
            Duratn.Text = "";
            Author.Text = "";
            Thumbnail.Source = null;
            VideoInfo info = new VideoInfo();
            await info.GetInfo(UrlBox.Text);

            SongName.Text = info.Title;
            Duratn.Text = info.Duration;
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
                SetInfo(UrlBox.Text);
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
                playlistId = dwn.ParseplaylistId(UrlBox.Text);
                if (playlistId == "")
                {
                    System.Windows.MessageBox.Show("Wrong URL");
                }
                else
                {
                    dwn.ParseVideosURLsAsync(playlistId);
                    dwn.DownloadAllVideos();
                    AddItemToListView();
                    UpdateLabels();
                }
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
            ItemOnList item = (ItemOnList)PlayView.SelectedItem;
            Process.Start(item.Path);
        }

        private void CreateContextMenuOnListView()
        {
            playviewmenu = new ContextMenuStrip();
            ToolStripItem itemDelete = playviewmenu.Items.Add("Delete playlist");
            itemDelete.Click += new EventHandler(PlayviewmenuDelete_click);

            ToolStripItem itemUpdate = playviewmenu.Items.Add("Update");
            itemUpdate.Click += new EventHandler(PlayviewmenuUpdate_clickAsync);

            ToolStripItem itemCopyURL = playviewmenu.Items.Add("Copy URL");
            itemCopyURL.Click += new EventHandler(PlayviewmenuCopyURL_click);

        }

        private void PlayviewmenuCopyURL_click(object sender, EventArgs e)
        {
            ItemOnList onList = (ItemOnList)PlayView.SelectedItem;
            string text = "https://www.youtube.com/playlist?list=" + onList.Id;
            System.Windows.Clipboard.SetText(text);
        }
        private void SetUpdateStatus(string name, string channel)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("c:/YDv2/info.xml");

            XmlNodeList nodeList = doc.SelectNodes("//Playlist");
            List<ItemOnList> items = new List<ItemOnList>();

            items.Clear();

            for (int i = 0; i < nodeList.Count; i++)
            {
                ItemOnList item = new ItemOnList();
                item.Id = nodeList[i].Attributes["Id"].Value;
                item.Name = nodeList[i].ChildNodes[0].InnerText;
                item.ChannelName = nodeList[i].ChildNodes[2].InnerText;
                item.SongsCount = nodeList[i].ChildNodes[1].InnerText;
                if (item.Name == name && item.ChannelName == channel)
                    item.LastUpdate = "Updating ...";
                else
                    item.LastUpdate = nodeList[i].ChildNodes[3].InnerText;
                item.Path = nodeList[i].ChildNodes[4].InnerText;

                items.Add(item);

            }

            PlayView.ItemsSource = items;
        }

        private async void PlayviewmenuUpdate_clickAsync(object sender, EventArgs e)
        {
            ItemOnList onList = (ItemOnList)PlayView.SelectedItem;
            SetUpdateStatus(onList.Name, onList.ChannelName);
            //            onList.StatusColor = System.Windows.Media.Brushes.Silver;
            await dwn.ParseVideosURLsAsync(onList.Id);
            dwn.DownloadAllVideos();
            AddItemToListView();
            UpdateLabels();
        }

        private void PlayviewmenuDelete_click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dwn.rootpath + "/info.xml");
            ItemOnList onList = (ItemOnList)PlayView.SelectedItem;
            XmlNode node = doc.SelectSingleNode("//Playlist[@Id='" + onList.Id + "']");
            node.ParentNode.RemoveChild(node);
            doc.Save(dwn.rootpath + "/info.xml");

            AddItemToListView();
            UpdateLabels();
        }

        private void PlayView_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as System.Windows.Controls.ListView).SelectedItem;
            if (item != null)
            {
                playviewmenu.Show(System.Windows.Forms.Cursor.Position);
            }
        }

        private void UrlBoxPlaylist_TextChanged(object sender, TextChangedEventArgs e)
        {


        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            bool onlyAudio = OnlyAudioCheckBox.IsChecked == true;
            dwn.DownloadByYouTubeExplode(UrlBox.Text, "c:/YDv2/Other Downloads", onlyAudio);
        }

        private void StackPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void TabHeader_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Cursor != System.Windows.Input.Cursors.Wait)
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Hand;
        }

        private void TabHeader_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (this.Cursor != System.Windows.Input.Cursors.Wait)
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Arrow;
        }
    }
}
