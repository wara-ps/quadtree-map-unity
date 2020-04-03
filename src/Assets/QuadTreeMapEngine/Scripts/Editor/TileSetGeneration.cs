using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using QuadTreeMapEngine.Data;
using UnityEditor;
using UnityEngine;

namespace QuadTreeMapEngine.Editor
{
    public static class TileSetGeneration
    {
        private static readonly string BaseFolder = "c:/srv/maps/granso";
        private static readonly Regex Pattern = new Regex(@".*L([0-9]+)_(-?[0-9]+)_(-?[0-9]+).*");

        /// <summary>
        /// Generate the metadata.xml file describing the Gränsö tileset by reading
        /// the contents of the hard-coded tile folder to construct the
        /// datastructure. The result is written to file.
        /// </summary>
        [MenuItem("QuadTreeMap/Generate metadata")]
        private static void GenerateMetadata()
        {
            try
            {
                var tilefolder = Path.Combine(BaseFolder, "tiles");
                var meshfiles = Directory.GetFiles(tilefolder, "*.obj", SearchOption.TopDirectoryOnly);

                Debug.Log(BaseFolder);

                var tiles = meshfiles.Select(GenerateTile).ToList();

                var metadata = new MapMetadata
                {
                    UtmProjection = new UtmProjection { Zone = 33, North = true },
                    Position = new Double2(600160.0, 6403336.0),
                    Rotation = new Double3(-90, 180, 0),
                    AnchorOffset = new Double2(250, 350),
                    BaseTileSize = 120,
                    MaxLevel = tiles.Max(x => x.Level),
                    Tiles = tiles
                };

                var xml = MapMetadata.WriteXml(metadata);
                Debug.Log(xml);

                var metafile = Path.Combine(BaseFolder, "metadata.xml");
                File.WriteAllText(metafile, xml, Encoding.UTF8);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                Debug.LogError(e.InnerException);
                throw;
            }
        }

        private static TileMetadata GenerateTile(string path)
        {
            var fn = Path.GetFileNameWithoutExtension(path);
            var match = Pattern.Match(fn ?? "");
            if (!match.Success)
            {
                Debug.LogError("Failed to match tile data: " + fn);
                return null;
            }

            var level = int.Parse(match.Groups[1].Value);
            var x = int.Parse(match.Groups[2].Value);
            var y = int.Parse(match.Groups[3].Value);

            var meshfile = Path.GetFileName(path);
            // todo: support for multiple textures per mesh
            var texturefile = $"L{level}_{x}_{y}_0.jpg";
            var bundlefile = $"tile_{level}_{x}_{y}.bundle";

            var data = new TileMetadata
            {
                Level = level,
                X = x,
                Y = y,
                MeshFile = meshfile,
                TextureFile = texturefile,
                BundleFile = bundlefile
            };

            return data;
        }

        /// <summary>
        /// Walk through the set of tiles, assigning the meshes and the textures to
        /// the correct asset bundle, depending on the name of the asset.
        /// If the user has some assets selected when calling this, only these are
        /// processed, otherwise all assets under "Assets/Tiles" are used.
        /// </summary>
        [MenuItem("QuadTreeMap/Assign asset bundles")]
        private static void GenerateAssetbundles()
        {
            var guids = Selection.assetGUIDs;
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets(null, new[] { "Assets/Tiles" });
            }

            foreach (var guid in guids)
            {
                var filepath = AssetDatabase.GUIDToAssetPath(guid);
                var match = Pattern.Match(filepath ?? "");
                if (!match.Success)
                {
                    Debug.LogError("Failed to match tile data: " + filepath);
                    continue;
                }

                var level = int.Parse(match.Groups[1].Value);
                var x = int.Parse(match.Groups[2].Value);
                var y = int.Parse(match.Groups[3].Value);
                var ext = match.Groups[4].Value;

                var bundlename = $"tile_{level}_{x}_{y}.bundle";

                Debug.Log(filepath + " " + level + " " + x + " " + y + " " + ext + "  " + bundlename);

                var importer = AssetImporter.GetAtPath(filepath);
                importer.SetAssetBundleNameAndVariant(bundlename, "");
            }
        }

        /// <summary>
        /// Build and export the asset bundles containing the tile meshes and textures.
        /// </summary>
        [MenuItem("QuadTreeMap/Build asset bundles")]
        private static void BuildAssetBundles()
        {
            var folder = "AssetBundles";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            BuildPipeline.BuildAssetBundles(folder, BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
            Debug.Log("Done.");
        }
    }
}
