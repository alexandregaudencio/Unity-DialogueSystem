using System;
using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{
    private bool isDialogueHappening = false;

    [Header("References")]
    [SerializeField] private CharacterDialogueAnimations characterAnimations;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text listenerNameText;
    [SerializeField] private CanvasGroup canvasGroup;
    private DialogueGroupSelector dialogueGroupSelector;
    private DSDialogueSO currentDialogue;

    [Header("Dialogue Animation Settings")]
    [SerializeField] private Animator talkingCharacter;
    [SerializeField] private Animator listeningCharacter;
    [SerializeField] private float popDuration = 0.2f;
    [SerializeField] private float popScale = 1.2f;


    public event Action<string, string> OnDialogueChanged; // evento pra disparar toda vez que o dialogo muda


    public GameObject rightGroup;
    List<string> actorsOnRightGroup = new List<string>(); // Inicializa uma lista vazia
    private Vector3 originalScale = new Vector3(1.4f, 1.4f, 1); // modificar para ser dinamico

    // iniciar o primeiro current dialogue
    private void Start()
    {
        dialogueUI.SetActive(false);
        dialogueGroupSelector = GetComponent<DialogueGroupSelector>();
        // currentDialogue = dialogueGroupSelector.targetDialogue;
        if (currentDialogue == null) currentDialogue = dialogueGroupSelector.targetDialogue;
    }

    private void OnEnable()
    {
        // Debug.Log($"{currentDialogue.Actor.ToString()}, {currentDialogue.speechAnimation}"); //* Chega corretamente
        if (dialogueUI.activeSelf && currentDialogue != null)
        {
            InitializeDialogueUI(currentDialogue);
        }
    }

    public void StartDialogue()
    {
        dialogueUI.SetActive(true);
        isDialogueHappening = true;
        currentDialogue = dialogueGroupSelector.targetDialogue;
        // Debug.Log($"current dialogue: {currentDialogue.Actor.ToString()}"); // chegando corretamente
        InitializeDialogueUI(currentDialogue);
    }

    private void Update()
    {
        CheckForInput();
    }

    private void CheckForInput()
    {
        if (isDialogueHappening && Input.anyKeyDown)
        {
            if (currentDialogue.Choices[0].NextDialogue == null)
            {
                FinishDialogue();
                return;
            }
            UpdateDialogue();

        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 50, 50), "Iniciar diálogo"))
        {
            StartDialogue();
        }
    }



    private void UpdateDialogue()
    {
        currentDialogue = currentDialogue.Choices[0].NextDialogue;
        dialogueText.text = currentDialogue.RequestText;
        string newActor = currentDialogue.Actor.ToString();

        //*  Quando o new actor for diferente do actor default (player)
        if (newActor != dialogueGroupSelector.ActorsOnDialogue[0].ToString())
        {
            // Debug.Log(dialogueGroupSelector.ActorsOnDialogue[1]);
            SetOnRightGroup(listeningCharacter, newActor, currentDialogue.speechAnimation, listenerNameText);
        }
        else
        {
            SetCharacterState(talkingCharacter, newActor, currentDialogue.speechAnimation, characterNameText); //* O primeiro character a falar nesse caso esta sendo considerado o player, que fica ao lado esquerdo
        }
        OnDialogueChanged?.Invoke(newActor, currentDialogue.speechAnimation);
    }



    private void PlayPopAnimation(TMP_Text text)
    {
        text.transform.DOScale(popScale, popDuration / 2).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                text.transform.DOScale(originalScale, popDuration / 2).SetEase(Ease.InQuad);
            });
    }

    // Unificação de SetCharacterForTalking e SetCharacterForListening
    private void SetCharacterState(Animator characterAnimator, string characterName, string animation, TMP_Text nameText)
    {
        nameText.text = characterName;
        dialogueText.alignment = TextAlignmentOptions.Left;
        talkingCharacter.gameObject.GetComponent<DialogueActor>().InitializeActor(characterName, animation);
        PlayPopAnimation(nameText);
    }


    private void SetOnRightGroup(Animator characterAnimator, string characterName, string animation, TMP_Text nameText)
    {
        nameText.text = characterName;
        dialogueText.alignment = TextAlignmentOptions.Right;
        PlayPopAnimation(nameText);

        // se o ator não está no grupo adicione ele no grupo
        if (!actorsOnRightGroup.Contains(characterName))
        {
            actorsOnRightGroup.Add(characterName);
            foreach (Transform child in rightGroup.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    child.GetComponent<DialogueActor>().InitializeActor(characterName, animation);
                    break;
                }
            }
        }
    }



    private void InitializeDialogueUI(DSDialogueSO currentDialogue)
    {
        dialogueText.text = currentDialogue.RequestText;
        characterNameText.text = currentDialogue.Actor.ToString();

        PlayPopAnimation(characterNameText);
        if (currentDialogue != null)
        {
            talkingCharacter.gameObject.GetComponent<DialogueActor>().InitializeActor(currentDialogue.Actor.ToString(), currentDialogue.speechAnimation.ToString());
        }

        if (dialogueGroupSelector.ActorsOnDialogue.Count >= 1)
        {
            string firstListener = dialogueGroupSelector.ActorsOnDialogue[1].ToString();
            // Debug.Log($"First talking {dialogueGroupSelector.firstTalking}");
        }
        OnDialogueChanged?.Invoke(currentDialogue.Actor.ToString(), currentDialogue.speechAnimation);

    }

    private void FinishDialogue()
    {
        dialogueUI.SetActive(false);
        isDialogueHappening = false;
        actorsOnRightGroup.Clear();
        dialogueText.text = "";
        listenerNameText.text = "";
        characterNameText.text = "";
        // currentDialogue = currentDialogue.;
        foreach (Transform child in rightGroup.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

    }


}
