using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Enumerations;
    using Utilities;
    using Windows;

    public class DSNode : Node
    {
        public string ID { get; set; }
        public string DialogueName { get; set; }
        public List<DSChoiceSaveData> Choices { get; set; }
        public string Text { get; set; }
        public DSDialogueType DialogueType { get; set; }
        public DSActor Actor { get; set; }

        public DSGroup Group { get; set; }
        protected DSGraphView graphView;
        private Color defaultBackgroundColor;

        [SerializeField]
        private MonoBehaviour associatedScript;

        private Port inputPort;
        TextElement dialogueNameTextElement;


        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Disconnect Input Ports", actionEvent => DisconnectInputPorts());
            evt.menu.AppendAction("Disconnect Output Ports", actionEvent => DisconnectOutputPorts());

            base.BuildContextualMenu(evt);
        }


        public virtual void Initialize(string nodeName, DSActor actor, DSGraphView dsGraphView, Vector2 position)
        {
            ID = Guid.NewGuid().ToString();
            DialogueName = nodeName ;
            Choices = new List<DSChoiceSaveData>();
            Text = "New Dialogue Text " + ID;
            Actor = actor;

            SetPosition(new Rect(position, Vector2.zero));

            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
        }

        public virtual void Draw()
        {

            /* INPUT CONTAINER */
            inputPort = this.CreatePort("Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);

            /* TITLE CONTAINER */
            dialogueNameTextElement = DSElementUtility.CreateTextElement(DialogueName.DialogueNameRangeFormat(), OnDialogueNameChanged);

            dialogueNameTextElement.AddClasses(
                "ds-node__text-field",
                "ds-node__text-field__hidden",
                "ds-node__filename-text-field"
            );

            titleContainer.Insert(0, dialogueNameTextElement);



            /* EXTENSION CONTAINER */
            VisualElement customDataContainer = new VisualElement();

            customDataContainer.AddToClassList("ds-node__custom-data-container");


            Foldout textFoldout = DSElementUtility.CreateFoldout("");

            List<string> actors = Enum.GetNames(typeof(DSActor)).ToList();
            int defaultIndex = actors.IndexOf(Actor.ToString());
            DropdownField dropdown = DSElementUtility.CreateDropdown("Actor", actors, OnActorChange, defaultIndex);
            textFoldout.Add(dropdown);

            TextField textTextField = DSElementUtility.CreateTextArea(Text, null, OnDialogueTextChanged);
            textTextField.AddClasses(
                "ds-node__text-field",
                "ds-node__quote-text-field"
            );
            textFoldout.Add(textTextField);
            customDataContainer.Add(textFoldout);


            extensionContainer.Add(customDataContainer);

        }

        private void OnDialogueNameChanged(ChangeEvent<string> callback)
        {
            TextElement target = (TextElement)callback.target;
            if (string.IsNullOrEmpty(target.text))
            {
                if (!string.IsNullOrEmpty(DialogueName))
                {
                    ++graphView.NameErrorsAmount;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(DialogueName))
                {
                    --graphView.NameErrorsAmount;
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

            DialogueName = target.text;

            graphView.AddGroupedNode(this, currentGroup);



        }

        private void OnDialogueTextChanged(ChangeEvent<string> callback)
        {
            TextField target = (TextField)callback.target;
            Text = target.text;
            dialogueNameTextElement.text = target.text.DialogueNameRangeFormat();

        }


        private string OnActorChange(string actor)
        {
            Actor = (DSActor)Enum.Parse(typeof(DSActor), actor, true);
            return actor;
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
            Debug.Log("starting node");
            Port inputPort = (Port)inputContainer.Children().First();

            return !inputPort.connected;
        }

        public void SetErrorStyle(Color color)
        {
            mainContainer.style.backgroundColor = color;
        }

        public void ResetStyle()
        {
            mainContainer.style.backgroundColor = defaultBackgroundColor;
        }

    }
}