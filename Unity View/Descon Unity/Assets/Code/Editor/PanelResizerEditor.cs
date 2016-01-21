using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(PanelResizer))]
public class PanelResizerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var panelTarget = (PanelResizer)target;

        if(GUILayout.Button("Fit Panel to Camera"))
        {
            panelTarget.UpdateBounds();
        }
    }
}
