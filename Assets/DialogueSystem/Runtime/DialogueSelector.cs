using UnityEngine;
using UnityEngine.Events;

namespace DialogueSystem
{
    using ScriptableObjects;

    public class DialogueSelector : MonoBehaviour
    {
        [SerializeField] private DSDialogueContainerSO dialogueContainer;
        [SerializeField] private DSDialogueGroupSO dialogueGroup;
        [SerializeField] private bool groupedDialogues;
        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;
        [SerializeField] private DSDialogueSO targetDialogue;

        [SerializeField] private UnityEvent onTextRequested;

        public DSDialogueSO TargetDialogue => targetDialogue;


        void OnEnable()
        {
            SubscribeEventOnTextRequest();
        }

        void OnDisable()
        {
            UnsubscribeEventOnTextRequest();
        }


        private void SubscribeEventOnTextRequest()
        {
            targetDialogue.TextRequested += () => onTextRequested?.Invoke();
        }
        private void UnsubscribeEventOnTextRequest()
        {
            targetDialogue.TextRequested -= () => onTextRequested?.Invoke();
        }




    }
}