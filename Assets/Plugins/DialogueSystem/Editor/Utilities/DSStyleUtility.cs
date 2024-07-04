using UnityEditor;
using UnityEngine.UIElements;

namespace DialogueSystem.Utilities
{
    public static class DSStyleUtility
    {
        public static VisualElement AddClasses(this VisualElement element, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                element.AddToClassList(className);
            }

            return element;
        }

        public static VisualElement AddStyleSheets(this VisualElement element, params string[] styleSheetNames)
        {
            foreach (string styleSheetName in styleSheetNames)
            {
                string styleSheetPath = string.Concat(GlobalVariables.StylePath, styleSheetName);
                
                StyleSheet styleSheet = (StyleSheet) EditorGUIUtility.Load(styleSheetPath);
                element.styleSheets.Add(styleSheet);
            }
            //"Assets/DialogSystem/Editor Default Resources/DialogueSystem/DSGraphViewStyles.uss"
            return element;
        }
    }
}