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
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Transform talkingPos, listeningPos;
    public GameObject charactersParent;
    public DialogueGroupSelector dialogue;
    DSDialogueSO targetDialogue;

    [Header("Dialogue Animation Settings")]
    public float popDuration = 0.2f;
    public float popScale = 1.2f;

    // private data
    private Animator[] talkingCharacters;
    private Animator currentTalkingCharacter;
    private Animator lastTalkingCharacter;
    private Vector3 originalScale;



    private void Start()
    {
        dialogueUI.SetActive(false);
        dialogue = GetComponent<DialogueGroupSelector>();

        SetUpCharacters();
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
        string newActor = targetDialogue.Actor.ToString();
        PlayPopAnimation();

        lastTalkingCharacter = currentTalkingCharacter;

        targetDialogue = targetDialogue.Choices[0].NextDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();

        if (lastTalkingCharacter.gameObject.name != targetDialogue.Actor.ToString())
        {
            SetCharacterForListening(lastTalkingCharacter);
        }

        SetCharacterForDialogue(newActor, targetDialogue.speechAnimation);
    }

    private void PlayPopAnimation()
    {
        originalScale = new Vector3(1.4f, 1.4f, 1);

        characterNameText.transform.DOScale(popScale, popDuration / 2).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                characterNameText.transform.DOScale(originalScale, popDuration / 2).SetEase(Ease.InQuad);
            });
    }

    void SetCharacterForDialogue(string actorName, string speechAnimation)
    {
        currentTalkingCharacter = FindCharacterAnimatorByName(targetDialogue.speechAnimation);
        if (currentTalkingCharacter != null)
        {
            currentTalkingCharacter.gameObject.SetActive(true);
            currentTalkingCharacter.transform.position = talkingPos.position;
            currentTalkingCharacter.Play(speechAnimation, 0);
        }
    }

    void SetCharacterForListening(Animator character)
    {
        if (character != null)
        {
            character.transform.position = listeningPos.position;
            character.Play("listening", 0);
        }
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

        PlayPopAnimation();

        if (targetDialogue != null)
        {
            SetCharacterForDialogue(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);
        }
    }

    void FinishDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueHappening = false;
        foreach (Animator obj in talkingCharacters)
        {
            obj.gameObject.SetActive(false);
        }
    }

    void SetUpCharacters()
    {
        if (charactersParent != null)
        {
            talkingCharacters = charactersParent.GetComponentsInChildren<Animator>(true);
        }
        else
        {
            Debug.LogWarning("GameObject 'Characters' não encontrado na cena.");
        }
    }

    Animator FindCharacterAnimatorByName(string name)
    {
        foreach (Animator animator in talkingCharacters)
        {
            if (animator.gameObject.name == targetDialogue.Actor.ToString())
            {
                return animator;
            }
        }
        return null;
    }
}
