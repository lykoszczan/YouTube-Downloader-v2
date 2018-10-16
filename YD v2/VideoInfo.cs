using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YoutubeExplode;

namespace YD_v2
{
    class VideoInfo
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Duration { get; set; }
        public ImageSource Image { get; set; }

        public VideoInfo()
        {
        }
        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
        public async Task GetInfo(string url)
        {
            List<String> info = new List<string>();
            var client = new YoutubeClient();
            string id = YoutubeClient.ParseVideoId(url);
            var video = await client.GetVideoAsync(id);
            var str = video.Thumbnails.LowResUrl;
            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(str);
            Bitmap bitmap = new Bitmap(stream);
            Image = ImageSourceForBitmap(bitmap);
            stream.Flush();
            stream.Close();
            webClient.Dispose();
            Title = video.Title;
            Author = video.Author;
            Duration = video.Duration.ToString();

        }
    }

}
