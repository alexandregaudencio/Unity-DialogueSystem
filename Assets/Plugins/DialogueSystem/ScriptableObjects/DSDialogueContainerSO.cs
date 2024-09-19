using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    using System;
    using Utilities;
    public class DSDialogueContainerSO : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public string FileName { get; set; }
        [field: SerializeField, ReadOnly] public SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>> DialogueGroups { get; set; }
        [field: SerializeField, ReadOnly] public List<DSDialogueSO> UngroupedDialogues { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            DialogueGroups = new SerializableDictionary<DSDialogueGroupSO, List<DSDialogueSO>>();
            UngroupedDialogues = new List<DSDialogueSO>();
        }

        public List<string> GetDialogueGroupNames()
        {
            List<string> dialogueGroupNames = new List<string>();

            foreach (DSDialogueGroupSO dialogueGroup in DialogueGroups.Keys)
            {
                dialogueGroupNames.Add(dialogueGroup.GroupName);
            }

            return dialogueGroupNames;
        }

        public List<DSDialogueSO> GetGroupedDialogue(DSDialogueGroupSO dialogueGroupSO)
        {
            dialogueGroupSO.FirstDialogueSO = GetFristDialogue(dialogueGroupSO);
            return DialogueGroups[dialogueGroupSO];
        }
        public DSDialogueSO GetFristDialogue(DSDialogueGroupSO dialogueGroupSO)
        {
            foreach (DSDialogueSO dialogueSO in DialogueGroups[dialogueGroupSO])
            {
                if (dialogueSO.IsStartingDialogue) return dialogueSO;
            }
            throw new NullReferenceException($"Dialogue Groups {dialogueGroupSO.name} there isn't a starting dialogue");
        }

        public List<string> GetGroupedDialogueNames(DSDialogueGroupSO dialogueGroup, bool startingDialoguesOnly)
        {
            List<DSDialogueSO> groupedDialogues = GetGroupedDialogue(dialogueGroup);

            List<string> groupedDialogueNames = new List<string>();

            foreach (DSDialogueSO dialogueSO in groupedDialogues)
            {
                if (startingDialoguesOnly && !dialogueSO.IsStartingDialogue)
                {
                    continue;
                }

                groupedDialogueNames.Add(dialogueSO.DialogueName);
            }

            return groupedDialogueNames;
        }




        public List<string> GetUngroupedDialogueNames(bool startingDialoguesOnly)
        {
            List<string> ungroupedDialogueNames = new List<string>();

            foreach (DSDialogueSO dialogueSO in UngroupedDialogues)
            {
                if (startingDialoguesOnly && !dialogueSO.IsStartingDialogue)
                {
                    continue;
                }

                ungroupedDialogueNames.Add(dialogueSO.DialogueName);
            }
            return ungroupedDialogueNames;
        }
    }
}