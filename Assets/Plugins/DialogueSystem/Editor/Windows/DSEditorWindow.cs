using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Utilities;

    public class DSEditorWindow : EditorWindow
    {
        private DSGraphView graphView;

        private readonly string defaultFileName = "DialoguesFileName";

        private static TextField fileNameTextField;
        private Button saveButton;
        private Button miniMapButton;
        private Button actorsButton;
        private string lastLoadedFilePath = "";

        [MenuItem("Tools/Dialogue Graph")]
        public static void Open()
        {
            GetWindow<DSEditorWindow>("Dialogue Graph");
        }

        private void OnEnable()
        {
            AddGraphView();
            AddToolbar();

            AddStyles();
        }

        private void AddGraphView()
        {
            graphView = new DSGraphView(this);

            graphView.StretchToParentSize();

            rootVisualElement.Add(graphView);
        }

        private void AddToolbar()
        {
            Toolbar toolbar = new Toolbar();

            fileNameTextField = DSElementUtility.CreateTextField(defaultFileName,"", callback =>
            {
                fileNameTextField.value = callback.newValue.RemoveWhitespaces().RemoveSpecialCharacters();
            });

            saveButton = DSElementUtility.CreateButton("Save", () => Save());

            Button loadButton = DSElementUtility.CreateButton("Load", OpenLoadFilePanel);
            Button clearButton = DSElementUtility.CreateButton("Clear", Clear);
            Button resetButton = DSElementUtility.CreateButton("Reset", ResetGraph);
            TextElement tf = DSElementUtility.CreateTextElement("               ");

            miniMapButton = DSElementUtility.CreateButton("Minimap",ToggleMiniMap);
            actorsButton = DSElementUtility.CreateButton("Actor", Actors);

            toolbar.Add(fileNameTextField);
            toolbar.Add(saveButton);
            toolbar.Add(loadButton);
            toolbar.Add(clearButton);
            toolbar.Add(resetButton);
            toolbar.Add(tf);
            toolbar.Add(miniMapButton);
            toolbar.Add(actorsButton);

            toolbar.AddStyleSheets("DSToolbarStyles.uss");

            rootVisualElement.Add(toolbar);
        }

        private void AddStyles()
        {
            rootVisualElement.AddStyleSheets("DSVariables.uss");
        }

        private void Save()
        {
            if (string.IsNullOrEmpty(fileNameTextField.value))
            {
                EditorUtility.DisplayDialog("Invalid file name.", "Please ensure the file name you've typed in is valid.", "Roger!");

                return;
            }

            DSIOUtility.Initialize(graphView, fileNameTextField.value);
            DSIOUtility.Save();
        }

        private void OpenLoadFilePanel()
        {
            string filePath = EditorUtility.OpenFilePanel("Dialogue Graphs", GlobalVariables.DialogueGraphsPath, "asset");
            Load(filePath);
        }

        private void Load(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))  return;    
            Clear();
            DSIOUtility.Initialize(graphView, Path.GetFileNameWithoutExtension(filePath));
            DSIOUtility.StartLoad();
            lastLoadedFilePath = filePath;
        }

        private void Actors()
        {
            DSActorWindow.OpenWindow();
        }

        private void Clear()
        {
            graphView.ClearGraph();
        }

        private void ResetGraph()
        {
            Clear();

            UpdateFileName(defaultFileName);
            Load(lastLoadedFilePath);
        }

        private void ToggleMiniMap()
        {
            graphView.ToggleMiniMap();

            miniMapButton.ToggleInClassList("ds-toolbar__button__selected");
        }

        public static void UpdateFileName(string newFileName)
        {
            fileNameTextField.value = newFileName;
        }

        public void EnableSaving()
        {
            saveButton.SetEnabled(true);
        }

        public void DisableSaving()
        {
            saveButton.SetEnabled(false);
        }


    }
}