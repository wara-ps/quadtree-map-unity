using System;
using System.Collections;
using System.Collections.Generic;
using QuadTreeMapEngine.Data;
using UnityEngine;

namespace QuadTreeMapEngine
{
    [RequireComponent(typeof(MapTileManager))]
    public abstract class MapTileLoader : MonoBehaviour
    {
        protected abstract IEnumerator LoadMeshRoutine(MapTreeNode node);
        protected abstract IEnumerator LoadTextureRoutine(MapTreeNode node);

        protected virtual void Start() { }

        protected virtual void Update() { }

        public virtual void LoadTile(MapTreeNode node, Action<bool> callback)
        {
            StartCoroutine(LoadTileRoutine(node, callback));
        }

        public virtual void UnloadTile(MapTreeNode node, Action<bool> callback)
        {
            Destroy(node.Tile.gameObject);
            node.Tile = null;

            callback?.Invoke(true);
        }

        protected virtual IEnumerator LoadTileRoutine(MapTreeNode node, Action<bool> callback)
        {
            CreateTile(node);

            var tasks = new List<Coroutine>
            {
                StartCoroutine(LoadMeshRoutine(node)),
                StartCoroutine(LoadTextureRoutine(node))
            };

            foreach (var task in tasks)
            {
                yield return task;
            }

            while (!node.Tile.MeshLoaded || !node.Tile.TextureLoaded)
            {
                yield return null;
            }

            callback?.Invoke(true);
        }

        protected void CreateTile(MapTreeNode node)
        {
            var g = new GameObject(node.Key);
            g.transform.SetParent(transform);
            g.transform.position = node.WorldPosition;
            g.transform.rotation = Quaternion.identity;

            node.Tile = g.AddComponent<MapTile>();
            node.Tile.Node = node;
        }
    }
}
