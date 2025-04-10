using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BlockInInspectorAttribute))]
public class BlockInInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; // disables the field
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true; // re-enable the GUI for the rest of the fields
    }
}
