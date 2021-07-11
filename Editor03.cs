using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshColors03))]
public class Editor03 : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ((MeshColors03)target).RequestUpdate();
    }
}
