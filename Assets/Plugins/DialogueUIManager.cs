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

    [Header("Dialogue UI Data")]
    public CharacterDialogueAnimations characterterAnimations;
    public GameObject dialogueUI;
    public TMP_Text dialogText;
    public TMP_Text characterNameText;
    public CanvasGroup canvasGroup;

    public Transform talkingPos;
    public Transform listeningPos;

    public Animator[] talkingCharacters; // Array que conterá os animators dos personagens

    public GameObject charactersParent;

    public Animator currentTalkingCharacter;
    public Animator lastTalkingCharacter;

    public float popDuration = 0.2f; // Duration of the pop animation
    public float popScale = 1.2f; // The scale to pop to
    private Vector3 originalScale;

    public DialogueGroupSelector dialogue;
    DSDialogueSO targetDialogue;

    private void Start()
    {
        dialogueUI.SetActive(false); // inicializa sem a UI de diálogo ativa
        dialogue = GetComponent<DialogueGroupSelector>();
        targetDialogue = dialogue.targetDialogue;

        // Preenche o array com os personagens
        SetUpCharacters();
    }

    void OnEnable()
    {
        targetDialogue = dialogue.targetDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();
        Debug.Log(targetDialogue);


        // Se for a primeira linha de diálogo, configura o personagem inicial
        if (targetDialogue != null)
        {
            SetCharacterForDialogue(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);
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
        // Salva o personagem atual antes de mudar para o próximo
        lastTalkingCharacter = currentTalkingCharacter;
        // Debug.Log($"last talking char {lastTalkingCharacter.gameObject.name} and new is {newActor}");

        targetDialogue = targetDialogue.Choices[0].NextDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();

        Debug.Log($"{lastTalkingCharacter.gameObject.name} and {targetDialogue.Actor}");
        // Se o novo diálogo for de outro personagem, atualiza as posições e animações
        if (lastTalkingCharacter.gameObject.name != targetDialogue.Actor.ToString())
        {
            Debug.Log("teste de mudar persoinagem");
            SetCharacterForListening(lastTalkingCharacter);
        }

        SetCharacterForDialogue(newActor, targetDialogue.speechAnimation);
    }


    private void PlayPopAnimation()
    {
        // Reset to original scale before playing the animation
        originalScale = new Vector3(1.4f, 1.4f, 1);

        // Animate the scale to create the pop effect
        characterNameText.transform.DOScale(popScale, popDuration / 2).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Scale back to the original size after the pop
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
            // Debug.Log("primeira iteração feita");
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
        currentTalkingCharacter = currentTalkingCharacter = FindCharacterAnimatorByName(targetDialogue.speechAnimation);
        targetDialogue = dialogue.targetDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();
        // Se for a primeira linha de diálogo, configura o personagem inicial
        if (targetDialogue != null)
        {
            SetCharacterForDialogue(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);
        }

    }

    void FinishDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueHappening = false;
    }

    void SetUpCharacters()
    {


        if (charactersParent != null)
        {
            // Obtém todos os animators dos filhos do GameObject "Characters"
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
