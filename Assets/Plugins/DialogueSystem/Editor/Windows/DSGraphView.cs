using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Windows
{
    using Data.Error;
    using Data.Save;
    using Elements;
    using Enumerations;
    using Utilities;

    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        private int nameErrorsAmount;

        public int NameErrorsAmount
        {
            get
            {
                return nameErrorsAmount;
            }

            set
            {
                nameErrorsAmount = value;

                if (nameErrorsAmount == 0)
                {
                    editorWindow.EnableSaving();
                }

                if (nameErrorsAmount == 1)
                {
                    editorWindow.DisableSaving();
                    Debug.Log("error");

                }
            }
        }

        public DSGraphView(DSEditorWindow dsEditorWindow)
        {
            editorWindow = dsEditorWindow;

            ungroupedNodes = new SerializableDictionary<string, DSNodeErrorData>();
            groups = new SerializableDictionary<string, DSGroupErrorData>();
            groupedNodes = new SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>>();

            AddManipulators();
            AddGridBackground();
            AddSearchWindow();
            AddMiniMap();

            OnElementsDeleted();
            OnGroupElementsAdded();
            OnGroupElementsRemoved();
            OnGroupRenamed();
            OnGraphViewChanged();

            AddStyles();
            AddMiniMapStyles();


            // this.RegisterCallback<MouseDownEvent>((evt) => Debug.Log("Mouse down"));
            // this.RegisterCallback<MouseUpEvent>((evt) => Debug.Log("Mouse up"));
            //this.RegisterCallback<MouseMoveEvent>(OnMouseMove);
        }
        private Edge edgeInProgress;


        //private void OnMouseDown(MouseDownEvent evt)
        //{
        //    if (evt.button == (int)MouseButton.LeftMouse && edgeInProgress == null)
        //    {
        //        // Comece a criar uma nova aresta
        //        edgeInProgress = new Edge();
        //        //edgeInProgress.input = new Port();
        //        //edgeInProgress.input.owe = null; // Porta de origem n�o definida ainda
        //        edgeInProgress.input.Connect(edgeInProgress);
        //        AddElement(edgeInProgress);
        //        evt.StopPropagation();
        //    }
        //}

        //private void OnMouseMove(MouseMoveEvent evt)
        //{
        //    if (edgeInProgress != null)
        //    {
        //        // Atualize a posi��o da extremidade da aresta conforme o mouse � movido
        //        Vector2 mousePosition = evt.mousePosition;
        //        //edgeInProgress.output = new Port();
        //        edgeInProgress.output.Connect(edgeInProgress);
        //        //edgeInProgress.output.owner = null; // Porta de destino n�o definida ainda
        //        edgeInProgress.UpdateEdgeControl();
        //        edgeInProgress.candidatePosition = mousePosition;
        //        evt.StopPropagation();
        //    }
        //}

        //private void OnMouseUp(MouseUpEvent evt)
        //{
        //    if (edgeInProgress != null)
        //    {
        //        // Verifique se a origem e o destino da aresta est�o definidos
        //        if (edgeInProgress.input.owner == null || edgeInProgress.output.owner == null)
        //        {
        //            OnEdgeCreatedWithoutConnection(edgeInProgress);
        //        }

        //        // Limpe a aresta em progresso
        //        edgeInProgress.input.Disconnect(edgeInProgress);
        //        edgeInProgress.output.Disconnect(edgeInProgress);
        //        RemoveElement(edgeInProgress);
        //        edgeInProgress = null;
        //        evt.StopPropagation();
        //    }
        //}

        //private void OnEdgeCreatedWithoutConnection(Edge edge)
        //{
        //    // Execute a l�gica desejada quando uma aresta � criada, mas n�o conectada a outro n�
        //    Debug.Log("Uma aresta foi criada, mas n�o conectada a outro n�!");

        //    // Aqui voc� pode executar qualquer a��o necess�ria quando uma aresta � criada sem uma conex�o
        //}



        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port)
                {
                    return;
                }

                if (startPort.node == port.node)
                {
                    return;
                }

                if (startPort.direction == port.direction)
                {
                    return;
                }

                compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            this.AddManipulator(CreateNodeContextualMenu("Create Dialogue", DSDialogueType.SingleChoice));
            //this.AddManipulator(CreateNodeContextualMenu("Add Multiple Choice", DSDialogueType.MultipleChoice));

            this.AddManipulator(CreateGroupContextualMenu());
        }

        private IManipulator CreateNodeContextualMenu(string actionTitle, DSDialogueType dialogueType)
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("", dialogueType, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup("DialogueGroup", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        public DSGroup CreateGroup(string title, Vector2 position)
        {
            DSGroup group = new DSGroup(title, position);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                if (!(selectedElement is DSNode))
                {
                    continue;
                }

                DSNode node = (DSNode)selectedElement;

                group.AddElement(node);
            }

            return group;
        }

        public bool SelectionHasMutipleGroup()
        {
            int count = 0;
            foreach (GraphElement selectedElement in selection)
            {
                if ((selectedElement is DSGroup))
                {
                    count++;
                }
            }
            Debug.Log(count);

            return count > 1;
        }

        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, Vector2 position, bool shouldDraw = true)
        {
            Type nodeType = Type.GetType($"{typeof(DSNode).Namespace}.DS{dialogueType}Node");
            DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            node.Initialize(nodeName, DSActor.None, this, position);

            if (shouldDraw)
            {
                node.Draw();
            }

            //DSGroup SelectedGroup = selection.First(s => s is DSGroup) as DSGroup;
            //Debug.Log(SelectedGroup);
            //if(SelectedGroup != null)
            //{
            //    AddGroupedNode(node, SelectedGroup);
            //    return node;
            //}

            AddUngroupedNode(node);
            return node;
        }

        private void OnElementsDeleted()
        {
            deleteSelection = (operationName, askUser) =>
            {
                Type groupType = typeof(DSGroup);
                Type edgeType = typeof(Edge);

                List<DSGroup> groupsToDelete = new List<DSGroup>();
                List<DSNode> nodesToDelete = new List<DSNode>();
                List<Edge> edgesToDelete = new List<Edge>();

                foreach (GraphElement selectedElement in selection)
                {
                    if (selectedElement is DSNode node)
                    {
                        nodesToDelete.Add(node);

                        continue;
                    }

                    if (selectedElement.GetType() == edgeType)
                    {
                        Edge edge = (Edge)selectedElement;

                        edgesToDelete.Add(edge);

                        continue;
                    }

                    if (selectedElement.GetType() != groupType)
                    {
                        continue;
                    }

                    DSGroup group = (DSGroup)selectedElement;

                    groupsToDelete.Add(group);
                }

                foreach (DSGroup groupToDelete in groupsToDelete)
                {
                    List<DSNode> groupNodes = new List<DSNode>();

                    foreach (GraphElement groupElement in groupToDelete.containedElements)
                    {
                        if (!(groupElement is DSNode))
                        {
                            continue;
                        }

                        DSNode groupNode = (DSNode)groupElement;

                        groupNodes.Add(groupNode);
                    }

                    groupToDelete.RemoveElements(groupNodes);

                    RemoveGroup(groupToDelete);

                    RemoveElement(groupToDelete);
                }

                DeleteElements(edgesToDelete);

                foreach (DSNode nodeToDelete in nodesToDelete)
                {
                    if (nodeToDelete.Group != null)
                    {
                        nodeToDelete.Group.RemoveElement(nodeToDelete);
                    }

                    RemoveUngroupedNode(nodeToDelete);

                    nodeToDelete.DisconnectAllPorts();

                    RemoveElement(nodeToDelete);
                }
            };
        }

        private void OnGroupElementsAdded()
        {
            elementsAddedToGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup dsGroup = (DSGroup)group;
                    DSNode node = (DSNode)element;

                    RemoveUngroupedNode(node);
                    AddGroupedNode(node, dsGroup);
                }
            };
        }

        private void OnGroupElementsRemoved()
        {
            elementsRemovedFromGroup = (group, elements) =>
            {
                foreach (GraphElement element in elements)
                {
                    if (!(element is DSNode))
                    {
                        continue;
                    }

                    DSGroup dsGroup = (DSGroup)group;
                    DSNode node = (DSNode)element;

                    RemoveGroupedNode(node, dsGroup);
                    AddUngroupedNode(node);
                }
            };
        }

        private void OnGroupRenamed()
        {
            groupTitleChanged = (group, newTitle) =>
            {
                DSGroup dsGroup = (DSGroup)group;

                dsGroup.title = newTitle.RemoveWhitespaces().RemoveSpecialCharacters();

                if (string.IsNullOrEmpty(dsGroup.title))
                {
                    if (!string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        ++NameErrorsAmount;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(dsGroup.OldTitle))
                    {
                        --NameErrorsAmount;
                    }
                }

                RemoveGroup(dsGroup);

                dsGroup.OldTitle = dsGroup.title;

                AddGroup(dsGroup);
            };
        }

        private void OnGraphViewChanged()
        {
            graphViewChanged = (changes) =>
            {
                if (changes.edgesToCreate != null)
                {
                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        Debug.Log("Node: " + edge.input.node.title + " ___ " + edge.output.node.title);
                        DSNode nextNode = (DSNode)edge.input.node;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = nextNode.ID;

                    }
                }

                if (changes.elementsToRemove != null)
                {
                    Type edgeType = typeof(Edge);

                    foreach (GraphElement element in changes.elementsToRemove)
                    {
                        if (element.GetType() != edgeType)
                        {
                            continue;
                        }

                        Edge edge = (Edge)element;

                        DSChoiceSaveData choiceData = (DSChoiceSaveData)edge.output.userData;

                        choiceData.NodeID = "";
                    }
                }

                return changes;
            };
        }


        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName;

            if (!ungroupedNodes.ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                ungroupedNodes.Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Add(node);

            Color errorColor = DSErrorData.Color;

            node.SetBackgroundColor(errorColor);

            if (ungroupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;

                ungroupedNodesList[0].SetBackgroundColor(errorColor);
            }
        }

        public void RemoveUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName;

            List<DSNode> ungroupedNodesList = ungroupedNodes[nodeName].Nodes;

            ungroupedNodesList.Remove(node);

            //node.ResetBackgroundColor();

            if (ungroupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                ungroupedNodesList[0].ResetBackgroundColor();

                return;
            }

            if (ungroupedNodesList.Count == 0)
            {
                ungroupedNodes.Remove(nodeName);
            }
        }

        private void AddGroup(DSGroup group)
        {
            string groupName = group.title;

            if (!groups.ContainsKey(groupName))
            {
                DSGroupErrorData groupErrorData = new DSGroupErrorData();

                groupErrorData.Groups.Add(group);

                groups.Add(groupName, groupErrorData);

                return;
            }

            List<DSGroup> groupsList = groups[groupName].Groups;

            groupsList.Add(group);


            group.SetErrorStyle();

            if (groupsList.Count == 2)
            {
                ++NameErrorsAmount;

                groupsList[0].SetErrorStyle();
            }
        }

        private void RemoveGroup(DSGroup group)
        {
            string oldGroupName = group.OldTitle;

            List<DSGroup> groupsList = groups[oldGroupName].Groups;

            groupsList.Remove(group);

            group.ResetStyle();

            if (groupsList.Count == 1)
            {
                --NameErrorsAmount;

                groupsList[0].ResetStyle();

                return;
            }

            if (groupsList.Count == 0)
            {
                groups.Remove(oldGroupName);
            }
        }

        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName;

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());
            }

            if (!groupedNodes[group].ContainsKey(nodeName))
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();

                nodeErrorData.Nodes.Add(node);

                groupedNodes[group].Add(nodeName, nodeErrorData);

                return;
            }

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Add(node);

            Color errorColor = DSErrorData.Color;

            node.SetBackgroundColor(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;
                groupedNodesList[0].SetBackgroundColor(errorColor);
            }
        }

        public void RemoveGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName;

            node.Group = null;

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;

            groupedNodesList.Remove(node);

            //node.ResetBackgroundColor();

            if (groupedNodesList.Count == 1)
            {
                --NameErrorsAmount;

                groupedNodesList[0].ResetBackgroundColor();

                return;
            }

            if (groupedNodesList.Count == 0)
            {
                groupedNodes[group].Remove(nodeName);

                if (groupedNodes[group].Count == 0)
                {
                    groupedNodes.Remove(group);
                }
            }
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        private void AddSearchWindow()
        {
            if (searchWindow == null)
            {
                searchWindow = ScriptableObject.CreateInstance<DSSearchWindow>();
            }

            searchWindow.Initialize(this);

            nodeCreationRequest = context => SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindow);
        }

        private void AddMiniMap()
        {
            miniMap = new MiniMap()
            {
                anchored = true
            };

            miniMap.SetPosition(new Rect(15, 50, 200, 180));

            Add(miniMap);

            miniMap.visible = false;
        }

        private void AddStyles()
        {
            this.AddStyleSheets(
                "DSGraphViewStyles.uss",
                "DSNodeStyles.uss"
            );
        }

        private void AddMiniMapStyles()
        {
            StyleColor backgroundColor = new StyleColor(new Color32(29, 29, 30, 255));
            StyleColor borderColor = new StyleColor(new Color32(51, 51, 51, 255));

            miniMap.style.backgroundColor = backgroundColor;
            miniMap.style.borderTopColor = borderColor;
            miniMap.style.borderRightColor = borderColor;
            miniMap.style.borderBottomColor = borderColor;
            miniMap.style.borderLeftColor = borderColor;
        }

        public Vector2 GetLocalMousePosition(Vector2 mousePosition, bool isSearchWindow = false)
        {
            Vector2 worldMousePosition = mousePosition;

            if (isSearchWindow)
            {
                worldMousePosition = editorWindow.rootVisualElement.ChangeCoordinatesTo(editorWindow.rootVisualElement.parent, mousePosition - editorWindow.position.position);
            }

            Vector2 localMousePosition = contentViewContainer.WorldToLocal(worldMousePosition);

            return localMousePosition;
        }

        public void ClearGraph()
        {
            graphElements.ForEach(graphElement => RemoveElement(graphElement));

            groups.Clear();
            groupedNodes.Clear();
            ungroupedNodes.Clear();

            NameErrorsAmount = 0;
        }

        public void ToggleMiniMap()
        {
            miniMap.visible = !miniMap.visible;
        }


    }
}