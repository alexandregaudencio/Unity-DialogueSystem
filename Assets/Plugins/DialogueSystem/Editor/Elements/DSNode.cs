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
    using Enumerations;
    using Unity.VisualScripting;
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

        public DSGroup Group { get; set; }
        protected DSGraphView graphView;
        private Color defaultBackgroundColor;

        [SerializeField]
        private MonoBehaviour associatedScript;

        private Port inputPort;
        TextElement dialogueNameTextElement;
        
        public Action<UnityEditor.Experimental.GraphView.Edge, Vector2> OnDropOutsidePortEvent;
        private IEdgeConnectorListener edgeConnectorListener;


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
            Text = " Dialogue " + ID;
            Actor = actor;

            SetPosition(new Rect(position, Vector2.zero));

            graphView = dsGraphView;
            defaultBackgroundColor = new Color(29f / 255f, 29f / 255f, 30f / 255f);

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");
            if (DialogueName == "")
            {
                SetErrorColor();
                ++graphView.NameErrorsAmount;

            }

            
            
        }

        public virtual void Draw()
        {

            /* INPUT CONTAINER */
            inputPort = this.CreatePort("Connection", Orientation.Horizontal, Direction.Input, Port.Capacity.Multi);
            inputContainer.Add(inputPort);
            
            inputPort.AddManipulator(new EdgeConnector<UnityEditor.Experimental.GraphView.Edge>(edgeConnectorListener));
            

        // ... other code ...
            // var edgeConnectorListener = new CustomEdgeConnectorListener();
            // inputPort.AddManipulator(new EdgeConnector<Edge>(new CustomEdgeConnectorListener()));
            // edgeConnectorListener.OnDropOutsidePortEvent = TestEvent;
            // inputPort.AddManipulator(MouseManipulator)

            /* TITLE CONTAINER */
            string Text = DialogueName.DialogueNameRangeFormat();
            dialogueNameTextElement = DSElementUtility.CreateTextElement(Text, OnDialogueNameChanged);

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

        // att
        public void TestEvent(UnityEditor.Experimental.GraphView.Edge edge, Vector2 position){
            // GraphView.CreateNode()
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
            if(target.text == "") SetErrorColor();
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
            Debug.Log("Oi");
            OnDropOutsidePortEvent?.Invoke(edge, position);
            throw new NotImplementedException();
        }

        public void OnDrop(GraphView graphView, UnityEditor.Experimental.GraphView.Edge edge)
        {
            throw new NotImplementedException();
        }

    }

    
}