using QuadTreeMapEngine.Data;
using UnityEngine;

namespace QuadTreeMapEngine
{
    public class MapTile : MonoBehaviour
    {
        public MapTreeNode Node { get; set; }
        public bool MeshLoaded { get; set; }
        public bool TextureLoaded { get; set; }
    }
}
