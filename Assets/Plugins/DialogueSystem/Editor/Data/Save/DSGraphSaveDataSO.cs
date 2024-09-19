using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Editor.Data.Save
{
    using DialogueSystem.Utilities;
    using System;

    public class DSGraphSaveDataSO : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public string FileName { get; set; }
        [field: SerializeField, ReadOnly] public List<DSGroupSaveData> Groups { get; set; }
        [field: SerializeField, ReadOnly] public List<DSNodeSaveData> Nodes { get; set; }
        [field: SerializeField, ReadOnly] public List<string> OldGroupNames { get; set; }
        [field: SerializeField, ReadOnly] public List<string> OldUngroupedNodeNames { get; set; }
        [field: SerializeField, ReadOnly] public SerializableDictionary<string, List<string>> OldGroupedNodeNames { get; set; }

        public void Initialize(string fileName)
        {
            FileName = fileName;

            Groups = new List<DSGroupSaveData>();
            Nodes = new List<DSNodeSaveData>();
        }
    }
}