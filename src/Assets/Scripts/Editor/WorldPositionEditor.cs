using UnityEditor;

[CustomEditor(typeof(WorldPosition))]
public class WorldPositionEditor : Editor
{
    private WorldPosition Target { get; set; }

    private void OnEnable()
    {
        Target = (WorldPosition)target;
    }

    public override void OnInspectorGUI()
    {
        var pos = Target.CurrentPosition;
        EditorGUILayout.LabelField($"Position: ({pos.Lat:0.######} N, {pos.Lng:0.######} E)");
    }
}
