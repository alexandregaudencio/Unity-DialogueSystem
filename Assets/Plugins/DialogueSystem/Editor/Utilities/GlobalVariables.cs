using System.IO;
using UnityEngine;

namespace DialogueSystem.Editor.Utilities
{
    public static class GlobalVariables
    {

        public static string MainPath => "Assets/Plugins/DialogueSystem/";
        public static string DialoguesPath => "Assets/Resources/Dialogues";
        public static string StylePath => string.Concat(MainPath, "Editor/Styles/");
        public static string DialogueGraphsPath => string.Concat(DialoguesPath, "/Graphs");
        public static string ActorEnumPath => string.Concat(MainPath, "/Runtime/Enumerations/DSActor.cs");

        private static string FindRelativeFolderPath(string folderName)
        {
            foreach (string directory in Directory.GetDirectories(Application.dataPath, "*", SearchOption.AllDirectories))
            {
                if (new DirectoryInfo(directory).Name == folderName)
                {
                    string relativePath = "Assets" + directory.Replace(Application.dataPath, "").Replace("\\", "/");
                    Debug.Log(relativePath);
                    return relativePath;
                }
            }

            return null;
        }

    }

}
