using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DialogueSystem.Inspectors
{
    using Utilities;
    using ScriptableObjects;
    [CustomEditor(typeof(DialogueGroupSelector))]
    public class DialogueGroupSelectorEditor : Editor
    {
        private SerializedProperty dialogueContainerProperty;
        private SerializedProperty dialogueGroupProperty;
        private SerializedProperty dialogueProperty;
        private SerializedProperty selectedDialogueGroupIndexProperty;
        private SerializedProperty selectedDialogueIndexProperty;
        private SerializedProperty onDialogueTextRequestedProperty;


        private void OnEnable()
        {
            FindReferences();
        }

        private void FindReferences()
        {
            dialogueContainerProperty = serializedObject.FindProperty("dialogueContainer");
            dialogueGroupProperty = serializedObject.FindProperty("dialogueGroup");
            dialogueProperty = serializedObject.FindProperty("dialogue");

            selectedDialogueGroupIndexProperty = serializedObject.FindProperty("selectedDialogueGroupIndex");
            selectedDialogueIndexProperty = serializedObject.FindProperty("selectedDialogueIndex");
            onDialogueTextRequestedProperty = serializedObject.FindProperty("onDialogueTextRequested");
        }

        private void DrawOnTextRequestEvents()
        {
            DSDialogueContainerSO dialogueContainer = (DSDialogueContainerSO)dialogueContainerProperty.objectReferenceValue;
            DSDialogueGroupSO dialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;
            List<DSDialogueSO> dialogues = dialogueContainer.GetGroupedDialogue(dialogueGroup);
            onDialogueTextRequestedProperty.arraySize = dialogues.Count;

            int listSize = onDialogueTextRequestedProperty.arraySize;
            string groupName = dialogueGroupProperty.objectReferenceValue.name;
            EditorGUILayout.LabelField($"Dialogue {groupName.ToUpper()} events", EditorStyles.boldLabel);

            for (int i = 0; i < listSize; i++)
            {
                SerializedProperty elementProperty = onDialogueTextRequestedProperty.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(elementProperty, new GUIContent(dialogues[i].Text));
            }




        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDialogueContainerArea();
            DSDialogueContainerSO currentDialogueContainer = (DSDialogueContainerSO)dialogueContainerProperty.objectReferenceValue;

            if (currentDialogueContainer == null)
            {
                StopDrawing("Select a Dialogue Container to see the rest of the Inspector.");

                return;
            }

            List<string> dialogueNames;
            string dialogueFolderPath = $"{GlobalVariables.DialoguesPath}/{currentDialogueContainer.FileName}";
            string dialogueInfoMessage;
            List<string> dialogueGroupNames = currentDialogueContainer.GetDialogueGroupNames();

            if (dialogueGroupNames.Count == 0)
            {
                StopDrawing("There are no Dialogue Groups in this Dialogue Container.");

                return;
            }

            DrawDialogueGroupArea(currentDialogueContainer, dialogueGroupNames);
            DSDialogueGroupSO dialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;
            dialogueNames = currentDialogueContainer.GetGroupedDialogueNames(dialogueGroup, true);
            dialogueFolderPath += $"/Groups/{dialogueGroup.GroupName}/Dialogues";
            dialogueInfoMessage = "There are nodialogue starting in this Dialogue Group.";
            if (dialogueNames.Count == 0)
            {
                StopDrawing(dialogueInfoMessage);
                return;
            }

            DrawDialogueArea(dialogueNames, dialogueFolderPath);
            DSInspectorUtility.DrawSpace();
            DrawOnTextRequestEvents();
            serializedObject.ApplyModifiedProperties();

        }

        private void DrawDialogueContainerArea()
        {
            DSInspectorUtility.DrawHeader("Dialogue Container");
            dialogueContainerProperty.DrawPropertyField();
            DSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueGroupArea(DSDialogueContainerSO dialogueContainer, List<string> dialogueGroupNames)
        {
            DSInspectorUtility.DrawHeader("Dialogue Group");

            int oldSelectedDialogueGroupIndex = selectedDialogueGroupIndexProperty.intValue;

            DSDialogueGroupSO oldDialogueGroup = (DSDialogueGroupSO)dialogueGroupProperty.objectReferenceValue;

            bool isOldDialogueGroupNull = oldDialogueGroup == null;

            string oldDialogueGroupName = isOldDialogueGroupNull ? "" : oldDialogueGroup.GroupName;

            UpdateIndexOnNamesListUpdate(dialogueGroupNames, selectedDialogueGroupIndexProperty, oldSelectedDialogueGroupIndex, oldDialogueGroupName, isOldDialogueGroupNull);

            selectedDialogueGroupIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue Group", selectedDialogueGroupIndexProperty, dialogueGroupNames.ToArray());

            string selectedDialogueGroupName = dialogueGroupNames[selectedDialogueGroupIndexProperty.intValue];

            DSDialogueGroupSO selectedDialogueGroup = DSIOUtility.LoadAsset<DSDialogueGroupSO>($"{GlobalVariables.DialoguesPath}/{dialogueContainer.FileName}/Groups/{selectedDialogueGroupName}", selectedDialogueGroupName);

            dialogueGroupProperty.objectReferenceValue = selectedDialogueGroup;

            DSInspectorUtility.DrawDisabledFields(() => dialogueGroupProperty.DrawPropertyField());

            //DSInspectorUtility.DrawSpace();
        }

        private void DrawDialogueArea(List<string> dialogueNames, string dialogueFolderPath)
        {
            //The Dialogue Group can has more than one dialogue sequence (more than one dialogue starting)
            DSInspectorUtility.DrawHeader("Target Dialogue Starting");

            int oldSelectedDialogueIndex = selectedDialogueIndexProperty.intValue;

            DSDialogueSO oldDialogue = (DSDialogueSO)dialogueProperty.objectReferenceValue;

            bool isOldDialogueNull = oldDialogue == null;

            string oldDialogueName = isOldDialogueNull ? "" : oldDialogue.DialogueName;

            UpdateIndexOnNamesListUpdate(dialogueNames, selectedDialogueIndexProperty, oldSelectedDialogueIndex, oldDialogueName, isOldDialogueNull);

            selectedDialogueIndexProperty.intValue = DSInspectorUtility.DrawPopup("Dialogue", selectedDialogueIndexProperty, dialogueNames.ToArray());

            string selectedDialogueName = dialogueNames[selectedDialogueIndexProperty.intValue];

            DSDialogueSO selectedDialogue = DSIOUtility.LoadAsset<DSDialogueSO>(dialogueFolderPath, selectedDialogueName);

            dialogueProperty.objectReferenceValue = selectedDialogue;

            DSInspectorUtility.DrawDisabledFields(() => dialogueProperty.DrawPropertyField());
        }

        private void StopDrawing(string reason, MessageType messageType = MessageType.Info)
        {
            DSInspectorUtility.DrawHelpBox(reason, messageType);

            DSInspectorUtility.DrawSpace();

            DSInspectorUtility.DrawHelpBox("You need to select a Dialogue for this component to work properly at Runtime!", MessageType.Warning);

            serializedObject.ApplyModifiedProperties();
        }

        private void UpdateIndexOnNamesListUpdate(List<string> optionNames, SerializedProperty indexProperty, int oldSelectedPropertyIndex, string oldPropertyName, bool isOldPropertyNull)
        {
            if (isOldPropertyNull)
            {
                indexProperty.intValue = 0;

                return;
            }

            bool oldIndexIsOutOfBoundsOfNamesListCount = oldSelectedPropertyIndex > optionNames.Count - 1;
            bool oldNameIsDifferentThanSelectedName = oldIndexIsOutOfBoundsOfNamesListCount || oldPropertyName != optionNames[oldSelectedPropertyIndex];

            if (oldNameIsDifferentThanSelectedName)
            {
                if (optionNames.Contains(oldPropertyName))
                {
                    indexProperty.intValue = optionNames.IndexOf(oldPropertyName);

                    return;
                }

                indexProperty.intValue = 0;
            }
        }


    }
}