using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using DialogueSystem.Data.Error;
    using ScriptableObjects;
    using Enumerations;
    using Utilities;
    using Windows;


    public class DSNode : Node, IEdgeConnectorListener
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSActor Actor { get; set; }
        public string SpeechAnimation { get; set; }
        List<string> animations { get; set; }
        public CharacterDialogueAnimations charactersDialogueAnimations { get; set; }

        public DropdownField animationDropdown { get; set; }


        public DSGroup Group { get; set; }
        protected DSGraphView graphView;
        private Color defaultBackgroundColor;

        [SerializeField]
        private MonoBehaviour associatedScript;

        private Port inputPort;
        TextElement dialogueNameTextElement;

        public Action<UnityEditor.Experimental.GraphView.Edge, Vector2> OnDropOutsidePortEvent;


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }


        public virtual void Initialize(string nodeName, DSActor actor, string _speechAnimation, DSGraphView dsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName;
            Choices = new List<DSChoiceSaveData>();
            Text = " Dialogue " + ID;
            Actor = actor;
            SpeechAnimation = _speechAnimation; //* Animação sendo definida
            SetPosition(new Rect(position, Vector2.zero));


            graphView = dsGraphView;
            graphView.NameErrorsAmount += 1;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
            if (DialogueName == "")
            {
                SetErrorColor();
                ++graphView.NameErrorsAmount;
            }

            charactersDialogueAnimations = DSIOUtility.LoadAsset<CharacterDialogueAnimations>("Assets", "CharactersDialogueAnimations");// <CharacterDialogueAnimations>("Assets/DataBase/Dialogues/DialogueAnimations/CharactersDialogueAnimations")
            if (!charactersDialogueAnimations) Debug.LogError("Node não conseguiu encontrar as animações de dialogo, por favor cheque o caminho para este asset.");





        }

        public virtual void Draw()
        {

            /* INPUT CONTAINER */
            inputPort = this.CreatePort("Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);



            // ... other code ...
            // var edgeConnectorListener = new CustomEdgeConnectorListener();
            // inputPort.AddManipulator(new EdgeConnector<Edge>(new CustomEdgeConnectorListener()));
            // edgeConnectorListener.OnDropOutsidePortEvent = TestEvent;
            // inputPort.AddManipulator(MouseManipulator)

            /* TITLE CONTAINER */
            string DialogueNameFormated = DialogueName.DialogueNameRangeFormat();
            dialogueNameTextElement = DSElementUtility.CreateTextElement(DialogueNameFormated, OnDialogueNameChanged);

            //! Verificar
            dialogueNameTextElement.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(1, dialogueNameTextElement);



            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");


            Foldout textFoldout = DSElementUtility.CreateFoldout("");

            //* Carregar lista de atores disponiveis
            List<string> actors = Enum.GetNames(typeof(DSActor)).ToList();
            int defaultIndex = actors.IndexOf(Actor.ToString());
            DropdownField dropdown = DSElementUtility.CreateDropdown("Actor", actors, OnActorChange, defaultIndex);
            textFoldout.Add(dropdown);


            //* Carregar lista de speech animations disponiveis
            animations = charactersDialogueAnimations.GetAnimationsForCharacter(Actor.ToString()).Select(anim => anim.name).ToList();
            int animationIndex = animations.IndexOf(SpeechAnimation);
            animationDropdown = DSElementUtility.CreateDropdown("Speech Animation", animations, OnAnimationChange, animationIndex);
            textFoldout.Add(animationDropdown);




            TextField textTextField = DSElementUtility.CreateTextArea(this.Text, null, OnDialogueTextChanged);
            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );
            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);


            extensionContainer.Add(customDataContainer);


            if (DialogueName != "") UpdateAnimationDropdownOnLoad();
            else Debug.Log("node nova");


        }

        public string OnAnimationChange(string animation)
        {
            SpeechAnimation = animation;
            return animation;
        }


        private string OnActorChange(string actor)
        {
            // Atualizar a lista de animações de acordo com o ator selecionado
            Actor = (DSActor)Enum.Parse(typeof(DSActor), actor, true);
            UpdateAnimationDropdown(actor);
            return actor;
        }

        private void UpdateAnimationDropdown(string newActor)
        {


            if (animationDropdown == null) return;

            // quando essa função é chamada sem actor defina o actor do dataSave da node
            if (SpeechAnimation == null)
            {
                animations = charactersDialogueAnimations.GetAnimationsForCharacter(newActor.ToString()).Select(anim => anim.name).ToList();
                animationDropdown.choices = animations;
                animationDropdown.value = animations[0];
            }
            else
            {
                animations = charactersDialogueAnimations.GetAnimationsForCharacter(newActor.ToString()).Select(anim => anim.name).ToList();
                animationDropdown.choices = animations;
                animationDropdown.value = SpeechAnimation;
            }



            // // Carregar as animações do novo ator
            // if (animationDropdown != null)
            // {
            //     animationDropdown.choices = animations;
            //     if (animations.Count > 0)
            //     {
            //         animationDropdown.value = animations[0]; // Selecionar a primeira animação por padrão
            //     }
            //     else
            //     {
            //         animationDropdown.value = string.Empty; // Limpar o dropdown se não houver animações
            //     }
            // }
        }

        void UpdateAnimationDropdownOnLoad()
        {
            animations = charactersDialogueAnimations.GetAnimationsForCharacter(Actor.ToString()).Select(anim => anim.name).ToList();
            animationDropdown.choices = animations;
            Debug.Log($"UpdateAnimationDropdownOnLoad {SpeechAnimation}");
            int currenAnimationIndex = animations.IndexOf(SpeechAnimation);
            animationDropdown.value = SpeechAnimation;
        }





        private void OnDialogueNameChanged(ChangeEvent<string> callback)
        {
            TextElement target = (TextElement)callback.target;

            if (string.IsNullOrEmpty(target.text))
            {
                SetErrorColor();
                if (!string.IsNullOrEmpty(DialogueName))
                {
                    ++graphView.NameErrorsAmount;
                }
            }
            else
            {

                ResetBackgroundColor();  //* Fix do bug da node sempre vermelha mesmo com texto
                --graphView.NameErrorsAmount;
                if (string.IsNullOrEmpty(DialogueName))
                {
                    ResetBackgroundColor();
                }
            }

            if (Group == null)
            {
                graphView.RemoveUngroupedNode(this);

                DialogueName = target.text;

                graphView.AddUngroupedNode(this);

                return;
            }

            DSGroup currentGroup = Group;

            graphView.RemoveGroupedNode(this, Group);

            DialogueName = target.text.SanitizeFileName();

            graphView.AddGroupedNode(this, currentGroup);



        }

        private void OnDialogueTextChanged(ChangeEvent<string> callback)
        {
            TextField target = (TextField)callback.target;
            Text = target.text;
            dialogueNameTextElement.text = target.text.DialogueNameRangeFormat();
            if (target.text == "") SetErrorColor();
            else ResetBackgroundColor();
        }




        public void DisconnectAllPorts()
        {
            DisconnectInputPorts();
            DisconnectOutputPorts();
        }

        private void DisconnectInputPorts()
        {
            DisconnectPorts(inputContainer);
        }

        private void DisconnectOutputPorts()
        {
            DisconnectPorts(outputContainer);
        }

        private void DisconnectPorts(VisualElement container)
        {
            foreach (Port port in container.Children())
            {
                if (!port.connected)
                {
                    continue;
                }
                graphView.DeleteElements(port.connections);
            }
        }

        public bool IsStartingNode()
        {
            Port inputPort = (Port)inputContainer.Children().First();
            inputContainer.Add(inputPort);

            return !inputPort.connected;
        }

        public void SetBackgroundColor(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void SetErrorColor()
        {
            mainContainer.style.backgroundColor = DSErrorData.Color;
        }

        public void ResetBackgroundColor()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

        // Defina o evento ou delegate apenas uma vez     

        public void OnDropOutsidePort(UnityEditor.Experimental.GraphView.Edge edge, Vector2 position)
        {
            OnDropOutsidePortEvent?.Invoke(edge, position);
            throw new NotImplementedException();
        }

        public void OnDrop(GraphView graphView, UnityEditor.Experimental.GraphView.Edge edge)
        {
            throw new NotImplementedException();
        }

    }


}