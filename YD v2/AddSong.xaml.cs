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

namespace YD_v2
{
    /// <summary>
    /// Logika interakcji dla klasy AddSong.xaml
    /// </summary>
    public partial class AddSong : Window
    {
        private Downloader dwn;
        public AddSong()
        {
            InitializeComponent();
            dwn = new Downloader();
            GifImage.Visibility = Visibility.Hidden;

        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Download_Click(object sender, RoutedEventArgs e)
        {
            string path = "c:/YDv2/Other Downloads";
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            dwn.DownloadOneSongAsync(UrlBox.Text,path);   
        }

        private void UrlBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (UrlBox.Text == "Paste YouTube URL here")
                UrlBox.Text = "";
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RoutedEventArgs eventArgs = new RoutedEventArgs();
                Download_Click(sender, eventArgs);
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
            if (info.Title!="" && GifImage != null)
            {
                VideoPanel.Background = (Brush)bc.ConvertFrom("#C8C8C8");
                GifImage.Visibility = Visibility.Hidden;
            }

        }
        private void UrlBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UrlBox.Text != "")
            {
                if(GifImage != null)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            dwn.DownloadOneVideoAsync(UrlBox.Text, "c:/YDv2/ASP-Kurs");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
             dwn.DownloadAudio(UrlBox.Text);
            //dwn.DownloadVideo(UrlBox.Text);
        }
    }
}
