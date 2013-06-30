using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WallpaperRotator
{
    class ImgurDownloader
    {
        public static List<ImgurURL> images;
        int screenWidth;
        int screenHeight;

        public ImgurDownloader(IEnumerable<ImgurURL> imageURLs, int ScreenWidth, int ScreenHeight)
        {
            screenWidth = ScreenWidth;
            screenHeight = ScreenHeight;
            images = new List<ImgurURL>();
            GetImageURLs(imageURLs);
            DownloadImages(images);
        }

        private static JObject returnJSONData(string id, ImgurURL.ImgurType type)
        {
            HttpWebRequest webRequest;
            StreamReader reader;
            JsonTextReader jsReader;
            if (type == ImgurURL.ImgurType.image)
            {
                webRequest = (HttpWebRequest)WebRequest.Create("https://api.imgur.com/3/image/" + id);
            }
            else if (type == ImgurURL.ImgurType.album)
            {
                webRequest = (HttpWebRequest)WebRequest.Create("https://api.imgur.com/3/album/" + id);
            }
            else {
                webRequest = (HttpWebRequest)WebRequest.Create("https://api.imgur.com/3/gallery/album/" + id);
            }
            webRequest.Headers.Add("Authorization", "Client-ID baf34f2e93744b2");
            reader = new StreamReader(webRequest.GetResponse().GetResponseStream());
            jsReader = new JsonTextReader(new StringReader(reader.ReadToEnd()));
            return (JObject)new JsonSerializer().Deserialize(jsReader);
        }

        private static void GetImageURLs(IEnumerable<ImgurURL> imageURLs)
        {
            
            foreach (ImgurURL item in imageURLs)
            {
                JObject json = returnJSONData(item.id, item.type);

                if (item.type == ImgurURL.ImgurType.image || (json["data"]["is_album"] != null && json["data"]["is_album"].ToString() == "false"))
                {
                    images.Add(new ImgurURL(Convert.ToString(json["data"]["link"]), Convert.ToString(json["data"]["id"]),
                                            Convert.ToInt32(json["data"]["width"]), Convert.ToInt32(json["data"]["height"]),
                                               ImgurURL.ImgurType.image));
                }
                else if (item.type == ImgurURL.ImgurType.album || (json["data"]["is_album"] != null && json["data"]["is_album"].ToString() == "true"))
                {
                    foreach (var temp in json["data"]["images"])
                    {
                        images.Add(new ImgurURL(Convert.ToString(temp["link"]), Convert.ToString(temp["id"]),
                                                Convert.ToInt32(temp["width"]), Convert.ToInt32(temp["height"]),
                                                ImgurURL.ImgurType.image));
                    }
                }  
            }
            
        }

        private void DownloadImages(List<ImgurURL> images)
        {
            foreach (ImgurURL item in images)
            {
                using (WebClient Client = new WebClient ())
                {
                    string filePath = item.link.Substring(item.link.LastIndexOf("/") + 1);
                    if (item.width > screenWidth && item.height > screenHeight && !File.Exists(filePath))
                    {
                        Client.DownloadFile(item.link, filePath);
                    }
                }
            }
        }

    }
}
