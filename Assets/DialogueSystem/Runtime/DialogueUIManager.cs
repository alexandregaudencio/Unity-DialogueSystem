using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{
    public Text dialogText;
    public CanvasGroup canvasGroup;
    public DialogueGroupSelector dialogue;

    private void Start()
    {
        dialogue = GetComponent<DialogueGroupSelector>();
        targetDialogue = dialogue.targetDialogue;

    }

    private void Update()
    {
        StartDialogue();

    }

    int index = 0;
    DSDialogueSO targetDialogue;
    public void StartDialogue()
    {

        if (Input.anyKeyDown)
        {
            //if (index > dialogue.DialogueGroupTarget.Count) return;
            if (targetDialogue.Choices[0].NextDialogue == null)
            {
                dialogText.text = "FIM DO DIALOGO.";
                return;
            }
            targetDialogue = targetDialogue.Choices[0].NextDialogue;
            dialogText.text = targetDialogue.RequestText;



        }

    }



}


