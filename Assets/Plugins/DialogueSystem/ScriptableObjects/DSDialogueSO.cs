using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    using System;
    using Enumerations;
    using CustomAttributes;

    public class DSDialogueSO : ScriptableObject
    {
        [SerializeField, ReadOnly] private string text;
        [field: SerializeField, ReadOnly] public string DialogueName { get; set; }
        [field: SerializeField, ReadOnly] public List<DSDialogueChoiceData> Choices { get; set; }
        [field: SerializeField, ReadOnly] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField, ReadOnly] public DSActor Actor { get; set; }
        [field: SerializeField, ReadOnly] public string speechAnimation { get; set; }

        [field: SerializeField, ReadOnly] public bool IsStartingDialogue { get; set; }
        public event Action TextRequested = delegate { };
        /// <summary>
        /// To simple acess to Text value of dialogue.
        /// </summary>
        public string Text => text;

        /// <summary>
        /// To Use just for show text on Interface, because the acess invoke TextRequest event.
        /// </summary>
        public string RequestText
        {
            get
            {
                TextRequested?.Invoke();
                return text;
            }
            set { text = value; }
        }

        public void Initialize(string dialogueName, string text, List<DSDialogueChoiceData> choices, DSDialogueType dialogueType, DSActor actor, bool isStartingDialogue, string SpeechAnimation)
        {
            DialogueName = dialogueName;
            RequestText = text;
            Choices = choices;
            DialogueType = dialogueType;
            Actor = actor;
            speechAnimation = SpeechAnimation;
            IsStartingDialogue = isStartingDialogue;

        }

    }





}