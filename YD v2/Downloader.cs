using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using VideoLibrary;
using Microsoft.Win32;
using System.Reflection;
using YoutubeExplode;
using YoutubeExplode.Models.MediaStreams;
using ToastNotifications;
using ToastNotifications.Position;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using System.Drawing;
using System.Net;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using YoutubeExtractor;

namespace YD_v2
{


    class ItemOnList
    {
        private const string rootpath = @"c:/YDv2";
        public string Name { get; set; }
        public string SongsCount { get; set; }
        public string LastUpdate { get; set; }
        public string Id { get; set; }
        public string Path { get; set; }
        public string ChannelName { get; set; }
        public System.Windows.Media.Brush StatusColor
        {
            get
            {
                if (LastUpdate == "Updating ...")
                    return System.Windows.Media.Brushes.Orange;
                else
                {
                    DateTime last = Convert.ToDateTime(LastUpdate);

                    if (DateTime.Compare(DateTime.Now.Date, last.Date) == 0)
                        return System.Windows.Media.Brushes.Green;
                    else
                        return System.Windows.Media.Brushes.Red;

                }
            }

            set
            {
                StatusColor = value;
            }
        }

        public ItemOnList()
        {
            
        }
    }
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

    class Downloader
    { 
        public string rootpath;


        public string PlaylistId { get; private set; }
        public string PlaylistName { get; private set; }
        public string Channelname { get; private set; }
        public int Itemscount { get; private set; }
        public int SongsCount { get; private set; }

        public List<string> playlistURL;


        public Downloader()
        {
            playlistURL = new List<string>();
            rootpath =  @"c:/YDv2";
            CreateInfoXML();
            CreateRootDirectory();
        }
        public static void SetStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }
        public void CreateInfoXML()
        {
            if (!File.Exists(rootpath + "/info.xml"))
            {
                XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                XmlNode playlistsNode = doc.CreateElement("Playlists");
                doc.AppendChild(playlistsNode);

                doc.Save(rootpath + "/info.xml");
            }
        }
        private List<string> SongsToDownload(List<string> songs, string path)
        {
            List<string> list = new List<string>();
            XmlDocument xml = new XmlDocument();
            XmlNode songsNode;
            XmlNode sng;
            XmlAttribute sngurl;
            if (!File.Exists(path + "/songs.xml"))
            {
                XmlNode docNode = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
                xml.AppendChild(docNode);

                songsNode = xml.CreateElement("Songs");
                xml.AppendChild(songsNode);

                xml.Save(path + "/songs.xml");
            }

            xml.Load(path + "/songs.xml");
            for(int i = 0; i < songs.Count; i++)
            {
                sng = xml.SelectSingleNode("//Song[@Url='" + songs[i] + "']");
                if(sng==null)
                {
                    list.Add(songs[i]);
                    songsNode = xml.SelectSingleNode("//Songs");
                    sng = xml.CreateElement("Song");
                    sngurl = xml.CreateAttribute("Url");
                    sngurl.Value = songs[i];
                    sng.Attributes.Append(sngurl);
                    songsNode.AppendChild(sng);
                }
            }
            xml.Save(path + "/songs.xml");
            return list;
        }
        private void AddNode(Google.Apis.YouTube.v3.Data.Playlist item)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(rootpath + "/info.xml");

            XmlNode playlistnode;
            XmlAttribute playlistid;

            if (doc.SelectSingleNode("//Playlist[@Id='" + item.Id + "']") == null)
            {
                playlistnode = doc.CreateElement("Playlist");
                playlistid = doc.CreateAttribute("Id");
                playlistid.Value = item.Id;
                playlistnode.Attributes.Append(playlistid);

                XmlNode node = doc.CreateElement("Name");
                node.AppendChild(doc.CreateTextNode(item.Snippet.Title));
                playlistnode.AppendChild(node);

                node = doc.CreateElement("SongsCount");
                node.AppendChild(doc.CreateTextNode(item.ContentDetails.ItemCount.ToString()));
                playlistnode.AppendChild(node);

                node = doc.CreateElement("ChannelName");
                node.AppendChild(doc.CreateTextNode(item.Snippet.ChannelTitle));
                playlistnode.AppendChild(node);

                node = doc.CreateElement("LastUpdate");
                node.AppendChild(doc.CreateTextNode(DateTime.Now.ToString()));
                playlistnode.AppendChild(node);

                node = doc.CreateElement("Path");
                node.AppendChild(doc.CreateTextNode(rootpath + "/" + item.Snippet.Title));
                playlistnode.AppendChild(node);

                doc.DocumentElement.AppendChild(playlistnode);

            }
            else
            {
                
                playlistnode = doc.SelectSingleNode("//Playlist[@Id='" + item.Id + "']");
                string k = playlistnode.Attributes["Id"].Value;
                foreach(XmlElement xn in playlistnode)
                {
                    switch(xn.Name)
                    {
                        case "Name":
                            xn.InnerText = item.Snippet.Title;
                            break;
                        case "SongsCount":
                            xn.InnerText = item.ContentDetails.ItemCount.ToString();
                            break;
                        case "ChannelName":
                            xn.InnerText = item.Snippet.ChannelTitle;
                            break;
                        case "LastUpdate":
                            xn.InnerText = DateTime.Now.ToString();
                            break;
                        case "Path":
                            xn.InnerText = rootpath + "/" + item.Snippet.Title;
                            break;
                        default:
                            xn.InnerText = "";
                            break;
                    }
                }               
            }

