using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueSystem.Elements
{
    using Data.Save;
    using Enumerations;
    using UnityEngine.UIElements;
    using Utilities;
    using Windows;

    public class DSSingleChoiceNode : DSNode
    {
        public override void Initialize(string nodeName, DSActor actor, string SpeechAnimation, DSGraphView dsGraphView, Vector2 position)
        {
            base.Initialize(nodeName, actor, SpeechAnimation, dsGraphView, position);

            DialogueType = DSDialogueType.SingleChoice;

            DSChoiceSaveData choiceData = new DSChoiceSaveData()
            {
                Text = "Next Dialogue"
            };

            Choices.Add(choiceData);
        }

        public override void Draw()
        {
            base.Draw();

            /* OUTPUT CONTAINER */

            foreach (DSChoiceSaveData choice in Choices)
            {
                Port choicePort = this.CreatePort(choice.Text);

                choicePort.userData = choice;
                choicePort.AddManipulator(new EdgeConnector<Edge>(graphView._edgeConnectorListener));

                outputContainer.Add(choicePort);
            }

            RefreshExpandedState();
        }
    }
}
