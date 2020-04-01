using QuadTreeMapEngine.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace QuadTreeMapEngine
{
    public class MapTileManager : MonoBehaviour
    {
        public Transform TargetTransform;
        public string BaseUrl;

        protected MapTileLoader Loader { get; set; }

        public MapMetadata Metadata { get; protected set; }
        protected List<MapTreeNode> BaseLevelNodes { get; } = new List<MapTreeNode>();
        protected Dictionary<string, MapTreeNode> NodeIndex { get; } = new Dictionary<string, MapTreeNode>();

        public bool SetupComplete { get; protected set; }

        protected virtual void Awake()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        }

        protected virtual void Start()
        {
            StartCoroutine(Setup());
        }

        protected IEnumerator Setup()
        {
            var start = DateTime.Now;
            SetupComplete = false;

            Loader = GetComponent<MapTileLoader>();

            yield return LoadMetadata();
            yield return SetupMapTree();

            SetupComplete = true;
            Debug.Log($"Finished tile manager setup in {(DateTime.Now - start).TotalSeconds} s.");
        }

        protected IEnumerator LoadMetadata()
        {
            //var www = new WWW(BaseUrl + "/metadata.xml");
            var www = UnityWebRequest.Get(BaseUrl + "/metadata.xml");
            //yield return www;
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError("Error while fetching map metadata: " + www.error);
                yield break;
            }

            Metadata = MapMetadata.ReadXml(www.downloadHandler.text);
            Metadata.CreateIndex();
        }

        protected void Update()
        {
            if (!SetupComplete || TargetTransform == null)
                return;

            var pos = Metadata.Position + new Double2(TargetTransform.position.x, TargetTransform.position.z);
            foreach (var node in BaseLevelNodes)
            {
                node.UpdateNodeRequirement(pos, false);
                node.UpdateNodeRedundancy();
                node.UpdateNode();
            }
        }

        protected IEnumerator SetupMapTree()
        {
            // create base level nodes
            foreach (var tiledata in Metadata.Tiles.Where(x => x.Level == 0))
            {
                var node = new MapTreeNode(null, tiledata);
                BaseLevelNodes.Add(node);
            }


            var time = DateTime.Now;
            // expand base level nodes into separate trees
            var q = new Queue<MapTreeNode>(BaseLevelNodes);
            while (q.Count > 0)
            {
                var node = q.Dequeue();
                NodeIndex[node.Key] = node;

                InitNode(node);

                if (node.Level >= Metadata.MaxLevel)
                    continue;

                // expand node into its children
                for (int row = 0; row < 2; ++row)
                {
                    for (int col = 0; col < 2; ++col)
                    {
                        var child = new MapTreeNode(node, node.Level + 1, node.X * 2 + col, node.Y * 2 + row);

                        node.AddChild(child);
                        q.Enqueue(child);
                    }
                }

                if ((DateTime.Now - time).TotalSeconds > 1f / 60)
                {
                    yield return null;
                    time = DateTime.Now;
                }
            }
        }

        protected void InitNode(MapTreeNode node)
        {
            if (node.Data == null)
                node.Data = Metadata.FindTile(node.Level, node.X, node.Y);

            node.Size = Metadata.BaseTileSize / (1 << node.Level);
            node.Origin = Metadata.Position + new Double2(node.Size * node.X, node.Size * node.Y);

            var tileoffset = new Vector3((float)(node.Size * node.X), 0, (float)(node.Size * node.Y));
            var anchoroffset = new Vector3((float)Metadata.AnchorOffset.X, 0, (float)Metadata.AnchorOffset.Y);
            node.WorldPosition = tileoffset;
            node.WorldRotation = Quaternion.Euler(Metadata.Rotation);
            node.AnchorOffset = anchoroffset - tileoffset;

            node.MinDistance = (node.Level == Metadata.MaxLevel) ? 0 : node.Size * Math.Sqrt(2);
            node.TileLoader = Loader;
        }
    }
}
