using System;
using UnityEngine;

namespace DialogueSystem.Editor.Data.Save
{
    using DialogueSystem.Utilities;
    [Serializable]
    public class DSGroupSaveData
    {
        [field: SerializeField, ReadOnly] public string ID { get; set; }
        [field: SerializeField, ReadOnly] public string Name { get; set; }
        [field: SerializeField, ReadOnly] public Vector2 Position { get; set; }
    }
}