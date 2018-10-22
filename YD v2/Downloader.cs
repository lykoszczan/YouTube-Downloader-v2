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
using System.Drawing;
using System.Net;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using YoutubeExtractor;

namespace YD_v2
{
    class Downloader
    { 
        public string PlaylistId { get; private set; }
        public string PlaylistName { get; private set; }
        public string Channelname { get; private set; }
        public int Itemscount { get; private set; }
        public int SongsCount { get; private set; }
        public string Rootpath { get; private set; }
        public string XmlInfoPath { get; set; }
            

        private List<string> playlistURL;
        public List<string> playlistSongs;

        public Downloader()
        {
            playlistURL = new List<string>();
            playlistSongs = new List<string>();
            Rootpath =  Properties.Settings.Default.DefaultPath + "/YDv2";
            XmlInfoPath = Properties.Settings.Default.DefaultPath + "/YDv2/Info.xml";
            CreateRootDirectory();
            CreateInfoXML();            
        }
        public void SetStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }
        private void CreateInfoXML()
        {
            if (!File.Exists(XmlInfoPath))
            {
                XmlDocument doc = new XmlDocument();
                XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                doc.AppendChild(docNode);

                XmlNode playlistsNode = doc.CreateElement("Playlists");
                doc.AppendChild(playlistsNode);

                doc.Save(XmlInfoPath);
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
            doc.Load(Rootpath + "/info.xml");

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
                node.AppendChild(doc.CreateTextNode(Rootpath + "/" + item.Snippet.Title));
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
                            xn.InnerText = Rootpath + "/" + item.Snippet.Title;
                            break;
                        default:
                            xn.InnerText = "";
                            break;
                    }
                }               
            }

            doc.Save(Rootpath + "/info.xml");
        }
        public string[] Directories()
        {
            string[] pom = { };
            SongsCount = 0;
            if (!Directory.Exists(Rootpath))
                return pom;
            else
            {

                pom = Directory.GetDirectories(Rootpath);
                for(int i = 0; i<pom.Length;i++)
                {
                    SongsCount = Directory.GetFiles(pom[i]).Length + SongsCount - 1;
                }

                return pom;
            }
        }
        public void CreateRootDirectory()
        {
            if(!Directory.Exists(Rootpath))
            {
                Directory.CreateDirectory(Rootpath);
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
                var streamInfo = streamInfoSet.Muxed.WithHighestVideoQuality();
                var ext = streamInfo.Container.GetFileExtension();
                if (!File.Exists(path + "/" + video.Title + "." + ext))
                {
                    await client.DownloadMediaStreamAsync(streamInfo, path + "/" + video.Title + "." + ext);
                    MessageBox.Show("Downloaded: " + video.Title);
                }
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
                MessageBox.Show(e.ToString());
            }
        }

        public void DownloadAllVideos(bool onlyAudio, List<string> Urls)
        {
            for (int i = 0; i <= Urls.Count - 1; i++)
            {
                DownloadByYouTubeExplode(Urls[i], Rootpath, onlyAudio);
            }
        }

        public void DownloadAllVideos(bool onlyAudio)
        {
            string dwnpath = Rootpath + "/" + PlaylistName;
            string[] index = { };
            if (!Directory.Exists(dwnpath))
            {
                Directory.CreateDirectory(dwnpath);
            }
            
            playlistURL = SongsToDownload(playlistURL, dwnpath);

            for (int i = 0;i<=playlistURL.Count-1;i++)
            {
                DownloadByYouTubeExplode(playlistURL[i], dwnpath, onlyAudio);
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
            playlistSongs.Clear();
            try
            {
                var yt = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyAJ10uqhKkiONdru2o_TKnF6EGJSCCowW0" });
                var playlistListRequest = yt.Playlists.List("snippet,contentDetails");
                playlistListRequest.Id = id;

                var playlistListResponse = await playlistListRequest.ExecuteAsync();



                AddNode(playlistListResponse.Items[0]);
                int i = 0;
                string videoIdWithTitle;
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

                            videoIdWithTitle = playlistItem.Snippet.Title + ";" + "https://www.youtube.com/embed/" + playlistItem.Snippet.ResourceId.VideoId;
                            playlistSongs.Add(videoIdWithTitle);


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
                DownloadByYouTubeExplode(idlist[i], path, true);
            }

        }

    }
}
