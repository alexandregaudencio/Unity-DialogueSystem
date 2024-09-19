using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueSystem.Editor.Windows
{
    using Data.Error;
    using Data.Save;
    using Elements;
    using Enumerations;
    using Utilities;
    using DialogueSystem.Utilities;


    public class EdgeConnectionListener : IEdgeConnectorListener
    {
        private DSGraphView _graphView;

        public EdgeConnectionListener(DSGraphView graphView)
        {
            _graphView = graphView;
        }

        public void OnDrop(GraphView graphView, Edge edge)
        {
        }

        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            if (edge.output != null && edge.input == null)
            {
                Vector2 localMousePosition = _graphView.GetLocalMousePosition(position);
                var newNode = _graphView.CreateNode("", DSDialogueType.SingleChoice, null, localMousePosition);

                var newEdge = edge.output.ConnectTo(newNode.inputContainer[0] as Port);
                _graphView.AddElement(newNode);
                _graphView.AddElement(newEdge);
            }

            edge.RemoveFromHierarchy();
        }
    }


    public class DSGraphView : GraphView
    {
        private DSEditorWindow editorWindow;
        private DSSearchWindow searchWindow;

        private MiniMap miniMap;

        private SerializableDictionary<string, DSNodeErrorData> ungroupedNodes;
        private SerializableDictionary<string, DSGroupErrorData> groups;
        private SerializableDictionary<Group, SerializableDictionary<string, DSNodeErrorData>> groupedNodes;

        private int createdGroupsCount = 0;

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

                if (nameErrorsAmount >= 1)
                {
                    editorWindow.DisableSaving();
                }
            }
        }
        public EdgeConnectionListener _edgeConnectorListener;

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

            _edgeConnectorListener = new EdgeConnectionListener(this);

        }



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
                menuEvent => menuEvent.menu.AppendAction(actionTitle, actionEvent => AddElement(CreateNode("", dialogueType, null, GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))))
            );

            return contextualMenuManipulator;
        }

        private IManipulator CreateGroupContextualMenu()
        {

            // Debug.Log($"{createdGroupsCount} conta de grupos na tela");
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => CreateGroup($"DialogueGroup({createdGroupsCount})", GetLocalMousePosition(actionEvent.eventInfo.localMousePosition))));


            return contextualMenuManipulator;
        }

        // att
        public DSGroup CreateGroup(string title, Vector2 position)
        {
            ++createdGroupsCount;
            DSGroup group = new DSGroup(title, position);

            AddGroup(group);

            AddElement(group);

            foreach (GraphElement selectedElement in selection)
            {
                //* att
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

        //* att
        public DSNode CreateNode(string nodeName, DSDialogueType dialogueType, string SpeechAnimation, Vector2 position, bool shouldDraw = true)
        {

            Type nodeType = Type.GetType($"{typeof(DSNode).Namespace}.DS{dialogueType}Node");
            DSNode node = (DSNode)Activator.CreateInstance(nodeType);
            node.Initialize(nodeName, DSActor.None, SpeechAnimation, this, position);
            // Debug.Log($"node data on graph view {node.SpeechAnimation}");

            if (shouldDraw)
            {
                node.Draw();
            }


            AddUngroupedNode(node);
            foreach (GraphElement selectedElement in selection)
            {
                if (selectedElement is DSGroup selectedGroup)
                {
                    AddGroupedNode(node, selectedGroup);
                    selectedGroup.AddElement(node);
                    return node;

                }
            }
            //node..connector = new EdgeConnector<Edge>(_edgeConnectorListener);
            //node.inputContainer.ad


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
            // Debug.Log("Grupo adicionado");
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
                Debug.Log("Nome modificado");
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

                // Debug.Log("Teste");
                if (changes.edgesToCreate != null)
                {

                    foreach (Edge edge in changes.edgesToCreate)
                    {
                        // edge.OnSelected?.invoke
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

        // att
        public void AddUngroupedNode(DSNode node)
        {
            string nodeName = node.DialogueName;

            if (!ungroupedNodes.ContainsKey(nodeName))  //* Verifica se a node não existe antes de adicionar ela
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

        // att
        public void AddGroupedNode(DSNode node, DSGroup group)
        {
            string nodeName = node.DialogueName;

            node.Group = group;

            if (!groupedNodes.ContainsKey(group))  // Verifica se o grupo não está presente no dicionário de nós agrupados
            {
                groupedNodes.Add(group, new SerializableDictionary<string, DSNodeErrorData>());  // Adiciona o grupo ao dicionário com um novo dicionário serializável
            }

            if (!groupedNodes[group].ContainsKey(nodeName))  // Verifica se o nome do nó não está presente no dicionário do grupo
            {
                DSNodeErrorData nodeErrorData = new DSNodeErrorData();  // Cria uma nova instância de DSNodeErrorData
                nodeErrorData.Nodes.Add(node);  // Adiciona o nó à lista de nós no DSNodeErrorData
                groupedNodes[group].Add(nodeName, nodeErrorData);  // Adiciona o DSNodeErrorData ao dicionário do grupo usando o nome do nó como chave

                return;  // Sai da função, pois o nó foi adicionado pela primeira vez
            }

            List<DSNode> groupedNodesList = groupedNodes[group][nodeName].Nodes;  // Obtém a lista de nós do dicionário do grupo

            groupedNodesList.Add(node);  // Adiciona o nó à lista de nós
            Debug.Log($"teste: {nodeName}");

            // foreach (DSNode item in groupedNodesList)
            // {
            //    //  Debug.Log(item.viewDataKey);
            // }
            // Debug.Log("Node adicionada a um grupo com sucesso");

            Color errorColor = DSErrorData.Color;

            node.SetBackgroundColor(errorColor);

            if (groupedNodesList.Count == 2)
            {
                ++NameErrorsAmount;  // Incrementa a quantidade de erros de nome
                groupedNodesList[0].SetBackgroundColor(errorColor);  // Define a cor de fundo do primeiro nó como a cor de erro
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
            Debug.Log("Botão Pressionado");
            miniMap.visible = !miniMap.visible;
        }

    }
}