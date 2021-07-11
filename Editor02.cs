using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshColors02))]
public class Editor02 : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        ((MeshColors02)target).RequestUpdate();
    }
}
