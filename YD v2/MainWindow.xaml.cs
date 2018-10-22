using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using Windows.Data;
using Windows.UI;
using Windows.UI.Notifications;

namespace YD_v2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const String APP_ID = "Microsoft.Samples.DesktopToastsSample";
        private string playlistId;
        private Downloader dwn;
        private string[] ViewItems;
        private System.Windows.Forms.NotifyIcon ni;
        private ContextMenuStrip playviewmenu;

        public MainWindow()
        {
            InitializeComponent();
            CreateContextMenuOnListView();
            dwn = new Downloader();
            //Downloader.SetStartup();
            AddItemToListView();
            UpdateLabels();
            Tray();
        }
        private void Tray()
        {
            ContextMenuStrip cm = new ContextMenuStrip();

            ToolStripItem itemShowFolder = cm.Items.Add("Show app folder");
            itemShowFolder.Click += new EventHandler(ItemShowFolder_click);
            ToolStripItem itemClose = cm.Items.Add("Close");
            itemClose.Click += new EventHandler(Item_click);
    
            ni = new System.Windows.Forms.NotifyIcon
            {
                
                Icon = new System.Drawing.Icon("Main.ico"),
                Visible = false
                
            };

            ni.ContextMenuStrip = cm;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                    ni.Visible = false;
                };
        }

        private void ItemShowFolder_click(object sender, EventArgs e)
        {
            Process.Start(Properties.Settings.Default.DefaultPath);
        }

        private void Item_click(object sender, EventArgs e)
        {
            ToolStripItem clickedItem = sender as ToolStripItem;
            System.Windows.Application.Current.Shutdown();

        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                ni.Visible = true;
                this.Hide();
            }
               
            
            base.OnStateChanged(e);
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
            for (int i=1; i<PlayView.Items.Count; i++)
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

            for(int i = 0; i < nodeList.Count; i ++)
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
        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private Boolean IsConfigured()
        {
            bool candwn = false;
            const string REGISTRY_KEY = @"HKEY_CURRENT_USER\YouTubeDownloader";
            const string REGISTY_VALUE = "FirstRun";
            if (Convert.ToInt32(Microsoft.Win32.Registry.GetValue(REGISTRY_KEY, REGISTY_VALUE, 0)) == 0)
            {
                candwn = false;
                //Change the value since the program has run once now
                //Microsoft.Win32.Registry.SetValue(REGISTRY_KEY, REGISTY_VALUE, 1, Microsoft.Win32.RegistryValueKind.DWord);
                System.Windows.MessageBox.Show("Before downloading music, set the default path for your files.");
            }
            else
            {
                candwn = true;
            }

            return candwn;
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            if (IsConfigured())
            {
                playlistId = dwn.ParseplaylistId(UrlBox.Text);
                if (playlistId == "")
                {
                    System.Windows.MessageBox.Show("Wrong URL");
                }
                else
                {
                    dwn.ParseVideosURLsAsync(playlistId);
                    dwn.DownloadAllVideos(true);
                    AddItemToListView();
                    UpdateLabels();
                }
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void UrlBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
            ItemOnList item = (ItemOnList) PlayView.SelectedItem;
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
            dwn.DownloadAllVideos(true);
            AddItemToListView();
            UpdateLabels();
        }

        private void PlayviewmenuDelete_click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(dwn.Rootpath + "/info.xml");
            ItemOnList onList = (ItemOnList)PlayView.SelectedItem;
            XmlNode node = doc.SelectSingleNode("//Playlist[@Id='" + onList.Id + "']");
            node.ParentNode.RemoveChild(node);
            doc.Save(dwn.Rootpath + "/info.xml");

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

        private void UrlBox_LostFocus(object sender, RoutedEventArgs e)
        {
            //UrlBox.Text = "Paste YouTube URL here";
        }

        private void AddBtb_click(object sender, RoutedEventArgs e)
        {
            //AddSong ads = new AddSong();
            //ads.ShowDialog();

        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            ItemOnList item = new ItemOnList();
            for(int i = 0; i < PlayView.Items.Count; i++)
            {
                item = (ItemOnList)PlayView.Items[i];

                dwn.RefreshListAsync(item.Id, item.Path);
            }
            AddItemToListView();
            UpdateLabels();
        }
    }
}
