using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Editor.Windows
{
    using Utilities;
    using Enumerations;

    public class DSActorWindow : EditorWindow
    {
        private string filePath => GlobalVariables.ActorEnumPath;
        private string enumName = nameof(DSActor);
        private string inputActorValue;

        private bool showErrorMessage;
        private string errorValue = "";

        public static void OpenWindow()
        {
            var window = GetWindow<DSActorWindow>("Actors");
            window.titleContent = new GUIContent("Actors");
        }

        private void OnGUI()
        {
            GUILayout.Space(20);
            AddActorField();
            if (showErrorMessage)
            {
                EditorGUILayout.HelpBox($"{errorValue} Already has defined an Actor.", MessageType.Warning);
            }
            GUILayout.Space(20);
            ShowActors();


        }


        private void ShowActors()
        {
            GUILayout.Label("Actors:", EditorStyles.boldLabel);
            foreach (string enumValue in Enum.GetNames(typeof(DSActor)))
            {
                GUILayout.Label(" - " + enumValue);
            }
        }

        private void AddActorField()
        {
            EditorGUILayout.BeginHorizontal();
            inputActorValue = EditorGUILayout.TextField(inputActorValue);

            if (GUILayout.Button("Add"))
            {
                AddValueToEnum(inputActorValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void AddValueToEnum(string value)
        {
            if (string.IsNullOrEmpty(value)) return;

            DSActor Actors;
            if (Enum.TryParse(value, out Actors))
            {
                showErrorMessage = true;
                errorValue = value;
                return;
            }

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                using (StreamWriter writer = new StreamWriter(filePath))
                {


                    bool insideEnum = false;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        string line = lines[i];
                        if (insideEnum && line.Contains("}"))
                        {
                            writer.WriteLine(string.Concat("        ,", value));
                            insideEnum = false;
                        }
                        writer.WriteLine(line);
                        if (line.Contains("enum " + enumName))
                        {
                            insideEnum = true;
                            showErrorMessage = false;
                        }
                    }
                }
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError("File not exist: " + filePath);
            }




        }


    }
}