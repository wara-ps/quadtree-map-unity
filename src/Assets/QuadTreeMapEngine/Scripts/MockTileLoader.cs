using System.Collections;
using QuadTreeMapEngine.Data;
using UnityEngine;

namespace QuadTreeMapEngine
{
    public class MockTileLoader : MapTileLoader
    {
        [SerializeField]
        protected Material TileMat;

        protected override IEnumerator LoadMeshRoutine(MapTreeNode node)
        {
            var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
            g.transform.SetParent(node.Tile.transform);

            g.transform.position = node.WorldPosition + 0.5f * new Vector3((float)node.Size, 0, (float)node.Size);
            g.transform.localRotation = Quaternion.identity;
            g.transform.localScale = new Vector3((float)node.Size, 1, (float)node.Size);

            yield return new WaitForSeconds(1);

            node.Tile.MeshLoaded = true;
        }

        protected override IEnumerator LoadTextureRoutine(MapTreeNode node)
        {
            while (!node.Tile.MeshLoaded)
                yield return null;

            var r = node.Tile.GetComponentInChildren<Renderer>();
            r.material = TileMat;
            r.material.color = Color.Lerp(Color.white, Color.red, node.Level * 0.3f);

            yield return new WaitForSeconds(1);

            node.Tile.TextureLoaded = true;
        }
    }
}
