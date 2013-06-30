using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallpaperRotator
{
    class ImgurURL
    {
        public enum ImgurType
        {
            album,
            image,
            gallery
        };

        public int width { get; set; }
        public int height { get; set; }
        public string id { get; set; }
        public string link { get; set; }
        public ImgurType type { get; set; }

        public ImgurURL(string Link)
        {
            link = removeHash(strip(Link));
            determineType(link);
            determineId(link);
        }

        public ImgurURL(string Link, string Id, int Width, int Height, ImgurType Type)
        {
            link = Link;
            id = Id;
            width = Width;
            height = Height;
            type = Type;
        }

        public override string ToString()
        {
            return type + "\t" + id + "\t" + width + "\t" + height + "\t" + link;
        }

        private string strip(string original)
        {
            return original.Replace("http://", "").Replace("www.", "");
        }

        private string removeHash(string original)
        {
            return original.Contains("#") ? original.Substring(0, original.IndexOf("#")) : original;
        }

        private void determineType(string Link)
        {
            if (Link.Contains("/a/"))
            {
                type = ImgurType.album;
            }
            else if (Link.Contains("/gallery/"))
            {
                type = ImgurType.gallery;
            }
            else
            {
                type = ImgurType.image;
            }
        }

        private void determineId(string link)
        {
            int start = link.LastIndexOf('/') + 1;
            int end = link.LastIndexOf('.') < start ? link.Length : link.LastIndexOf('.');
            id = link.Substring(start, end - start);
            if (id.Contains("?"))
            {
                id = id.Substring(0, id.IndexOf("?"));
            }
        }

    }
}
