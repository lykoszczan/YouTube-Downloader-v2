using System;
using System.Collections.Generic;
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

namespace YD_v2
{
    public class SelectedSong
    {
        public string VideoTitle { get; set; }
        public string VideoId { get; set; }

        public SelectedSong()
        {

        }
    }

    public partial class SelectSongs : Window
    {
        public string PlaylistID { get; set; }
        public string PlaylistTitle { get; set; }


        public SelectSongs()
        {
            InitializeComponent();
        }

        public void FillListView(List<string> videoIdWithTitleList)
        {
            List<SelectedSong> selectedSongsList = new List<SelectedSong>();
            string[] temp;
            for(int i=0; i<videoIdWithTitleList.Count; i++)
            {
                temp = videoIdWithTitleList[i].Split(';');

                SelectedSong song = new SelectedSong();
                song.VideoTitle = temp[0];
                song.VideoId = temp[1];

                selectedSongsList.Add(song);
            }
            PlayView.ItemsSource = selectedSongsList;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {



            DownloadPlaylist();


        }
        
        private List<string> GetAllCheckedVideos()
        {
            List<string> idList = new List<string>();
            foreach(SelectedSong item in PlayView.SelectedItems)
            {
                idList.Add(item.VideoId);
            }
            return idList;
        }

        private async Task DownloadPlaylist()
        {
            Downloader downloaderClient = new Downloader();
            bool onlyAudio = OnlyAudioCheckBox.IsChecked == true;
            bool SaveToDefaultPath = SaveToDefaultPathCheckBox.IsChecked == true;
            string DownloadPath = Properties.Settings.Default.DefaultPath;
            if (!SaveToDefaultPath)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                // Process save file dialog box results
                if (result == true)
                {
                    // Save document
                    string filename = dlg.FileName;
                }
            }
            List<string> tempList = new List<string>();
            tempList = GetAllCheckedVideos();
            //await downloaderClient.ParseVideosURLsAsync(PlaylistID);
            downloaderClient.DownloadAllVideos(onlyAudio, tempList);
        }
    }
}
