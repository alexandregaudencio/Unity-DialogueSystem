using System;
using UnityEngine;

namespace DialogueSystem.Editor.Data.Save
{
    using DialogueSystem.Utilities;

    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField, ReadOnly] public string Text { get; set; }
        [field: SerializeField, ReadOnly] public string NodeID { get; set; }
    }
}