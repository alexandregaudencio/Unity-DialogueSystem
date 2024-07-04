using UnityEngine;

namespace DialogueSystem.ScriptableObjects
{
    using CustomAttributes;
    public class DSDialogueGroupSO : ScriptableObject
    {
        [field: SerializeField, ReadOnly] public string GroupName { get; set; }
        [field: SerializeField, ReadOnly] public DSDialogueSO FirstDialogueSO { get; set; }

        public void Initialize(string groupName)
        {
            GroupName = groupName;
        }



    }
}