using UnityEngine;
using System.Collections.Generic;

namespace DialogueSystem
{
    using System;
    using DialogueSystem.Enumerations;
    using ScriptableObjects;
    using UnityEngine.Events;

    public class DialogueGroupSelector : MonoBehaviour
    {
        [SerializeField] private DSDialogueContainerSO dialogueContainer;
        [SerializeField] private DSDialogueGroupSO dialogueGroup;
        [SerializeField] private DSDialogueSO dialogue;
        [SerializeField] private bool startingDialoguesOnly;
        public List<DSActor> ActorsOnDialogue = new List<DSActor>();

        [SerializeField] private int selectedDialogueGroupIndex;
        [SerializeField] private int selectedDialogueIndex;

        public DSDialogueSO Dialogue { get => dialogue; private set => dialogue = value; }
        public List<DSDialogueSO> DialogueGroupTarget = new List<DSDialogueSO>();
        public DSDialogueGroupSO targetGroup => dialogueGroup;
        public DSDialogueSO targetDialogue => dialogue;

        [SerializeField] private List<UnityEvent> onDialogueTextRequested;
        public DSActor firstListener;
        public DSActor firstTalking;



        private void Awake()
        {
            DialogueGroupTarget = dialogueContainer.GetGroupedDialogue(dialogueGroup);


        }

        void OnEnable()
        {
            SubscribeDialogueEvents();
            characterList();
        }

        void OnDisable()
        {
            UnsubscribeDialogueEvents();
        }



        private void SubscribeDialogueEvents()
        {
            for (int i = 0; i < DialogueGroupTarget.Count; i++)
            {
                int index = i;
                //TODO: subscribe method. Not expression
                DialogueGroupTarget[index].TextRequested += () => onDialogueTextRequested[index].Invoke();
            }
        }

        private void UnsubscribeDialogueEvents()
        {
            for (int i = 0; i < DialogueGroupTarget.Count; i++)
            {
                int index = i;
                //TODO: unsubscribe as method. Not expression
                DialogueGroupTarget[index].TextRequested -= () => onDialogueTextRequested[index].Invoke();
            }
        }


        [ContextMenu("Reset Events")]
        public void ResetTextRequestEvents()
        {
            foreach (UnityEvent events in onDialogueTextRequested)
            {
                events.RemoveAllListeners();

            }
        }

        // *define the first character to speak and define it as player 
        //* definir o segundo personagem na lista como quem esta na ordem para escutar 


        public void characterList()
        {

            for (int i = 0; i < DialogueGroupTarget.Count; i++)
            {
                DSActor currentActor = DialogueGroupTarget[i].Actor;
                Debug.Log($"Actor on dialogue {currentActor}");
                // Verifica se o ator já foi adicionado à lista
                if (!ActorsOnDialogue.Contains(currentActor))
                {
                    ActorsOnDialogue.Add(currentActor);
                }
            }
            ActorsOnDialogue.Reverse();
            firstTalking = ActorsOnDialogue[0];
            firstListener = ActorsOnDialogue[1];
        }
    }
}