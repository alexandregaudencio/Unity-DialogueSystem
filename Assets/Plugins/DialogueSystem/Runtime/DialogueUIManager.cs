using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{
    public TMP_Text dialogText;
    public CanvasGroup canvasGroup;
    public DialogueGroupSelector dialogue;

    private void Start()
    {

        dialogue = GetComponent<DialogueGroupSelector>();
        targetDialogue = dialogue.targetDialogue;
        dialogText.text = targetDialogue.RequestText;
        StartDialogue();
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
            Debug.Log("teste");
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


