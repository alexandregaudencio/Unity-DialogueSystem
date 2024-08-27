using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using System;
using UnityEngine.UI;




[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{
    // update ui de dialogo
    // Setar o texto

    // do lado direito o npc1
    private bool dialogueHappening;


    [Header("Dialogue UI Data")]
    public CharacterDialogueAnimations characterterAnimations;
    public GameObject dialogueUI;
    public TMP_Text dialogText;
    public TMP_Text characterNameText;
    public CanvasGroup canvasGroup;

    public Animator talkingCharacter;
    public Animator listeningCharacter;


    public string lastActor;




    public DialogueGroupSelector dialogue;
    DSDialogueSO targetDialogue;

    private void Start()
    {
        dialogueUI.SetActive(false); // inicializa sem dialogo ativo
        dialogue = GetComponent<DialogueGroupSelector>();
    }

    void OnEnable()
    {
        targetDialogue = dialogue.targetDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();
        AnimationClip talkingAnimation = characterterAnimations.GetAnimationClip(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);
        Debug.Log($"Dialogue ui manager quer exibir animação {talkingAnimation.name}");
    }

    void Update()
    {
        CheckForInput();
    }

    void CheckForInput()
    {
        if (dialogueHappening)
        {
            if (Input.anyKeyDown)
            {
                if (targetDialogue.Choices[0].NextDialogue == null)
                {
                    finishDialogue();
                    return;
                }
                UpdateDialogue();
            }
        }
    }

    void UpdateDialogue()
    {

        targetDialogue = targetDialogue.Choices[0].NextDialogue;
        dialogText.text = targetDialogue.RequestText;
        characterNameText.text = targetDialogue.Actor.ToString();
        AnimationClip talkingAnimation = characterterAnimations.GetAnimationClip(targetDialogue.Actor.ToString(), targetDialogue.speechAnimation);



        talkingCharacter.Play($"{targetDialogue.speechAnimation}", talkingCharacter.GetLayerIndex($"{targetDialogue.Actor}"));
        Debug.Log($"Dialogue ui manager quer exibir animação {talkingAnimation.name}");
    }

    public void StartDialogue()
    {
        dialogueUI.SetActive(true);
        dialogueHappening = true;
    }

    // Desativa e limpa a tela da UI de dialogo
    void finishDialogue()
    {
        dialogueUI.SetActive(false);
        dialogueHappening = false;
    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 50, 50), "Iniciar dialogo"))
            StartDialogue();
    }

    void UpdateSpritesPosition()
    {
        // atualiza a posição e sprite animado do talking char correspondendo com quem está falando
        // atualiza o sprite do listening char, e coloca ele na animação default (escutando - idle)
        // diminuir opacidade do personagem que não está falando (deixar ele com layer mais escuro)
    }


}


