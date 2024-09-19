using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Editor.Data.Save
{
    using DialogueSystem.Utilities;
    using Enumerations;

    [Serializable]
    public class DSNodeSaveData
    {
        [field: SerializeField, ReadOnly] public string ID { get; set; }
        [field: SerializeField, ReadOnly] public string Name { get; set; }
        [field: SerializeField, ReadOnly] public string Text { get; set; }
        [field: SerializeField, ReadOnly] public List<DSChoiceSaveData> Choices { get; set; }
        [field: SerializeField, ReadOnly] public string GroupID { get; set; }
        [field: SerializeField, ReadOnly] public DSDialogueType DialogueType { get; set; }
        [field: SerializeField, ReadOnly] public DSActor Actor { get; set; }
        [field: SerializeField, ReadOnly] public string _SpeechAnimation { get; set; }
        [field: SerializeField, ReadOnly] public Vector2 Position { get; set; }
    }
}