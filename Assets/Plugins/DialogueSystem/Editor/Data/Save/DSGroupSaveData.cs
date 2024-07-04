using System;
using UnityEngine;

namespace DialogueSystem.Data.Save
{
    using CustomAttributes  ;
    [Serializable]
    public class DSGroupSaveData
    {
        [field: SerializeField, ReadOnly] public string ID { get; set; }
        [field: SerializeField, ReadOnly] public string Name { get; set; }
        [field: SerializeField, ReadOnly] public Vector2 Position { get; set; }
    }
}