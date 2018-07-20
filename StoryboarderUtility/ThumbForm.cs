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
        public const int ThumbMargin = 32;
        public const int SliderWidth = 8;
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
            panelThumb.Invalidate();
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

        Storyboard currStoryboard = new Storyboard() { ImageDiagonalSize = ThumbDiagonalSize };
        
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
            storyboard.ImageDiagonalSize = ThumbDiagonalSize;
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

            float f = (float)(current.ImageDiagonalSize / Math.Sqrt(current.aspectRatio * current.aspectRatio + 1));
            current.ImageWidth = (int)(current.aspectRatio * f);
            current.ImageHeight = (int)f;


            foreach (var b in loaded.boards) {
                LoadBoard(current, b);
            }

            // sort boards
            Dictionary<string, int> uid2order = new Dictionary<string, int>();
            for (int i = 0; i < loaded.boards.Count; ++i)
                uid2order.Add(loaded.boards[i].uid, i);
            current.boards.Sort((a, b) => uid2order[a.uid] - uid2order[b.uid]);
            

            current.filePath = loaded.filePath;
            current.version = loaded.version;

            current.boardsLastModified = loaded.boardsLastModified;
            current.storyboardLastModified = loaded.storyboardLastModified;
            current.lastModified = loaded.lastModified;

            current.UpdateHash();

            panelThumb.Invalidate();
        }


        void UnloadBoard(Storyboard sb, Board board) {
            sb.boardDict.Remove(board.uid);
            sb.boards.Remove(board);


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

                Image[] layerImages = new Image[curBoard.layers.Count];
                for (int i = 0; i < layerImages.Length; ++i) {
                    try { layerImages[i] = Image.FromFile(curBoard.layers[i].filePath); }
                    catch (IOException) { }                        
                }

                int width = sb.ImageWidth;
                int height = sb.ImageHeight;

                Bitmap boardImage = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(boardImage)) {
                    g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
                    g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                    Rectangle rect = new Rectangle(0, 0, width, height);

                    g.Clear(Color.White);
                    foreach (var img in layerImages) {
                        if (img != null)
                            g.DrawImage(img, rect);
                    }
                        
                }

                foreach (var img in layerImages)
                    img.Dispose();

                curBoard.image = boardImage;



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



            int clientWidth = panelThumb.ClientSize.Width - SliderWidth;

            int margin = (int)(ThumbMargin * ImageDisplayScale);
            int sideMargin = margin;
            int dw = (int)(currStoryboard.ImageWidth * ImageDisplayScale);
            int dh = (int)(currStoryboard.ImageHeight * ImageDisplayScale);

            int columns = (int)Math.Floor(Math.Max(0, clientWidth - sideMargin * 2 - dw) / (float)(dw + margin)) + 1;

            int rows = currStoryboard.boards.Count / columns;
            if (currStoryboard.boards.Count % columns != 0)
                rows += 1;

            int areaHeight = sideMargin * 2 + margin + (dh + margin) * rows;
            int areaWidth = sideMargin * 2 + margin + (dw + margin) * columns;

            Bitmap bm = new Bitmap(areaWidth, areaHeight);

            using (var g = Graphics.FromImage(bm)) {
                g.Clear(Color.DimGray);
                DrawThumbs(g, areaWidth, 0);
            }

            bm.Save(sfd.FileName);

        }


        void DrawThumbs(Graphics g, int targetWidth, int yOffset) {
            int clientWidth = targetWidth;

            int margin = (int)(ThumbMargin * ImageDisplayScale);
            int sideMargin = margin;
            int dw = (int)(currStoryboard.ImageWidth * ImageDisplayScale);
            int dh = (int)(currStoryboard.ImageHeight * ImageDisplayScale);

            int columns = (int)Math.Floor(Math.Max(0, clientWidth - sideMargin * 2 - dw) / (float)(dw + margin)) + 1;
            int actualSideMargin = Math.Max(0, clientWidth - columns * dw - (columns - 1) * margin) / 2;

            int rows = currStoryboard.boards.Count / columns;
            if (currStoryboard.boards.Count % columns != 0)
                rows += 1;

            int areaHeight = sideMargin * 2 + margin + (dh + margin) * rows;

            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;


            //Color brushColor = Color.FromArgb(255, (int)(Color.DimGray.R * 0.5f), (int)(Color.DimGray.G * 0.5f), (int)(Color.DimGray.B * 0.5f));
            Color brushColor = Color.FromArgb(128, 0, 0, 0);
            int shadowOffset = (int)(ThumbMargin / 2 * ImageDisplayScale);

            using (Brush darkBrush = new SolidBrush(brushColor)) {
                for (int i = 0; i < currStoryboard.boards.Count; ++i) {
                    int cidx = i % columns;
                    int ridx = i / columns;

                    var board = currStoryboard.boards[i];
                    int x = actualSideMargin + cidx * (dw + margin);
                    int y = sideMargin + ridx * (dh + margin) - yOffset;

                    Rectangle rect = new Rectangle(x, y, dw, dh);
                    Rectangle shadowRect = rect;
                    shadowRect.X += shadowOffset;
                    shadowRect.Y += shadowOffset;

                    g.FillRectangle(darkBrush, shadowRect);
                    g.DrawImage(board.image, rect);
                    rect.Inflate(1, 1);
                    g.DrawRectangle(Pens.Black, rect);
                }
            }


        }

        /*
        private void panelThumb_Paint(object sender, PaintEventArgs e)
        {
            int clientWidth = panelThumb.ClientSize.Width - SliderWidth;
            int clientHeight = panelThumb.ClientSize.Height;

            int margin = (int)(ThumbMargin * ImageDisplayScale);
            int sideMargin = margin;
            int dw = (int)(currStoryboard.ImageWidth * ImageDisplayScale);
            int dh = (int)(currStoryboard.ImageHeight * ImageDisplayScale);

            int columns = (int)Math.Floor(Math.Max(0, clientWidth - sideMargin * 2 - dw) / (float)(dw + margin)) + 1;
            //int actualMargin = columns <= 1 ? 0 : (clientWidth - sideMargin * 2 - dw * columns) / (columns - 1);
            int actualSideMargin = Math.Max(0, clientWidth - columns * dw - (columns - 1) * margin) / 2;

            int rows = currStoryboard.boards.Count / columns;
            if (currStoryboard.boards.Count % columns != 0)
                rows += 1;

            int areaHeight = sideMargin * 2 + margin + (dh + margin) * rows;

            int yOffset = (int)(Math.Max(0, areaHeight - clientHeight) * scrollRatio);

            Color brushColor = Color.FromArgb(255, (int)(Color.DimGray.R * 0.5f), (int)(Color.DimGray.G * 0.5f), (int)(Color.DimGray.B * 0.5f));

            int shadowOffset = (int)(ThumbMargin / 2 * ImageDisplayScale);

            using (Brush darkBrush = new SolidBrush(brushColor)) {
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                for (int i = 0; i < currStoryboard.boards.Count; ++i) {
                    int cidx = i % columns;
                    int ridx = i / columns;

                    var board = currStoryboard.boards[i];
                    int x = actualSideMargin + cidx * (dw + margin);
                    int y = sideMargin + ridx * (dh + margin) - yOffset;

                    Rectangle rect = new Rectangle(x, y, dw, dh);
                    Rectangle shadowRect = rect;
                    //shadowRect.Inflate(ThumbMargin / 6, ThumbMargin / 6);
                    shadowRect.X += shadowOffset;
                    shadowRect.Y += shadowOffset;

                    e.Graphics.FillRectangle(darkBrush, shadowRect);
                    e.Graphics.DrawImage(board.image, rect);
                    rect.Inflate(1, 1);
                    e.Graphics.DrawRectangle(Pens.Black, rect);
                }
            }


            int sliderBarHeight = Math.Min(clientHeight, clientHeight * clientHeight / areaHeight);
            int sliderBarY = (int)(((areaHeight - clientHeight) * scrollRatio / areaHeight) * clientHeight);

            e.Graphics.FillRectangle(Brushes.Gray, new Rectangle(clientWidth, 0, SliderWidth, clientHeight));
            e.Graphics.FillRectangle(Brushes.DarkGray, new Rectangle(clientWidth, sliderBarY, SliderWidth, sliderBarHeight));
        }
        */

            
        private void panelThumb_Paint(object sender, PaintEventArgs e)
        {
            int clientWidth = panelThumb.ClientSize.Width - SliderWidth;
            int clientHeight = panelThumb.ClientSize.Height;

            int margin = (int)(ThumbMargin * ImageDisplayScale);
            int sideMargin = margin;
            int dw = (int)(currStoryboard.ImageWidth * ImageDisplayScale);
            int dh = (int)(currStoryboard.ImageHeight * ImageDisplayScale);

            int columns = (int)Math.Floor(Math.Max(0, clientWidth - sideMargin * 2 - dw) / (float)(dw + margin)) + 1;

            int rows = currStoryboard.boards.Count / columns;
            if (currStoryboard.boards.Count % columns != 0)
                rows += 1;

            int areaHeight = sideMargin * 2 + margin + (dh + margin) * rows;

            int yOffset = (int)(Math.Max(0, areaHeight - clientHeight) * scrollRatio);

            Color brushColor = Color.FromArgb(255, (int)(Color.DimGray.R * 0.5f), (int)(Color.DimGray.G * 0.5f), (int)(Color.DimGray.B * 0.5f));

            DrawThumbs(e.Graphics, clientWidth, yOffset);


            int sliderBarHeight = Math.Min(clientHeight, clientHeight * clientHeight / areaHeight);
            int sliderBarY = (int)(((areaHeight - clientHeight) * scrollRatio / areaHeight) * clientHeight);

            e.Graphics.FillRectangle(Brushes.Gray, new Rectangle(clientWidth, 0, SliderWidth, clientHeight));
            e.Graphics.FillRectangle(Brushes.DarkGray, new Rectangle(clientWidth, sliderBarY, SliderWidth, sliderBarHeight));
        }



        float scrollRatio = 0;

        float startScrollRatio = 0;
        int startY = 0;
        bool scrolling = false;

        private void panelThumb_MouseDown(object sender, MouseEventArgs e)
        {
            startY = e.Y;
            startScrollRatio = scrollRatio;
            scrolling = true;
        }

        private void panelThumb_MouseUp(object sender, MouseEventArgs e)
        {
            scrolling = false;
        }

        private void panelThumb_MouseMove(object sender, MouseEventArgs e)
        {
            if (scrolling) {
                int clientHeight = panelThumb.ClientSize.Height;
                int clientWidth = panelThumb.ClientSize.Width - SliderWidth;

                int margin = (int)(ThumbMargin * ImageDisplayScale);
                int sideMargin = margin;
                int dw = (int)(currStoryboard.ImageWidth * ImageDisplayScale);
                int dh = (int)(currStoryboard.ImageHeight * ImageDisplayScale);


                int columns = (int)Math.Floor(Math.Max(0, clientWidth - sideMargin * 2 - dw) / (float)(dw + margin)) + 1;
                int rows = currStoryboard.boards.Count / columns;
                if (currStoryboard.boards.Count % columns != 0)
                    rows += 1;

                int areaHeight = sideMargin * 2 + margin + (dh + margin) * rows;

                float deltaScroll = (float)(startY - e.Y) / Math.Max(0, areaHeight - clientHeight);

                scrollRatio = startScrollRatio + deltaScroll;
                scrollRatio = Math.Max(0, scrollRatio);
                scrollRatio = Math.Min(1, scrollRatio);

                panelThumb.Invalidate();
            }

        }
    }
}
