using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PathTool))]
public class PathToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        PathTool tool = (PathTool)target;

        GUI.backgroundColor = Color.cyan;

        if (GUILayout.Button("Rebuild Colliders"))
        {
            tool.RebuildColliders();
        }

        GUI.backgroundColor = Color.white;
    }
}