            doc.Save(rootpath + "/info.xml");
        }
        public string[] Directories()
        {
            string[] pom = { };
            SongsCount = 0;
            if (!Directory.Exists(rootpath))
                return pom;
            else
            {

                pom = Directory.GetDirectories(rootpath);
                for(int i = 0; i<pom.Length;i++)
                {
                    SongsCount = Directory.GetFiles(pom[i]).Length + SongsCount - 1;
                }

                return pom;
            }
        }
        public void CreateRootDirectory()
        {
            if(!Directory.Exists(rootpath))
            {
                Directory.CreateDirectory(rootpath);
            }
        }
        public async Task DownloadByYouTubeExplode(string url, string path, bool onlyAudio)
        {
            var client = new YoutubeClient();
            string id = YoutubeClient.ParseVideoId(url);
            var video = await client.GetVideoAsync(id);
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);

            if (onlyAudio)
            {
                var streamInfo = streamInfoSet.Audio.WithHighestBitrate();
                var ext = streamInfo.Container.GetFileExtension();
                if (!File.Exists(path + "/" + video.Title + "." + ext))
                {
                    await client.DownloadMediaStreamAsync(streamInfo, path + "/" + video.Title + "." + ext);
                    MessageBox.Show("Downloaded: " + video.Title);
                }
            }
            else
            {
                var streamInfo = streamInfoSet.Video.WithHighestVideoQuality();
                var ext = streamInfo.Container.GetFileExtension();
                if (!File.Exists(path + "/" + video.Title + "." + ext))
                {
                    await client.DownloadMediaStreamAsync(streamInfo, path + "/" + video.Title + "." + ext);
                    MessageBox.Show("Downloaded: " + video.Title);
                }
            }

        }
        public async Task DownloadOneSongAsync(string url, string path)
        {
            var client = new YoutubeClient();
            string id = YoutubeClient.ParseVideoId(url);
            
            var video = await client.GetVideoAsync(id);

            
            var streamInfoSet = await client.GetVideoMediaStreamInfosAsync(id);


            var streamInfo = streamInfoSet.Audio.WithHighestBitrate();
            var ext = streamInfo.Container.GetFileExtension();
            if(!File.Exists(path + "/" + video.Title + "." + ext))
            {
                await client.DownloadMediaStreamAsync(streamInfo, path + "/" + video.Title + "." + ext);
                MessageBox.Show("Downloaded: " + video.Title);
            }

        }
        public void DownloadOneSong(string url,string path)
        {
            string sourceformp3 = path;
            var youtube = YouTube.Default;

            try
            {
                var vid = youtube.GetVideo(url);
                //File.WriteAllBytes(dwnpath + "/" + vid.FullName, vid.GetBytes()); // w tym miejscu najprawdopobniej wyciek, lepiej uzyc strumienia
                var stream = new MemoryStream(vid.GetBytes());

                FileStream file = new FileStream(path + "/" + vid.FullName, FileMode.Create, FileAccess.ReadWrite);
                stream.CopyTo(file);
                file.Close();

                stream = null;

                //string videoname = vid.FullName;
                //videoname = videoname.Substring(0, videoname.Length - 4);

                //var inputFile = new MediaFile { Filename = path + "/" + vid.FullName };
                //var outputFile = new MediaFile { Filename = $"{sourceformp3 + "/" + videoname }.mp3" };

                //using (var engine = new Engine())
                //{
                //    engine.GetMetadata(inputFile);

                //    engine.Convert(inputFile, outputFile);

                //}

                //File.Delete(path + "/" + vid.FullName);

            }
            catch (Exception e)
            {

            }
        }
        public async Task DownloadOneVideoAsync(string url, string path)
        {
            string sourceformp3 = path;
            var youtube = YouTube.Default;

            try
            {
                var vid = await youtube.GetVideoAsync(url);
                Stream sm = await vid.StreamAsync();
                FileStream file = new FileStream(path + "/" + vid.FullName, FileMode.Create, FileAccess.ReadWrite);
                sm.CopyTo(file);
                file.Close();
                ShowNotification(vid.Title);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        public void DownloadAllVideos()
        {
            string dwnpath = rootpath + "/" + PlaylistName;
            string[] index = { };
            if (!Directory.Exists(dwnpath))
            {
                Directory.CreateDirectory(dwnpath);
            }
            //SaveSongsNodes(playlistURL, dwnpath);
            playlistURL = SongsToDownload(playlistURL, dwnpath);
            for(int i = 0;i<=playlistURL.Count-1;i++)
            {
                DownloadOneSongAsync(playlistURL[i], dwnpath);
            }
        }        
        public string ParseplaylistId(string url)
        {
            int pos;
            pos = url.IndexOf("list=");
            if (pos != -1)
            {
                url = url.Substring(pos + 5);
            }
            else
                url = "";
            PlaylistId = url;
            return url;
        }
        public async Task ParseVideosURLsAsync(string id)
        {
            playlistURL.Clear();
            try
            {
                var yt = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyAJ10uqhKkiONdru2o_TKnF6EGJSCCowW0" });
                var playlistListRequest = yt.Playlists.List("snippet,contentDetails");
                playlistListRequest.Id = id;

                var playlistListResponse = await playlistListRequest.ExecuteAsync();
                AddNode(playlistListResponse.Items[0]);
                foreach (var video in playlistListResponse.Items)
                {
                    
                    var uploadsListId = video.Id;
                    var nextPageToken = "";
                    PlaylistName = video.Snippet.Title;
                    Channelname = video.Snippet.ChannelTitle;
                    Itemscount = (int)video.ContentDetails.ItemCount;
                    while (nextPageToken != null)
                    {
                        var playlistItemsListRequest = yt.PlaylistItems.List("snippet");
                        playlistItemsListRequest.PlaylistId = uploadsListId;
                        playlistItemsListRequest.MaxResults = 50;
                        playlistItemsListRequest.PageToken = nextPageToken;
                        // Retrieve the list of videos uploaded to the authenticated user's channel.  
                        var playlistItemsListResponse =  await playlistItemsListRequest.ExecuteAsync();

                        foreach (var playlistItem in playlistItemsListResponse.Items)
                        {

                            playlistURL.Add("https://www.youtube.com/embed/" + playlistItem.Snippet.ResourceId.VideoId);

                            //Console.WriteLine(counter.ToString() + ". https://www.youtube.com/embed/" + playlistItem.Snippet.ResourceId.VideoId);
                            //Console.Write("Video Title ={0} ", playlistItem.Snippet.Title);  
                            //Console.Write("Video Descriptions = {0}", playlistItem.Snippet.Description);  
                            //Console.WriteLine("Video ImageUrl ={0} ", playlistItem.Snippet.Thumbnails.High.Url);  
                            //Console.WriteLine("----------------------");  
                        }
                        nextPageToken = playlistItemsListResponse.NextPageToken;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Wystąpił wyjątek: " + e);
            }
        }
        public async Task RefreshListAsync(string url, string path)
        {
            var client = new YoutubeClient();
            var playlist = await client.GetPlaylistAsync(url,10);
            var videos = playlist.Videos;
            List<string> idlist = new List<string>();
            foreach(var item in videos)
            {
                idlist.Add(item.Id);
            }
            idlist = SongsToDownload(idlist, path);
            for (int i = 0; i <= idlist.Count - 1; i++)
            {
                DownloadOneSongAsync(idlist[i], path);
            }

        }
        private void ShowToast()
        {
            
        }
        private void ShowNotification(string title)
        {
            Notifier notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: System.Windows.Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: -580,
                    offsetY: -600);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(5));

                cfg.Dispatcher = System.Windows.Application.Current.Dispatcher;


            });


            //ToastNotifier aa;
            //ToastNotification toast = new ToastNotification()

            //aa.Show();
            notifier.ShowError("Downloaded " + title);
        }

    }
}
