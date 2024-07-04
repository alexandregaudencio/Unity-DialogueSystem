using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(DialogueSelector))]
public class MessageUIManager : MonoBehaviour
{
    public Text dialogText;
    public CanvasGroup canvasGroup;

    public DialogueSelector dialogue;

    bool isActive = false;

    private void Start()
    {
        dialogue = GetComponent<DialogueSelector>();
        targetDialogue = dialogue.TargetDialogue;
        dialogText.text = targetDialogue.RequestText;


    }

    private void Update()
    {
        StartDialogue();

    }

    DSDialogueSO targetDialogue;
    public void StartDialogue()
    {

        if (Input.anyKeyDown)
        {
            isActive = !isActive;
            if (isActive) dialogText.text = targetDialogue.RequestText;
            else dialogText.text = "...";
        }

    }


}

