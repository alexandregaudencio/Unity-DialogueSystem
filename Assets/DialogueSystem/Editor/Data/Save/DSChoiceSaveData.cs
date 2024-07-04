using System;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    using CustomAttributes;

    [Serializable]
    public class DSChoiceSaveData
    {
        [field: SerializeField, ReadOnly] public string Text { get; set; }
        [field: SerializeField, ReadOnly] public string NodeID { get; set; }
    }
}