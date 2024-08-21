using DialogueSystem;
using DialogueSystem.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;




[RequireComponent(typeof(DialogueGroupSelector))]
public class DialogueUIManager : MonoBehaviour
{

    // update ui de dialogo
    // Setar o texto
    // diminuir opacidade do personagem que não está falando (deixar ele com layer preto)
    // 

    public GameObject dialogueUI;
    public TMP_Text dialogText;
    public CanvasGroup canvasGroup;
    public DialogueGroupSelector dialogue;
    DSDialogueSO targetDialogue;

    private void Start()
    {

        dialogue = GetComponent<DialogueGroupSelector>();
        targetDialogue = dialogue.targetDialogue;
        dialogText.text = targetDialogue.RequestText;
        // targetDialogue
        StartDialogue();
    }

    private void Update()
    {
        StartDialogue();

    }

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

        //  Instancia a UI de dialogo na tela
        void PopDialogueScreen(){

        }

        // Desativa e limpa a tela da UI de dialogo
        void finishDialogue(){
            

        }

        // Inicia nova linha de dialogo, atualiza o sprite do personagem falando e do personagem escutando e flipa o sprite dele no eixo X
        void UpdateDialogueUi(){

        }

    }



}


