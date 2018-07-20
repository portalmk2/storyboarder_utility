using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Json;
using System.Drawing;
using System.Diagnostics;

namespace StoryboarderUtility
{
    public partial class ThumbForm : Form
    {
        public const int ThumbMargin = 16;
        public const int ThumbDiagonalSize = 600;

        public ThumbForm()
        {
            InitializeComponent();

            timer = new Timer();
            timer.Enabled = false;
            timer.Tick += reloadTimer_Tick;
            timer.Interval = 200;
        }


        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Storyboarder(*.storyboarder)|*.storyboarder|All files(*.*)|*.*";

            if (ofd.ShowDialog() != DialogResult.OK) {
                ofd.Dispose();
                return;
            }


            Storyboard loaded = ParseStoryboard(ofd.FileName);
            if (loaded != null) {
                ReloadStoryboard(currStoryboard, loaded);
                UpdateAllBoardControlSize();

                SetupFileWatch();

                panelThumb.BackgroundImage = null;
                statusFileNameLabel.Text = currStoryboard.filePath;
            }
                

        }

        private void btnReload_Click(object sender, EventArgs e)
        {
            Storyboard loaded = ParseStoryboard(currStoryboard.filePath);
            if (loaded != null)
                ReloadStoryboard(currStoryboard, loaded);
        }



        float ImageDisplayScale { get { return scaleSlider.Value / 10.0f; } }

        private void scaleSlider_Scroll(object sender, EventArgs e) {
            UpdateAllBoardControlSize();
        }

        void UpdateAllBoardControlSize() {
            panelThumb.SuspendLayout();


            Padding margin = new Padding((int)(ThumbMargin * ImageDisplayScale));

            foreach (var b in currStoryboard.boards) {
                int width = (int)(b.ImageWidth * ImageDisplayScale);
                int height = (int)(b.ImageHeight * ImageDisplayScale);

                b.imageControl.Width = width;
                b.imageControl.Height = height;

                int widthDelta = b.imageControl.Width - b.imageControl.DisplayRectangle.Width;
                int heightDelta = b.imageControl.Height - b.imageControl.DisplayRectangle.Height;

                b.imageControl.Width = width + widthDelta;
                b.imageControl.Height = height + heightDelta;

                b.imageControl.Margin = margin;
            }

            panelThumb.ResumeLayout();
        }

        FileSystemWatcher fileWatcher = null;
        Timer timer;

        void SetupFileWatch() {
            if (fileWatcher != null) {
                fileWatcher.EnableRaisingEvents = false;
                fileWatcher.Dispose();
                fileWatcher = null;
            }

            // Create a new FileSystemWatcher and set its properties.
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = Path.GetDirectoryName(currStoryboard.filePath);
            /* Watch for changes in LastAccess and LastWrite times, and 
               the renaming of files or directories. */
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            // Only watch text files.
            fileWatcher.Filter = "*.storyboarder";

            // Add event handlers.
            fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);

