using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using static StoryboarderUtility.MiscUtil;

namespace StoryboarderUtility
{
    [DataContract]
    public class Storyboard
    {
        [DataMember]
        public string version = "";

        [DataMember(Name = "aspectRatio")]
        public string aspectRatioString = "1";

        public float aspectRatio = 1;

        public int ImageDiagonalSize = 600;   //diagonal

        public int ImageWidth = 0;
        public int ImageHeight = 0;

        [DataMember]
        public List<Board> boards = new List<Board>();

        public Dictionary<string, Board> boardDict = new Dictionary<string, Board>();

        public string filePath = "";

        public DateTime storyboardLastModified;
        public DateTime boardsLastModified;
        public DateTime lastModified;

        public int Hash = 0;
        

        public void PostLoad() {
            aspectRatio = float.Parse(aspectRatioString);

            boardsLastModified = new DateTime(0);
            boardDict = boards.ToDictionary(b => b.uid);
            foreach (var b in boards) {
                b.PostLoad(this);
                if (boardsLastModified > b.lastModified)
                    boardsLastModified = b.lastModified;
            }



            storyboardLastModified = System.IO.File.GetLastWriteTime(filePath);
            lastModified = boardsLastModified > storyboardLastModified ? boardsLastModified : storyboardLastModified;

            UpdateHash();
        }

        public void UpdateHash()
        {
            int Hash = 0;
            Hash = RingShift(Hash, 3) ^ version.GetHashCode();
            Hash = RingShift(Hash, 3) ^ aspectRatio.GetHashCode();
            Hash = RingShift(Hash, 3) ^ filePath.GetHashCode();

            if (boards != null) {
                foreach (var b in boards) {
                    b.UpdateHash();
                    Hash = RingShift(Hash, 3) ^ b.Hash;
                }
            }

                
        }

    }


    [DataContract]
    public class Board {

        public static readonly Dictionary<string, int> LayerOrder = new Dictionary<string, int>() {
            {"reference",0 },
            {"fill",1 },
            {"tone",2 },
            {"pencil",3 },
            {"ink",4 },
            {"notes",5 },
        };

        [DataMember]
        public string uid = "";

        [DataMember]
        public string url = "";
        
        [DataMember(Name = "layers")]
        public Dictionary<string, Layer> layerDict = new Dictionary<string, Layer>();
        public List<Layer> layers = new List<Layer>();

        //public int ImageWidth = 0;
        //public int ImageHeight = 0;
        //public int x, y;

        public System.Drawing.Image image;

        public DateTime lastModified;

        public int Hash = 0;




        public void UpdateLayerList() {
            var ll = layerDict.ToList();
            ll.Sort((a, b) => LayerOrder[a.Key] - LayerOrder[b.Key]);
            layers = (from p in ll select p.Value).ToList();
        }
        public void PostLoad(Storyboard sb) {
            UpdateLayerList();

            lastModified = new DateTime(0);

            foreach (Layer l in layers) {
                l.PostLoad(sb, this);
                if (l.lastModified > lastModified)
                    lastModified = l.lastModified;
            }

        }


        public void UpdateHash() {
            Hash = 0;
            Hash = RingShift(Hash, 3) ^ uid.GetHashCode();

            if (layers != null) {
                foreach (var l in layers) {
                    l.UpdateHash();
                    Hash = RingShift(Hash, 3) ^ l.Hash;
                }
            }

                
        }

    }


    [DataContract]
    public class Layer {
        [DataMember]
        public string url = "";


        [DataMember]
        public float opacity = 1;


        public string filePath = "";
        public DateTime lastModified;


        public int Hash = 0;


        public void PostLoad(Storyboard sb, Board board) {
            string basePath = System.IO.Path.GetDirectoryName(sb.filePath);
            filePath = System.IO.Path.Combine(basePath, "images", url);

            lastModified = System.IO.File.GetLastWriteTime(filePath);
        }

        public void UpdateHash() {
            Hash = 0;
            Hash = RingShift(Hash, 3) ^ filePath.GetHashCode();
            Hash = RingShift(Hash, 3) ^ opacity.GetHashCode();

        }
    }
}
