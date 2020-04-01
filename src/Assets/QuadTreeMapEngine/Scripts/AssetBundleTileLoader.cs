using QuadTreeMapEngine.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace QuadTreeMapEngine
{
    public class AssetBundleTileLoader : MapTileLoader
    {
        [SerializeField]
        protected Material TileMat;

        public string BaseUrl { get; protected set; }

        protected Dictionary<string, AssetBundle> AssetBundleCache = new Dictionary<string, AssetBundle>();

        private readonly Queue<TileWorkItem> LoadTasks = new Queue<TileWorkItem>();
        private readonly Queue<TileWorkItem> UnloadTasks = new Queue<TileWorkItem>();

        protected override void Start()
        {
            var url = GetComponent<MapTileManager>().BaseUrl;
            BaseUrl = Path.Combine(url, "bundles");

            for (int i = 0; i < 5; ++i)
                StartCoroutine(LoadWorkerRoutine());
            for (int i = 0; i < 5; ++i)
                StartCoroutine(UnloadWorkerRoutine());
        }

        public override void LoadTile(MapTreeNode node, Action<bool> callback)
        {
            var task = new TileWorkItem { Node = node, Callback = callback };
            LoadTasks.Enqueue(task);
        }

        public override void UnloadTile(MapTreeNode node, Action<bool> callback)
        {
            var task = new TileWorkItem { Node = node, Callback = callback };
            UnloadTasks.Enqueue(task);
        }

        /// <summary>
        /// Worker "thread" to consume the queue of tile loading tasks.
        /// </summary>
        protected IEnumerator LoadWorkerRoutine()
        {
            while (true)
            {
                if (LoadTasks.Count > 0)
                {
                    var task = LoadTasks.Dequeue();
                    if (!task.Node.IsRequired)
                    {
                        task.Callback?.Invoke(false);
                        continue;
                    }

                    yield return LoadBundleRoutine(task.Node);

                    if (!task.Node.IsRequired)
                    {
                        UnloadBundle(task.Node);
                        task.Callback?.Invoke(false);
                        continue;
                    }

                    CreateTile(task.Node);

                    yield return LoadMeshRoutine(task.Node);

                    if (!task.Node.IsRequired)
                    {
                        Destroy(task.Node.Tile.gameObject);
                        task.Node.Tile = null;
                        UnloadBundle(task.Node);
                        task.Callback?.Invoke(false);
                        continue;
                    }

                    yield return LoadTextureRoutine(task.Node);

                    if (!task.Node.IsRequired)
                    {
                        Destroy(task.Node.Tile.gameObject);
                        task.Node.Tile = null;
                        UnloadBundle(task.Node);
                        task.Callback?.Invoke(false);
                        continue;
                    }

                    task.Callback?.Invoke(true);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Worker "thread" to consume the queue of tile unloading tasks. This
        /// involves waiting for tiles to be redundant before actually
        /// destroying them.
        /// </summary>
        protected IEnumerator UnloadWorkerRoutine()
        {
            while (true)
            {
                if (UnloadTasks.Count > 0)
                {
                    var task = UnloadTasks.Dequeue();

                    // mistake, abort
                    if (task.Node.IsRequired)
                    {
                        task.Callback?.Invoke(false);
                        continue;
                    }

                    // try again later
                    if (!task.Node.IsUnloadable())
                    {
                        UnloadTasks.Enqueue(task);
                        yield return null;
                        continue;
                    }

                    // destroy tile now
                    Destroy(task.Node.Tile.gameObject);
                    task.Node.Tile = null;
                    UnloadBundle(task.Node);

                    task.Callback?.Invoke(true);
                }

                yield return null;
            }
        }

        /// <summary>
        /// Fetch the asset bundle containing the tile from the web or from
        /// disk, depending on the url.
        /// </summary>
        protected IEnumerator LoadBundleRoutine(MapTreeNode node)
        {
            if (AssetBundleCache.ContainsKey(node.Key))
                yield break;

            if (node.Data == null)
                yield break;

            var url = GetAbsoluteUrl(node.Data.BundleFile);
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(url, 0, 0))
            {
                yield return request.SendWebRequest();

                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError($"Failed to send web request to URL: {url}");
                    Debug.LogError(request.error);
                }
                else
                {
                    var bundle = DownloadHandlerAssetBundle.GetContent(request);
                    AssetBundleCache[node.Key] = bundle;
                }
            }
        }

        /// <summary>
        /// Load mesh from the loaded asset bundle and instantiate the mesh
        /// GameObject.
        /// </summary>
        protected override IEnumerator LoadMeshRoutine(MapTreeNode node)
        {
            if (!AssetBundleCache.ContainsKey(node.Key))
            {
                node.Tile.MeshLoaded = true;
                yield break;
            }

            var g = new GameObject("mesh");
            g.transform.SetParent(node.Tile.transform);
            g.transform.localPosition = node.AnchorOffset;
            g.transform.localRotation = node.WorldRotation;

            var filter = g.AddComponent<MeshFilter>();
            var collider = g.AddComponent<MeshCollider>();
            var bundle = AssetBundleCache[node.Key];

            var request = bundle.LoadAssetAsync<Mesh>(node.Data.MeshFile);
            yield return request;

            filter.sharedMesh = (Mesh)request.asset;
            collider.sharedMesh = filter.sharedMesh;
            node.Tile.MeshLoaded = true;
        }

        /// <summary>
        /// Load texture from the loaded asset bundle and assign the material
        /// to the tile.
        /// </summary>
        protected override IEnumerator LoadTextureRoutine(MapTreeNode node)
        {
            if (!AssetBundleCache.ContainsKey(node.Key))
            {
                node.Tile.TextureLoaded = true;
                yield break;
            }

            var bundle = AssetBundleCache[node.Key];

            var request = bundle.LoadAssetAsync<Texture2D>(node.Data.TextureFile);
            yield return request;

            while (!node.Tile.MeshLoaded)
                yield return null;

            var g = node.Tile.GetComponentInChildren<MeshFilter>().gameObject;
            var renderer = g.AddComponent<MeshRenderer>();
            renderer.material = TileMat;
            renderer.material.mainTexture = (Texture2D)request.asset;

            node.Tile.TextureLoaded = true;
        }

        private string GetAbsoluteUrl(string relative)
        {
            var absolute = Path.Combine(BaseUrl, GetBuildPlatform(), relative);
            absolute = absolute.Replace("\\", "/");
            return absolute;
        }

        private string GetBuildPlatform()
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return "WebGL";
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                return "Windows";
            return "";
        }

        private void UnloadBundle(MapTreeNode node)
        {
            if (!AssetBundleCache.ContainsKey(node.Key))
                return;

            var bundle = AssetBundleCache[node.Key];
            AssetBundleCache.Remove(node.Key);
            bundle.Unload(true);
        }
    }

    internal class TileWorkItem
    {
        public MapTreeNode Node { get; set; }
        public Action<bool> Callback { get; set; }
    }
}