            // Begin watching.
            fileWatcher.EnableRaisingEvents = true;
        }


        private void reloadTimer_Tick(object sender, EventArgs e)
        {
            Storyboard loaded = ParseStoryboard(currStoryboard.filePath);
            ReloadStoryboard(currStoryboard, loaded);
            timer.Stop();
        }

        void OnFileChanged(object sender, FileSystemEventArgs e) {
            this.BeginInvoke(new Action(() => timer.Enabled = true));
        }





        public class ImageInfo {
            public string filePath;
            public Image image;
            public DateTime lastModified;
            public int RefCount = 0;
        }

        Storyboard currStoryboard = new Storyboard() { ImageSize = ThumbDiagonalSize };
        
        Storyboard ParseStoryboard(string fileName) {
            string sbTxt = null;
            
            var time = Stopwatch.StartNew();
            while (sbTxt == null && time.ElapsedMilliseconds < 500) {
                try {
                    sbTxt = File.ReadAllText(fileName);
                }
                catch (IOException) {
                    sbTxt = null;
                }
            }

            time.Stop();
            if (sbTxt == null)
                return null;            

            DataContractJsonSerializerSettings jsopt = new DataContractJsonSerializerSettings();
            jsopt.UseSimpleDictionaryFormat = true;
            DataContractJsonSerializer js = new DataContractJsonSerializer(typeof(Storyboard), jsopt);
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(sbTxt));
            Storyboard storyboard = (Storyboard)js.ReadObject(stream);
            storyboard.filePath = fileName;
            storyboard.PostLoad();

            return storyboard;
        }

        void ReloadStoryboard(Storyboard current, Storyboard loaded) {
            if (current.Hash == loaded.Hash && current.lastModified == loaded.lastModified) {
                //MessageBox.Show("Same");
                return;
            }
            

            foreach (var b in current.boards.ToArray()) {
                if (!loaded.boardDict.ContainsKey(b.uid))
                    UnloadBoard(current, b);
            }

            current.aspectRatioString = loaded.aspectRatioString;
            current.aspectRatio = loaded.aspectRatio;

            foreach (var b in loaded.boards) {
                LoadBoard(current, b);
            }

            // sort boards
            Dictionary<string, int> uid2order = new Dictionary<string, int>();
            for (int i = 0; i < loaded.boards.Count; ++i)
                uid2order.Add(loaded.boards[i].uid, i);
            current.boards.Sort((a, b) => uid2order[a.uid] - uid2order[b.uid]);

            for (int i = 0; i < current.boards.Count; ++i)
                panelThumb.Controls.SetChildIndex(current.boards[i].imageControl, i);


            current.filePath = loaded.filePath;
            current.version = loaded.version;

            current.boardsLastModified = loaded.boardsLastModified;
            current.storyboardLastModified = loaded.storyboardLastModified;
            current.lastModified = loaded.lastModified;

            current.UpdateHash();
        }


        void UnloadBoard(Storyboard sb, Board board) {
            sb.boardDict.Remove(board.uid);
            sb.boards.Remove(board);


            panelThumb.Controls.Remove(board.imageControl);
            board.imageControl.Dispose();
            board.imageControl = null;

            board.image.Dispose();
            board.image = null;
        }

        void LoadBoard(Storyboard sb, Board loadedBoard) {
            Board curBoard;
            if (!sb.boardDict.TryGetValue(loadedBoard.uid, out curBoard)) {
                curBoard = new Board() { uid = loadedBoard.uid };
                sb.boardDict.Add(curBoard.uid, curBoard);
                sb.boards.Add(curBoard);
            }

            if (curBoard.Hash == loadedBoard.Hash && curBoard.lastModified == loadedBoard.lastModified)
                return;

            // load board
            curBoard.url = loadedBoard.url;

            foreach (var p in curBoard.layerDict.ToArray()) {
                if (!loadedBoard.layerDict.ContainsKey(p.Key))
                    UnloadLayer(curBoard, p.Key);
            }

            bool layerChanged = curBoard.image == null;
            foreach (var p in loadedBoard.layerDict) {
                layerChanged = LoadLayer(curBoard, p.Key, p.Value) || layerChanged;
            }


            curBoard.UpdateLayerList();
            curBoard.lastModified = loadedBoard.lastModified;

            
            if (layerChanged) {
                if (curBoard.image != null)
                    curBoard.image.Dispose();

                //Bitmap bm = new Bitmap()
                Image[] layerImages = (from l in curBoard.layers select Image.FromFile(l.filePath)).ToArray();
                int maxWidth = layerImages.Length == 0 ? 0 : layerImages.Max(img => img.Width);
                int maxHeight = layerImages.Length == 0 ? 0 : layerImages.Max(img => img.Height);

                if (maxWidth == 0 || maxHeight == 0) {
                    float f = (float)(sb.ImageSize / Math.Sqrt(sb.aspectRatio * sb.aspectRatio + 1));
                    maxWidth = (int)(sb.aspectRatio * f);
                    maxHeight = (int)f;
                }

                float dgnl = (float)Math.Sqrt(maxWidth * maxWidth + maxHeight * maxHeight);
                float scale = sb.ImageSize / dgnl;
                int width = (int)(scale * maxWidth);
                int height = (int)(scale * maxHeight);

                curBoard.ImageWidth = width;
                curBoard.ImageHeight = height;

                Bitmap boardImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(boardImage)) {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    Rectangle rect = new Rectangle(0, 0, width, height);

                    g.Clear(Color.White);
                    foreach (var img in layerImages)
                        g.DrawImage(img, rect);
                }

                foreach (var img in layerImages)
                    img.Dispose();

                curBoard.image = boardImage;

                
                if (curBoard.imageControl == null) {
                    curBoard.imageControl = new PictureBox();
                    curBoard.imageControl.BorderStyle = BorderStyle.FixedSingle;
                    curBoard.imageControl.SizeMode = PictureBoxSizeMode.StretchImage;

                    panelThumb.Controls.Add(curBoard.imageControl);
                }
                curBoard.imageControl.Image = curBoard.image;
                


            }

        }


        void UnloadLayer(Board board, string layerName) {
            board.layerDict.Remove(layerName);
            board.UpdateLayerList();
        }


        bool LoadLayer(Board board, string layerName, Layer layer) {
            Layer curLayer;
            if (!board.layerDict.TryGetValue(layerName, out curLayer)) {
                curLayer = new Layer();
                board.layerDict.Add(layerName, curLayer);
            }

            if (curLayer.Hash == layer.Hash && curLayer.lastModified == layer.lastModified)
                return false;

            curLayer.url = layer.url;
            curLayer.opacity = layer.opacity;
            curLayer.filePath = layer.filePath;
            curLayer.lastModified = layer.lastModified;

            return true;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {

            var lastBoard = currStoryboard.boards.LastOrDefault();
            if (lastBoard == null) {
                MessageBox.Show("No board available");
                return;
            }

            var sfd = new SaveFileDialog();
            sfd.Filter = "Bitmap files(*.bmp)|*.bmp";
            if (sfd.ShowDialog() != DialogResult.OK)
                return;


            
        }
    }
}
