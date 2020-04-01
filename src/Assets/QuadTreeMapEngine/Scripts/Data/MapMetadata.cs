using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace QuadTreeMapEngine.Data
{
    [XmlRoot("Map")]
    public class MapMetadata
    {
        [XmlElement]
        public Double2 Position { get; set; } = new Double2();

        [XmlElement]
        public Double3 Rotation { get; set; } = new Double3();

        [XmlElement]
        public Double2 AnchorOffset { get; set; } = new Double2();

        [XmlElement]
        public int MaxLevel { get; set; }

        [XmlElement]
        public double BaseTileSize { get; set; }

        [XmlArray("Tiles")]
        [XmlArrayItem("Tile")]
        public List<TileMetadata> Tiles { get; set; }

        protected Dictionary<string, TileMetadata> TileIndex = new Dictionary<string, TileMetadata>();

        public void CreateIndex()
        {
            foreach (var data in Tiles)
            {
                var key = $"{data.Level}_{data.X}_{data.Y}";
                TileIndex[key] = data;
            }
        }

        public TileMetadata FindTile(int level, int x, int y)
        {
            var key = $"{level}_{x}_{y}";
            return TileIndex.ContainsKey(key) ? TileIndex[key] : null;
        }

        public static MapMetadata ReadXml(string xml)
        {
            var bom = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xml.StartsWith(bom))
                xml = xml.Remove(0, bom.Length);

            using (var reader = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(MapMetadata));
                var data = (MapMetadata)serializer.Deserialize(reader);
                return data;
            }
        }

        public static string WriteXml(MapMetadata data)
        {
            using (var writer = new StringWriterUtf8())
            {
                var serializer = new XmlSerializer(typeof(MapMetadata));
                serializer.Serialize(writer, data);
                return writer.ToString();
            }
        }
    }

    public class TileMetadata
    {
        [XmlAttribute]
        public int Level { get; set; }

        [XmlAttribute]
        public int X { get; set; }

        [XmlAttribute]
        public int Y { get; set; }

        [XmlElement]
        public string MeshFile { get; set; }

        [XmlElement]
        public string TextureFile { get; set; }

        [XmlElement]
        public string BundleFile { get; set; }
    }

    public class MockMetadata : MapMetadata
    {
        public static MapMetadata Mock()
        {
            var tiles = new List<TileMetadata>();
            var radius = 1;
            var level = 2;
            for (int i = -radius; i <= radius; ++i)
            {
                for (int j = -radius; j <= radius; ++j)
                {
                    tiles.AddRange(CreateSubTree(0, i, j, level));
                }
            }
            var data = new MapMetadata
            {
                MaxLevel = level,
                BaseTileSize = 100,
                Tiles = tiles
            };
            Debug.Log(string.Join("\n", tiles.Select(x => x.MeshFile)));

            var ser = new XmlSerializer(typeof(MapMetadata));
            var w = new StringWriter();
            ser.Serialize(w, data);
            Debug.Log(w.ToString());

            return data;
        }

        private static List<TileMetadata> CreateSubTree(int level, int x, int y, int maxlevel)
        {
            var tiles = new List<TileMetadata>();
            tiles.Add(new TileMetadata
            {
                Level = level,
                X = x,
                Y = y,
                MeshFile = $"L{level}_{x}_{y}.obj"
            });

            if (level == maxlevel)
            {
                return tiles;
            }

            for (int row = 0; row < 2; ++row)
            {
                for (int col = 0; col < 2; ++col)
                {
                    tiles.AddRange(CreateSubTree(level + 1, x * 2 + col, y * 2 + row, maxlevel));
                }
            }

            return tiles;
        }
    }

    internal class StringWriterUtf8 : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
