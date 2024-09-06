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
    private bool isDialogueHappening;

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


    public GameObject rightGroup;
    List<string> actorsOnRightGroup = new List<string>(); // Inicializa uma lista vazia

    private Vector3 originalScale = new Vector3(1.4f, 1.4f, 1); // modificar para ser dinamico

    private void Start()
    {
        dialogueUI.SetActive(false);
        dialogueGroupSelector = GetComponent<DialogueGroupSelector>();
    }

    private void OnEnable()
    {
        if (currentDialogue != null) currentDialogue = dialogueGroupSelector.targetDialogue;

        if (dialogueUI.activeSelf)
        {
            InitializeDialogueUI();
        }
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

        Debug.Log($"Dialogo de {newActor}");
        //*  Quando o new actor for diferente do actor default (player)
        if (newActor != dialogueGroupSelector.ActorsOnDialogue[0].ToString())
        {

            SetOnRightGroup(listeningCharacter, newActor, currentDialogue.speechAnimation, listenerNameText);
        }
        else
        {
            SetCharacterState(talkingCharacter, newActor, currentDialogue.speechAnimation, characterNameText); //* O primeiro character a falar nesse caso esta sendo considerado o player, que fica ao lado esquerdo
        }
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
        characterAnimator.gameObject.name = characterName;
        characterAnimator.Play(animation);
        dialogueText.alignment = TextAlignmentOptions.Left;
        PlayPopAnimation(nameText);

        foreach (Transform child in rightGroup.transform) //* coloca todos os outros em listening
        {
            child.GetComponent<Animator>().Play($"{child.gameObject.name} listening");
        }
    }


    private void SetOnRightGroup(Animator characterAnimator, string characterName, string animation, TMP_Text nameText)
    {
        talkingCharacter.Play($"{talkingCharacter.gameObject.name} listening");
        actorsOnRightGroup.Append(characterName);
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
                    child.gameObject.name = characterName;
                    Animator childAnimator = child.GetComponent<Animator>(); // Atualiza o Animator com o personagem atual e toca a animação correspondente
                    childAnimator.Play(animation);
                    // Sai do loop ao encontrar e ativar o primeiro filho desativado
                    break;
                    //* PARAR TODAS AS ANIMAÇÕES MENOS A DO CHARACTER ATUAL
                }
            }
        }
        foreach (Transform child in rightGroup.transform)
        {
            if (child.gameObject.name != characterName)
            {
                Animator childAnimator = child.GetComponent<Animator>(); // Atualiza o Animator com o personagem atual e toca a animação correspondente
                childAnimator.Play($"{child.gameObject.name} listening");

            }
            else
            {
                Animator childAnimator = child.GetComponent<Animator>(); // Atualiza o Animator com o personagem atual e toca a animação correspondente
                childAnimator.Play(animation);
            }
        }
    }

    public void StartDialogue()
    {
        dialogueUI.SetActive(true);
        isDialogueHappening = true;
        currentDialogue = dialogueGroupSelector.targetDialogue;
        InitializeDialogueUI();
    }

    private void InitializeDialogueUI()
    {
        dialogueText.text = currentDialogue.RequestText;
        characterNameText.text = currentDialogue.Actor.ToString();
        PlayPopAnimation(characterNameText);

        if (currentDialogue != null)
        {
            talkingCharacter.Play(currentDialogue.speechAnimation);
        }

        if (dialogueGroupSelector.ActorsOnDialogue.Count >= 1)
        {
            string firstListener = dialogueGroupSelector.ActorsOnDialogue[1].ToString();
            Debug.Log($"Primeiro personagem escutando: {firstListener}");
            listeningCharacter.Play("listening");

        }
    }

    private void FinishDialogue()
    {
        dialogueUI.SetActive(false);
        isDialogueHappening = false;
        actorsOnRightGroup.Clear();
        dialogueText.text = "";
        listenerNameText.text = "";
        characterNameText.text = "";
        foreach (Transform child in rightGroup.transform)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }

    }
}
