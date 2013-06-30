using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace WallpaperRotator
{
    public partial class Form1 : Form
    {

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        public enum Style : int
        {
            Tiled,
            Centered,
            Stretched
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        public static IEnumerable<FileInfo> GetFilesByExtensions(DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
            {
                throw new ArgumentNullException("extensions");
            }
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        public static void SetRandomWallPaper(Style style)
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            IEnumerable<FileInfo> files = GetFilesByExtensions(dir,".jpg", "png");
            Random randomNumber = new Random();
            FileInfo randomFile = files.ElementAt(randomNumber.Next(0, files.Count()));
            Console.WriteLine(randomFile.FullName);

            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            if (style == Style.Stretched)
            {
                key.SetValue(@"WallpaperStyle", 2.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Centered)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 0.ToString());
            }

            if (style == Style.Tiled)
            {
                key.SetValue(@"WallpaperStyle", 1.ToString());
                key.SetValue(@"TileWallpaper", 1.ToString());
            }

            SystemParametersInfo(SPI_SETDESKWALLPAPER,
                0,
                randomFile.FullName,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
        }


        public Form1()
        {
            InitializeComponent();
            Thread newThread = new Thread(new ThreadStart(Run));
            newThread.Start();
            
        }

        private void Run()
        {
            WebClient client = new WebClient();
            var data = client.DownloadString("http://www.reddit.com/r/wallpapers/new.json?sort=hot");
            var jsReader = new JsonTextReader(new StringReader(data));
            var json = (JObject)new JsonSerializer().Deserialize(jsReader);
            var URLs = from p in json["data"]["children"]
                            where Convert.ToString(p["data"]["thumbnail"]) != "self"
                            where Convert.ToString(p["data"]).Contains("imgur.com")
                            select new ImgurURL(Convert.ToString(p["data"]["url"]).Replace("[IMG]", "").Replace("[/IMG]", ""));
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            ImgurDownloader images = new ImgurDownloader(URLs, screenWidth, screenHeight);
        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
            else
            {
                this.Hide();
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetRandomWallPaper(Style.Stretched);
        }

    }
}
