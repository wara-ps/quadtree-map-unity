using UnityEditor;

namespace QuadTreeMapEngine.Editor
{
    [CustomEditor(typeof(MapTile))]
    public class MapTileEditor : UnityEditor.Editor
    {
        private MapTile Target { get; set; }

        private void OnEnable()
        {
            Target = (MapTile)target;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Coordinate: {Target.Node.X}, {Target.Node.Y}");
            EditorGUILayout.LabelField($"Position: {Target.Node.Center.X}, {Target.Node.Center.Y}");
            EditorGUILayout.Space();
            EditorGUILayout.DoubleField("Player Distance", Target.Node.Distance);
            EditorGUILayout.LabelField("Is Required", $"{Target.Node.IsRequired}");
            EditorGUILayout.LabelField("Is Updating", $"{Target.Node.IsUpdating}");
            EditorGUILayout.LabelField("Is Loaded", $"{Target.Node.IsLoaded}");
            EditorGUILayout.LabelField("Is Unloadable", $"{Target.Node.IsUnloadable()}");
        }
    }
}
