using UnityEngine;
using UnityEditor;

namespace DialogueSystem.Editor.CustomAttributes
{
    using DialogueSystem.Utilities;

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;  // Desativa a edição
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;   // Reativa a edição para outros elementos
        }
    }



}
