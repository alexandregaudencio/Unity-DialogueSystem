using System;
using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{
    private bool dialogueHappening;

    [Header("References")]
    [SerializeField] CharacterDialogueAnimations characterterAnimations;
    [SerializeField] GameObject dialogueUI;
    [SerializeField] TMP_Text dialogText;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] TMP_Text listenerNameText;
    [SerializeField] CanvasGroup canvasGroup;
    public DialogueGroupSelector dialogue;
    DSDialogueSO targetDialogue;

    [Header("Dialogue Animation Settings")]
    public Animator TalkingCharacter;
    public Animator ListeningCharacter;
    public float popDuration = 0.2f;
    public float popScale = 1.2f;

    // private data
    private Animator[] talkingCharacters;
    private Vector3 originalScale;



    private void Start()
    {
        dialogueUI.SetActive(false);
        dialogue = GetComponent<DialogueGroupSelector>();

    }

    void OnEnable()
    {
        targetDialogue = dialogue.targetDialogue;


        if (dialogueUI.activeSelf)
        {
            InitializeDialogueUI();
        }
    }

    void Update()
    {
        CheckForInput();
    }

    void CheckForInput()
    {
        if (dialogueHappening && Input.anyKeyDown)
        {
            if (targetDialogue.Choices[0].NextDialogue == null)
            {
                FinishDialogue();
                return;
            }
            UpdateDialogue();
        }
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 50, 50), "Iniciar dialogo"))
            StartDialogue();
    }

    void UpdateDialogue()
    {
        targetDialogue = targetDialogue.Choices[0].NextDialogue;
        dialogText.text = targetDialogue.RequestText;
        string newActor = targetDialogue.Actor.ToString(); // recebe a informação de quem é o novo ator

        //* update this as detecting if first character to speak isnt the new actor
        if (newActor != "Thaynara")
        {
            SetCharacterForListening(newActor, targetDialogue.speechAnimation);
            listenerNameText.text = targetDialogue.Actor.ToString();
            PlayPopAnimation(listenerNameText);
            TalkingCharacter.Play("listening");
        }
        else
        {
            characterNameText.text = targetDialogue.Actor.ToString();
            SetCharacterForTalking(newActor, targetDialogue.speechAnimation);
            PlayPopAnimation(characterNameText);
            ListeningCharacter.Play("listening");
        }

    }

    private void PlayPopAnimation(TMP_Text text)
    {
        originalScale = new Vector3(1.4f, 1.4f, 1);
        text.transform.DOScale(popScale, popDuration / 2).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                text.transform.DOScale(originalScale, popDuration / 2).SetEase(Ease.InQuad);
            });
    }


    void SetCharacterForTalking(string CharacterName, string Animation)
    {
        TalkingCharacter.Play(Animation);
    }

    void SetCharacterForListening(string CharacterName, string Animation)
    {
        ListeningCharacter.Play(Animation);
    }


    public void StartDialogue()
    {
        dialogueUI.SetActive(true);
        dialogueHappening = true;
        targetDialogue = dialogue.targetDialogue;
        InitializeDialogueUI();
    }

    void InitializeDialogueUI()
    {
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();
        PlayPopAnimation(characterNameText);

        if (targetDialogue != null)
        {
            SetCharacterForTalking(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);
        }
        if (dialogue.ActorsOnDialogue.Count >= 1)
        {
            Debug.Log($" Primeiro personagem escutando {dialogue.ActorsOnDialogue[1].ToString()}");
            SetCharacterForListening(dialogue.ActorsOnDialogue[1].ToString(), "listening");
        }
    }

    void FinishDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueHappening = false;
    }

    void DarkenCharacter()
    {

    }
}
