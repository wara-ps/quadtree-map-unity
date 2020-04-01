using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace QuadTreeMapEngine.Data
{
    public class MapTreeNode : QuadTreeNode
    {
        public TileMetadata Data { get; set; }
        public MapTile Tile { get; set; }

        public bool IsRequired { get; protected set; }
        public bool IsRedundant { get; protected set; }
        public bool IsLoaded { get; protected set; }
        public bool IsUpdating { get; protected set; }
        public double Distance { get; protected set; }

        public double Size { get; set; }
        public Double2 Origin { get; set; }
        public Double2 Center => Origin + new Double2(0.5 * Size, 0.5 * Size);
        public double MinDistance { get; set; }

        public Vector3 WorldPosition { get; set; }
        public Quaternion WorldRotation { get; set; }
        public Vector3 AnchorOffset { get; set; }

        public MapTileLoader TileLoader { get; set; }

        public new MapTreeNode Parent => (MapTreeNode)base.Parent;
        protected new IEnumerable<MapTreeNode> Children => base.Children.Cast<MapTreeNode>();

        public MapTreeNode(MapTreeNode parent, int level, int x, int y) : base(parent, level, x, y)
        {
        }

        public MapTreeNode(MapTreeNode parent, TileMetadata data) : base(parent, data.Level, data.X, data.Y)
        {
            Data = data;
        }

        public void UpdateNodeRequirement(Double2 pos, bool satisfied)
        {
            Distance = (Center - pos).Magnitude;
            IsRequired = !satisfied && Distance > MinDistance;
            foreach (var child in Children.Where(x => x != null))
            {
                child.UpdateNodeRequirement(pos, satisfied || IsRequired);
            }
        }

        public bool UpdateNodeRedundancy()
        {
            if (IsRequired)
            {
                SetSubtreeRedundancy(IsLoaded);
                return IsLoaded;
            }
            else
            {
                bool result = true;
                foreach (var child in Children.Where(x => x != null))
                {
                    result &= child.UpdateNodeRedundancy();
                }

                if (result)
                {
                    IsRedundant = true;
                    return true;
                }
                else
                {
                    IsRedundant = false;
                    return IsLoaded;
                }
            }
        }

        protected void SetSubtreeRedundancy(bool redundant)
        {
            IsRedundant = redundant;
            foreach (var child in Children.Where(x => x != null))
            {
                child.SetSubtreeRedundancy(redundant);
            }
        }

        public void UpdateNode(Action callback = null)
        {
            if (!IsUpdating)
            {
                IsUpdating = true;

                if (IsRequired)
                {
                    LoadTile(() =>
                    {
                        IsUpdating = false;
                        callback?.Invoke();
                    });
                }
                else
                {
                    UnloadTile(() =>
                    {
                        IsUpdating = false;
                        callback?.Invoke();
                    });
                }
            }

            foreach (var child in Children.Where(x => x != null))
            {
                child.UpdateNode();
            }
        }

        protected void LoadTile(Action callback)
        {
            if (IsLoaded)
            {
                callback?.Invoke();
                return;
            }

            if (TileLoader == null)
                throw new Exception("TileLoader must be assigned!");

            TileLoader.LoadTile(this, success =>
            {
                IsLoaded = success;
                callback?.Invoke();
            });
        }

        protected void UnloadTile(Action callback)
        {
            if (!IsLoaded)
            {
                callback?.Invoke();
                return;
            }

            if (TileLoader == null)
                throw new Exception("TileLoader must be assigned!");

            TileLoader.UnloadTile(this, success =>
            {
                IsLoaded = !success;
                callback?.Invoke();
            });
        }

        public bool IsUnloadable()
        {
            return IsRedundant;
            //if (IsRequired)
            //    return false;

            //if (Parent != null && Parent.IsRequired && Parent.IsLoaded)
            //    return true;

            //var children = Children.Where(x => x != null).ToList();
            //return children.Any() && children.All(x => (x.IsRequired && x.IsLoaded) || x.IsUnloadable());
        }
    }
}