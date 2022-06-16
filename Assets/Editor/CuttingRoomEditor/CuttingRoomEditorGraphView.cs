using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System;
using System.Linq;

namespace CuttingRoom.Editor
{
    public class CuttingRoomEditorGraphView : GraphView
    {
        /// <summary>
        /// The nodes currently in the graph.
        /// </summary>
        public List<NarrativeObjectNode> NarrativeObjectNodes { get; private set; } = new List<NarrativeObjectNode>();

        /// <summary>
        /// Edges currently in the graph and their info.
        /// </summary>
        private List<EdgeState> EdgeStates { get; set; } = new List<EdgeState>();

        /// <summary>
        /// Invoked whenever the OnGraphViewChangeEvent method is invoked.
        /// </summary>
        public event Action<GraphViewChange> OnGraphViewChanged;

        /// <summary>
        /// Invoked whenever the view container changes.
        /// </summary>
        public event Action OnViewContainerPushed;

        /// <summary>
        /// Invoked whenever a view containers root narrative object changes.
        /// </summary>
        public event Action OnRootNarrativeObjectChanged;

        /// <summary>
        /// Invoked whenever the candidates of the visible view container change.
        /// </summary>
        public event Action OnNarrativeObjectCandidatesChanged;

        /// <summary>
        /// The supported node types in this graph view.
        /// </summary>
        private enum NodeType
        {
            NarrativeObject,
        }

        /// <summary>
        /// The supported port types in this graph view.
        /// </summary>
        private enum PortType
        {
            Input,
            Output,
        }

        /// <summary>
        /// The guid representing the container which is the narrative space.
        /// This container has no associated narrative object.
        /// </summary>
        public const string rootViewContainerGuid = "0";

        /// <summary>
        /// View stack for rendering the recursive layers of the narrative space.
        /// </summary>
        private Stack<ViewContainer> viewContainerStack = new Stack<ViewContainer>();

        /// <summary>
        /// The view container stack.
        /// </summary>
        public Stack<ViewContainer> ViewContainerStack { get { return viewContainerStack; } }

        /// <summary>
        /// The collection of containers which currently exists.
        /// These may or may not currently be on the view stack.
        /// </summary>
        public List<ViewContainer> viewContainers = new List<ViewContainer>();

        public CuttingRoomEditorGraphView(CuttingRoomEditorWindow window)
        {
            window.OnWindowCleared += OnWindowCleared;

            window.OnNarrativeObjectCreated += OnNarrativeObjectCreated;

            graphViewChanged += OnGraphViewChangedEvent;

            // Load the style sheet defining the style of the graph view.
            styleSheets.Add(Resources.Load<StyleSheet>("CuttingRoomEditorGraphView"));

            // Set the min and max zoom scales allowed.
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Add manipulators to handle interaction with the editor.
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new FreehandSelector());

            // Create a grid background instance.
            GridBackground gridBackground = new GridBackground();

            // Add the grid to this visual element.
            Insert(0, gridBackground);

            // Fit the grid background to the size of this visual element.
            gridBackground.StretchToParentSize();

            // Root view container always exists.
            viewContainers.Add(new ViewContainer(rootViewContainerGuid));

            // Push the "root" container of the graph view.
            // This element should never be removed and represents a container
            // for objects which are not part of another narrative object.
            viewContainerStack.Push(viewContainers[0]);
        }


        /// <summary>
        /// Add a node which inherits from NarrativeObjectNode to the graph view.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        public NarrativeObjectNode AddNode<T>(NarrativeObject narrativeObject, NarrativeObject parentNarrativeObject) where T : NarrativeObjectNode
        {
            // Careful here as the constructor is being reflected based on the parameters.
            // If parameters change, this method call will still be valid but fail to find ctor.
            NarrativeObjectNode narrativeObjectNode = Activator.CreateInstance(typeof(T), new object[] { narrativeObject, parentNarrativeObject }) as NarrativeObjectNode;

            NarrativeObjectNodes.Add(narrativeObjectNode);

            return narrativeObjectNode;
        }

        /// <summary>
        /// Invoked whenever a selection is made within the graph view.
        /// </summary>
        /// <param name="selectable"></param>
        public override void AddToSelection(ISelectable selectable)
        {
            if (selectable is NarrativeObjectNode)
            {
                GameObject narrativeObjectGameObject = (selectable as NarrativeObjectNode).NarrativeObject.gameObject;

                if (!Selection.objects.Contains(narrativeObjectGameObject))
                {
                    List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>(Selection.objects);

                    selectedObjects.Add(narrativeObjectGameObject);

                    Selection.objects = selectedObjects.ToArray();
                }
            }

            base.AddToSelection(selectable);
        }

        /// <summary>
        /// Invoked whenever a deselection is made within the graph view.
        /// </summary>
        /// <param name="selectable"></param>
        public override void RemoveFromSelection(ISelectable selectable)
        {
            if (selectable is NarrativeObjectNode)
            {
                GameObject narrativeObjectGameObject = (selectable as NarrativeObjectNode).NarrativeObject.gameObject;

                if (Selection.objects.Contains(narrativeObjectGameObject))
                {
                    List<UnityEngine.Object> selectedObjects = new List<UnityEngine.Object>(Selection.objects);

                    selectedObjects.Remove(narrativeObjectGameObject);

                    Selection.objects = selectedObjects.ToArray();
                }
            }

            base.RemoveFromSelection(selectable);
        }

        /// <summary>
        /// Invoked whenever the selection is cleared within the graph view.
        /// </summary>
        public override void ClearSelection()
        {
            Selection.objects = new UnityEngine.Object[0];

            base.ClearSelection();
        }

        /// <summary>
        /// Returns a list of compatible ports for a specified start port to connect to.
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="nodeAdapter"></param>
        /// <returns></returns>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            // Work out the type of the start node.
            if (startPort.node is NarrativeObjectNode)
            {
                NarrativeObjectNode startNarrativeObjectNode = startPort.node as NarrativeObjectNode;

                // Whether the compatible ports are input ports or output ports.
                PortType compatiblePortType = PortType.Output;

                // If the start port is an output port, then compatible ports are input ports.
                if (startPort == startNarrativeObjectNode.OutputPort)
                {
                    compatiblePortType = PortType.Input;
                }

                ports.ForEach((port) =>
                {
                    // If the port isnt the start port (cant link to itself) and
                    // the node isn't the same node (can't link own input to own output).
                    if (startPort != port && startPort.node != port.node)
                    {
                        if (port.node is NarrativeObjectNode)
                        {
                            NarrativeObjectNode narrativeObjectNode = port.node as NarrativeObjectNode;

                            // If searching for an input port and the port being examined is
                            // the input port of the node or vice versa for output nodes.
                            if (compatiblePortType == PortType.Input && port == narrativeObjectNode.InputPort ||
                                compatiblePortType == PortType.Output && port == narrativeObjectNode.OutputPort)
                            {
                                // TODO: Dont add port as compatible if a link already exists between the start port and the port being evaluated.
                                EdgeState existingEdgeState = EdgeStates.Where((edgeState) =>
                                {
                                    if (compatiblePortType == PortType.Input)
                                    {
                                        return edgeState.OutputNarrativeObjectNode.OutputPort == startPort && edgeState.InputNarrativeObjectNode.InputPort == port;
                                    }
                                    else
                                    {
                                        return edgeState.InputNarrativeObjectNode.InputPort == startPort && edgeState.OutputNarrativeObjectNode.OutputPort == port;
                                    }
                                }).FirstOrDefault();

                                // If no edge state exists already, then it is a valid port.
                                if (existingEdgeState == null)
                                {
                                    compatiblePorts.Add(port);
                                }
                            }
                        }
                    }
                });
            }

            return compatiblePorts;
        }

        /// <summary>
        /// Invoked when the editor window this graph view belongs to is cleared.
        /// </summary>
        private void OnWindowCleared()
        {
            // Remove all existing nodes as they have lost their references and must be regenerated.
            foreach (NarrativeObjectNode node in NarrativeObjectNodes)
            {
                if (Contains(node))
                {
                    RemoveElement(node);
                }
            }

            // Clear out old node references.
            NarrativeObjectNodes.Clear();

            // Remove all edge states as they have lost references and must be regenerated.
            foreach (EdgeState edgeState in EdgeStates)
            {
                if (Contains(edgeState.Edge))
                {
                    RemoveElement(edgeState.Edge);
                }
            }

            // Clear out old edge states.
            EdgeStates.Clear();
        }

        /// <summary>
        /// Invoked whenever a new narrative object is created via the toolbar.
        /// </summary>
        private void OnNarrativeObjectCreated(NarrativeObject narrativeObject)
        {
            // If the current view container has no root object, then set it.
            if (!ViewContainerHasRootNarrativeObject(viewContainerStack.Peek()))
            {
                SetNarrativeObjectAsRootOfViewContainer(viewContainerStack.Peek(), narrativeObject);
            }
        }

        /// <summary>
        /// Invoked when the graph view is changed.
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                foreach (GraphElement graphElement in graphViewChange.elementsToRemove)
                {
                    // If an element has been removed.
                    if (graphElement is Edge)
                    {
                        // Find the edge
                        EdgeState deletedEdgeState = EdgeStates.Where(edgeState => edgeState.Edge == graphElement as Edge).FirstOrDefault();

                        if (deletedEdgeState == null)
                        {
                            continue;
                        }

                        // Disconnect the narrative objects in the edge state which has been removed.
                        deletedEdgeState.OutputNarrativeObjectNode.NarrativeObject.outputSelectionDecisionPoint.RemoveCandidate(deletedEdgeState.InputNarrativeObjectNode.NarrativeObject.gameObject);

                        // Delete the edge state as it's corresponding edge no longer exists.
                        EdgeStates.Remove(deletedEdgeState);
                    }
                    else if (graphElement is NarrativeObjectNode)
                    {
                        NarrativeObjectNode narrativeObjectNode = graphElement as NarrativeObjectNode;

                        // Destroy the object in the hierarchy that the node being deleted represents.
                        UnityEngine.Object.DestroyImmediate(narrativeObjectNode.NarrativeObject.gameObject);

                        // Remove the node object as it has been removed from the graph.
                        //NarrativeObjectNodes.Remove(narrativeObjectNode);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                foreach (Edge edge in graphViewChange.edgesToCreate)
                {
                    NarrativeObjectNode outputNode = FindNarrativeObjectWithPort(edge.output);

                    NarrativeObjectNode inputNode = FindNarrativeObjectWithPort(edge.input);

                    if (outputNode != null && inputNode != null)
                    {
                        // Add the connection to the narrative object.
                        outputNode.NarrativeObject.outputSelectionDecisionPoint.AddCandidate(inputNode.NarrativeObject.gameObject);

                        // Add the edge state as this edge has come into existence without being created during the Populate method.
                        EdgeStates.Add(new EdgeState
                        {
                            Edge = edge,
                            InputNarrativeObjectNode = inputNode,
                            OutputNarrativeObjectNode = outputNode
                        });
                    }
                }
            }

            // Invoke change event.
            OnGraphViewChanged?.Invoke(graphViewChange);

            return graphViewChange;
        }

        private NarrativeObjectNode FindNarrativeObjectWithPort(Port port)
        {
            foreach (NarrativeObjectNode node in NarrativeObjectNodes)
            {
                if (node.InputPort == port || node.OutputPort == port)
                {
                    return node;
                }
            }

            Debug.LogError("Narrative Object Node with specified port not found.");

            return null;
        }

        /// <summary>
        /// Invoked to push a new view container onto the view stack.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void PushViewContainer(NarrativeObjectNode narrativeObjectNode)
        {
            ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == narrativeObjectNode.NarrativeObject.guid).FirstOrDefault();

            if (viewContainer == null)
            {
                viewContainer = new ViewContainer(narrativeObjectNode.NarrativeObject.guid);

                viewContainers.Add(viewContainer);
            }

            PushViewContainer(viewContainer);
        }

        /// <summary>
        /// Push view container onto the view stack and invoke callbacks.
        /// </summary>
        /// <param name="viewContainer"></param>
        private void PushViewContainer(ViewContainer viewContainer)
        {
            viewContainerStack.Push(viewContainer);

            OnViewContainerPushed?.Invoke();
        }

        /// <summary>
        /// Invoked to pop a view container off the view stack if possible.
        /// </summary>
        /// <returns>Whether a view container was successfully popped from the view stack.</returns>
        public bool PopViewContainer()
        {
            // Never pop off the final element (which is the root view container).
            if (viewContainerStack.Count > 1)
            {
                viewContainerStack.Pop();

                // No callback here as the window handles this event (as it invokes it).

                return true;
            }

            return false;
        }

        /// <summary>
        /// Pop containers until the view desired is top of the view stack.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        public bool PopViewContainersToViewContainer(ViewContainer viewContainer)
        {
            if (!viewContainerStack.Contains(viewContainer))
            {
                Debug.LogError($"View Container Stack does not contain a View Container with the guid: {viewContainer.narrativeObjectGuid}");

                return false;
            }

            bool popOccurred = false;

            while (viewContainerStack.Peek() != viewContainer && viewContainerStack.Count > 1)
            {
                bool pop = PopViewContainer();

                if (!popOccurred)
                {
                    if (pop)
                    {
                        popOccurred = true;
                    }
                }
            }

            return popOccurred;
        }

        /// <summary>
        /// Whether a narrative object is currently visible on the graph view.
        /// </summary>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        private bool IsVisible(NarrativeObject narrativeObject)
        {
            // If the narrative objects node is in the currently visible view container.
            if (viewContainerStack.Peek().ContainsNode(narrativeObject.guid))
            {
                return true;
            }

            // Find the view containers which contain the narrative object.
            IEnumerable<ViewContainer> viewContainersWithNarrativeObject = viewContainers.ToList().Where(viewContainer => viewContainer.ContainsNode(narrativeObject.guid));

            // If no view containers contain the narrative object, it has just been created and therefore should be visible.
            if (viewContainersWithNarrativeObject.Count() == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the list of deleted view containers and narrative object guids when deleting a view container.
        /// This is recursive so returns the contents of any child view containers and their contained narrative object guids.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <param name="deletedViewContainers"></param>
        /// <param name="deletedNarrativeObjectGuids"></param>
        private void GetDeletedViewContainersAndNarrativeObjectGuids(ViewContainer viewContainer, ref List<ViewContainer> deletedViewContainers, ref List<string> deletedNarrativeObjectGuids)
        {
            deletedViewContainers.Add(viewContainer);

            deletedNarrativeObjectGuids.Add(viewContainer.narrativeObjectGuid);

            // Check for nested view containers for the contents of the view container passed as parameter.
            foreach (string guid in viewContainer.narrativeObjectNodeGuids)
            {
                // Find the view containers of any children of this view container.
                ViewContainer childViewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == guid).FirstOrDefault();

                // If the child node has a view container, this and its contents must go too.
                if (childViewContainer != null)
                {
                    // Recursively call this method to get rid of the contents.
                    GetDeletedViewContainersAndNarrativeObjectGuids(childViewContainer, ref deletedViewContainers, ref deletedNarrativeObjectGuids);
                }
                else
                {
                    // Doesn't have a view container so just mark the object itself for deletion (no recursion required).
                    deletedNarrativeObjectGuids.Add(guid);
                }
            }
        }

        /// <summary>
        /// Get the guid for the root narrative object of the specified view container.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        private string GetViewContainerRootNarrativeObjectGuid(ViewContainer viewContainer)
        {
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = GetNarrativeSpace();

                // If a root is set on the narrative space, return its guid.
                if (narrativeSpace != null && narrativeSpace.rootNarrativeObject != null)
                {
                    return narrativeSpace.rootNarrativeObject.guid;
                }
            }
            else
            {
                NarrativeObject narrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (narrativeObject != null)
                {
                    if (narrativeObject is GraphNarrativeObject)
                    {
                        GraphNarrativeObject graphNarrativeObject = narrativeObject.GetComponent<GraphNarrativeObject>();

                        // If a root is set on the graph object, return its guid.
                        if (graphNarrativeObject.rootNarrativeObject != null)
                        {
                            return graphNarrativeObject.rootNarrativeObject.guid;
                        }
                    }
                }
            }

            return string.Empty;
        }

        public class PopulateResult
        {
            /// <summary>
            /// Whether the call to Populate changed the graph view and should be saved.
            /// </summary>
            public bool GraphViewChanged { get; set; } = false;

            /// <summary>
            /// The guid and position of any newly created nodes.
            /// </summary>
            public List<Tuple<string, Vector2>> CreatedNodes { get; set; } = new List<Tuple<string, Vector2>>();
        }

        /// <summary>
        /// Populate the graph view based on the contents of the scene currently open.
        /// </summary>
        /// <returns>Whether the graph view contents have been changed.</returns>
        public PopulateResult Populate(CuttingRoomEditorGraphViewState graphViewState, NarrativeObject[] narrativeObjects)
        {
            // Returned populate result.
            PopulateResult populateResult = new PopulateResult();

            if (graphViewState != null)
            {
                // If the number of view containers is different to the save, then a view container has been added or removed
                // or if there view stack has been pushed/popped.
                // This change must be saved.
                if (viewContainers.Count != graphViewState.viewContainerStates.Count ||
                    viewContainerStack.Count != graphViewState.viewContainerStackGuids.Count)
                {
                    populateResult.GraphViewChanged = true;
                }

                // If a narrative object has been added or removed since the last state, then save.
                if (narrativeObjects.Length != graphViewState.narrativeObjectNodeStates.Count)
                {
                    populateResult.GraphViewChanged = true;
                }

                // Ensure all view containers exist.
                foreach (ViewContainerState viewContainerState in graphViewState.viewContainerStates)
                {
                    bool viewContainerShouldExist = viewContainerState.narrativeObjectGuid == rootViewContainerGuid || narrativeObjects.Where(narrativeObject => narrativeObject.guid == viewContainerState.narrativeObjectGuid).FirstOrDefault() != null;

                    // The view container (if it exists).
                    ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == viewContainerState.narrativeObjectGuid).FirstOrDefault();

                    // Ensure that the narrative object which the view container state represents still exists or check if the container is the root before restoring.
                    if (viewContainerShouldExist)
                    {
                        // If the view container doesnt exist.
                        if (viewContainer == null)
                        {
                            viewContainer = new ViewContainer(viewContainerState.narrativeObjectGuid);

                            viewContainers.Add(viewContainer);
                        }

                        // Find which guids inside the view container still exist.
                        List<string> narrativeObjectNodeGuids = new List<string>();

                        foreach (string guid in viewContainerState.narrativeObjectNodeGuids)
                        {
                            // Get the narrative object which has the guid.
                            NarrativeObject existingNarrativeObject = narrativeObjects.Where(narrativeObject => narrativeObject.guid == guid).FirstOrDefault();

                            // If the narrative object exists, then it's guid is still valid, else the narrative object has been removed so don't add the guid again.
                            if (existingNarrativeObject != null)
                            {
                                narrativeObjectNodeGuids.Add(existingNarrativeObject.guid);
                            }
                        }

                        // Restore the guids of the container.
                        viewContainer.narrativeObjectNodeGuids = narrativeObjectNodeGuids;
                    }
                    else
                    {
                        // The container should not exist. Remove it if it exists.
                        if (viewContainers.Contains(viewContainer))
                        {
                            if (viewContainerStack.Contains(viewContainer))
                            {
                                PopViewContainersToViewContainer(viewContainer);
                            }

                            List<ViewContainer> deletedViewContainers = new List<ViewContainer>();

                            List<string> deletedNarrativeObjectGuids = new List<string>();

                            // Find the view containers and narrative objects which must be removed (self and children of view container which should not exist).
                            GetDeletedViewContainersAndNarrativeObjectGuids(viewContainer, ref deletedViewContainers, ref deletedNarrativeObjectGuids);

                            foreach (string guid in deletedNarrativeObjectGuids)
                            {
                                NarrativeObject deletedNarrativeObject = narrativeObjects.Where(narrativeObject => narrativeObject.guid == guid).FirstOrDefault();

                                // Delete narrative object if it exists in the scene.
                                if (deletedNarrativeObject != null)
                                {
                                    UnityEngine.Object.DestroyImmediate(deletedNarrativeObject.gameObject);
                                }
                            }

                            foreach (ViewContainer deletedViewContainer in deletedViewContainers)
                            {
                                // Pop view to be deleted if its on the stack at the moment.
                                if (viewContainerStack.Contains(deletedViewContainer))
                                {
                                    PopViewContainersToViewContainer(deletedViewContainer);
                                }

                                // Remove the view containers which have to be deleted.
                                if (viewContainers.Contains(deletedViewContainer))
                                {
                                    viewContainers.Remove(deletedViewContainer);
                                }
                            }
                        }
                    }
                }

                foreach (string guid in graphViewState.viewContainerStackGuids)
                {
                    ViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == guid).FirstOrDefault();

                    if (viewContainer != null)
                    {
                        if (!viewContainerStack.Contains(viewContainer))
                        {
                            PushViewContainer(viewContainer);
                        }
                    }
                }
            }

            // Remove any objects which have been deleted due to their view container being deleted or recursively as part of the parent view container being deleted.
            narrativeObjects = narrativeObjects.Where(narrativeObject => narrativeObject != null).ToArray();

            // The view container being rendered.
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // Get the narrative object represented by the visible view container.
            NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

            foreach (NarrativeObject narrativeObject in narrativeObjects)
            {
                // If the narrative object is not visible, then continue onto the next and create no node.
                if (!IsVisible(narrativeObject))
                {
                    continue;
                }

                // Find any nodes which already exist in the graph view for the specified narrative object.
                IEnumerable<NarrativeObjectNode> existingNodesForNarrativeObject = NarrativeObjectNodes.Where((node) => node.NarrativeObject.guid == narrativeObject.guid);

                NarrativeObjectNode narrativeObjectNode = null;

                // If a node doesn't exist, create one.
                if (existingNodesForNarrativeObject.Count() == 0)
                {
                    if (narrativeObject is AtomicNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<AtomicNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);
                    }
                    else if (narrativeObject is GraphNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<GraphNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);

                        GraphNarrativeObjectNode graphNarrativeObjectNode = narrativeObjectNode as GraphNarrativeObjectNode;

                        graphNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        graphNarrativeObjectNode.OnClickViewContents += PushViewContainer;
                    }
                    else if (narrativeObject is GroupNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<GroupNarrativeObjectNode>(narrativeObject, visibleViewContainerNarrativeObject);

                        GroupNarrativeObjectNode groupNarrativeObjectNode = narrativeObjectNode as GroupNarrativeObjectNode;

                        groupNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        groupNarrativeObjectNode.OnClickViewContents += PushViewContainer;
                    }
                    else
                    {
                        Debug.LogError($"Node cannot be created as type of associated narrative object is not known.\nName: {narrativeObject.name}.");

                        continue;
                    }

                    if (!viewContainerStack.Peek().ContainsNode(narrativeObjectNode.NarrativeObject.guid))
                    {
                        // Add the node to the visible container on the view stack.
                        viewContainerStack.Peek().AddNode(narrativeObjectNode.NarrativeObject.guid);
                    }

                    narrativeObjectNode.OnSetAsRoot += OnNarrativeObjectNodeSetAsRoot;

                    narrativeObjectNode.OnSetAsCandidate += OnNarrativeObjectNodeSetAsCandidate;
                    narrativeObjectNode.OnRemoveAsCandidate += OnNarrativeObjectNodeRemoveAsCandidate;

                    AddElement(narrativeObjectNode);
                }
                else
                {
                    narrativeObjectNode = existingNodesForNarrativeObject.First();

                    // Ensure this node is visible as it will definitely be in the correct location now.
                    narrativeObjectNode.visible = true;
                }

                // If a save graph view exists, try to find the properties for this node.
                if (graphViewState != null)
                {
                    NarrativeObjectNodeState nodeState = graphViewState.narrativeObjectNodeStates.FirstOrDefault((nodeState) => nodeState.narrativeObjectGuid == narrativeObject.guid);

                    // If a node state exists then restore the node with the correct values.
                    if (nodeState != null)
                    {
                        narrativeObjectNode.SetPosition(new Rect(nodeState.position, narrativeObjectNode.GetPosition().size));
                    }
                    else
                    {
                        // The current centre of the graph view window.
                        Vector2 graphViewCenter = contentViewContainer.WorldToLocal(layout.center);

                        // Store node as created.
                        populateResult.CreatedNodes.Add(new Tuple<string, Vector2>(narrativeObjectNode.NarrativeObject.guid, graphViewCenter));

                        // Make invisible to avoid popping onto screen at 0,0 before appearing at the centre of the graph view.
                        narrativeObjectNode.visible = false;

                        // Node state for this node doesn't exist. Graph view is different from it's save state.
                        populateResult.GraphViewChanged = true;
                    }
                }
                else
                {
                    // At least one node has been added and there is no save state so create one.
                    populateResult.GraphViewChanged = true;
                }
            }

            // Get the guid of the root narrative object of the view.
            string viewContainerRootNarrativeObjectGuid = GetViewContainerRootNarrativeObjectGuid(visibleViewContainer);

            // Find the nodes which are in the current view container. These will be rendered.
            IEnumerable<NarrativeObjectNode> visibleNarrativeObjectNodes = NarrativeObjectNodes.Where(narrativeObjectNode => visibleViewContainer.ContainsNode(narrativeObjectNode.NarrativeObject.guid));

            // For each node, make sure all edges exist.
            foreach (NarrativeObjectNode narrativeObjectNode in visibleNarrativeObjectNodes)
            {
                // If the node is the root of the current view.
                if (narrativeObjectNode.NarrativeObject.guid == viewContainerRootNarrativeObjectGuid)
                {
                    narrativeObjectNode.EnableRootVisuals();
                }

                // If the visible view is a representing a group, show candidate graphics on candidate nodes.
                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject as GroupNarrativeObject;

                    if (groupNarrativeObject.groupSelectionDecisionPoint.Candidates.Contains(narrativeObjectNode.NarrativeObject.gameObject))
                    {
                        narrativeObjectNode.EnableCandidateVisuals();
                    }
                }

                // For each connection from the nodes outputs.
                foreach (GameObject candidate in narrativeObjectNode.NarrativeObject.outputSelectionDecisionPoint.Candidates)
                {
                    NarrativeObject candidateNarrativeObject = candidate.GetComponent<NarrativeObject>();

                    // Find an edge state for the connection between the two nodes.
                    EdgeState edgeState = EdgeStates.Where(edgeState => edgeState.OutputNarrativeObjectNode.NarrativeObject.guid == narrativeObjectNode.NarrativeObject.guid && edgeState.InputNarrativeObjectNode.NarrativeObject.guid == candidateNarrativeObject.guid).FirstOrDefault();

                    // If no connection already exists.
                    if (edgeState == null)
                    {
                        // Get the node representing the input narrative object.
                        NarrativeObjectNode inputNarrativeObjectNode = NarrativeObjectNodes.Where(node => node.NarrativeObject.guid == candidateNarrativeObject.guid).FirstOrDefault();

                        // If the input node doesn't exist, something very wrong has happened somewhere!
                        if (inputNarrativeObjectNode == null)
                        {
                            Debug.LogError($"Narrative Object Node not found for guid: {candidate}");

                            continue;
                        }

                        // Create a new edge between the output port of the first narrative object node and the input port of the connected narrative object node.
                        Edge edge = new Edge()
                        {
                            output = narrativeObjectNode.OutputPort,
                            input = inputNarrativeObjectNode.InputPort,
                        };

                        edge.input.Connect(edge);
                        edge.output.Connect(edge);

                        // Store the edge state for this new edge.
                        EdgeStates.Add(new EdgeState { Edge = edge, InputNarrativeObjectNode = inputNarrativeObjectNode, OutputNarrativeObjectNode = narrativeObjectNode });

                        // Add edge to the graph view.
                        AddElement(edge);
                    }
                    else
                    {
                        if (!Contains(edgeState.Edge))
                        {
                            AddElement(edgeState.Edge);
                        }
                    }
                }
            }

            return populateResult;
        }

        /// <summary>
        /// Get the current Narrative Space instance within the scene.
        /// </summary>
        /// <returns></returns>
        private NarrativeSpace GetNarrativeSpace()
        {
            NarrativeSpace narrativeSpace = UnityEngine.Object.FindObjectOfType<NarrativeSpace>();

            if (narrativeSpace == null)
            {
                narrativeSpace = CuttingRoomContextMenus.CreateNarrativeSpace();
            }

            // Ensure sequencer exists.
            Sequencer sequencer = UnityEngine.Object.FindObjectOfType<Sequencer>();

            if (sequencer == null)
            {
                sequencer = CuttingRoomContextMenus.CreateSequencer();

                // Set narrative space on the sequencer.
                sequencer.narrativeSpace = narrativeSpace;
            }

            return narrativeSpace;
        }

        /// <summary>
        /// Get the narrative object with the specified guid.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private NarrativeObject GetNarrativeObject(string guid)
        {
            NarrativeObject[] narrativeObjects = UnityEngine.Object.FindObjectsOfType<NarrativeObject>();

            return narrativeObjects.Where(narrativeObject => narrativeObject.guid == guid).FirstOrDefault();
        }

        /// <summary>
        /// Invoked whenever a narrative object is set as root on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeSetAsRoot(NarrativeObjectNode narrativeObjectNode)
        {
            SetNarrativeObjectAsRootOfViewContainer(viewContainerStack.Peek(), narrativeObjectNode.NarrativeObject);

            // Invoke the callback as a root has changed somewhere on the graph.
            OnRootNarrativeObjectChanged?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a narrative object is Set as a candidate on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeSetAsCandidate(NarrativeObjectNode narrativeObjectNode)
        {
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // If the container is the root view, no candidates are allowed so ignore.
            if (visibleViewContainer.narrativeObjectGuid != rootViewContainerGuid)
            {
                NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                    groupNarrativeObject.groupSelectionDecisionPoint.AddCandidate(narrativeObjectNode.NarrativeObject.gameObject);

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Invoked whenever a narrative object is removed as a candidate on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnNarrativeObjectNodeRemoveAsCandidate(NarrativeObjectNode narrativeObjectNode)
        {
            ViewContainer visibleViewContainer = viewContainerStack.Peek();

            // If the container is the root view, no candidates are allowed so ignore.
            if (visibleViewContainer.narrativeObjectGuid != rootViewContainerGuid)
            {
                NarrativeObject visibleViewContainerNarrativeObject = GetNarrativeObject(visibleViewContainer.narrativeObjectGuid);

                if (visibleViewContainerNarrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = visibleViewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                    groupNarrativeObject.groupSelectionDecisionPoint.RemoveCandidate(narrativeObjectNode.NarrativeObject.gameObject);

                    OnNarrativeObjectCandidatesChanged?.Invoke();
                }
            }
        }

        /// <summary>
        /// Set the root narrative object of a view container.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <param name="narrativeObject"></param>
        /// <returns></returns>
        private bool SetNarrativeObjectAsRootOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            // If the current view is the narrative space.
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = GetNarrativeSpace();

                if (narrativeSpace == null)
                {
                    return false;
                }

                narrativeSpace.rootNarrativeObject = narrativeObject;
            }
            else
            {
                // Find the narrative object which has the same guid as the current view container.
                NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (viewContainerNarrativeObject == null)
                {
                    Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                    return false;
                }

                if (viewContainerNarrativeObject is GraphNarrativeObject)
                {
                    GraphNarrativeObject graphNarrativeObject = viewContainerNarrativeObject.GetComponent<GraphNarrativeObject>();

                    graphNarrativeObject.rootNarrativeObject = narrativeObject;
                }
                else if (viewContainerNarrativeObject is GroupNarrativeObject)
                {
                    // TODO: Do nothing but perhaps add as candidate?
                }
                else
                {
                    Debug.LogError("Cannot set root node of narrative object as type is unknown.");

                    return false;
                }
            }

            return true;
        }

        private bool AddNarrativeObjectAsCandidateOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            // Find the narrative object which has the same guid as the current view container.
            NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

            if (viewContainerNarrativeObject == null)
            {
                Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                return false;
            }

            if (viewContainerNarrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = viewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                groupNarrativeObject.groupSelectionDecisionPoint.AddCandidate(narrativeObject.gameObject);
            }
            else
            {
                Debug.LogError("Cannot add candidate as narrative object has no candidates.");

                return false;
            }

            return true;
        }

        private bool RemoveNarrativeObjectAsCandidateOfViewContainer(ViewContainer viewContainer, NarrativeObject narrativeObject)
        {
            // Find the narrative object which has the same guid as the current view container.
            NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

            if (viewContainerNarrativeObject == null)
            {
                Debug.LogError($"Narrative Object with guid {narrativeObject.guid} does not exist.");

                return false;
            }

            if (viewContainerNarrativeObject is GroupNarrativeObject)
            {
                GroupNarrativeObject groupNarrativeObject = viewContainerNarrativeObject.GetComponent<GroupNarrativeObject>();

                groupNarrativeObject.groupSelectionDecisionPoint.RemoveCandidate(narrativeObject.gameObject);
            }
            else
            {
                Debug.LogError("Cannot add candidate as narrative object has no candidates.");

                return false;
            }

            return true;

        }

        /// <summary>
        /// Query whether a view container has a root narrative object set.
        /// </summary>
        /// <param name="viewContainer"></param>
        /// <returns></returns>
        private bool ViewContainerHasRootNarrativeObject(ViewContainer viewContainer)
        {
            if (viewContainer.narrativeObjectGuid == rootViewContainerGuid)
            {
                NarrativeSpace narrativeSpace = GetNarrativeSpace();

                if (narrativeSpace == null)
                {
                    return false;
                }

                return narrativeSpace.rootNarrativeObject != null;
            }
            else
            {
                // Find the narrative object which has the same guid as the current view container.
                NarrativeObject viewContainerNarrativeObject = GetNarrativeObject(viewContainer.narrativeObjectGuid);

                if (viewContainerNarrativeObject == null)
                {
                    Debug.LogError($"Narrative Object with guid {viewContainer.narrativeObjectGuid} does not exist.");

                    return false;
                }

                if (viewContainerNarrativeObject is GraphNarrativeObject)
                {
                    GraphNarrativeObject graphNarrativeObject = viewContainerNarrativeObject.GetComponent<GraphNarrativeObject>();

                    return graphNarrativeObject.rootNarrativeObject != null;
                }
                else if (viewContainerNarrativeObject is GroupNarrativeObject)
                {
                    // TODO: No roots inside a group.

                    return false;
                }
                else
                {
                    Debug.LogError("Cannot determine if root exists as ");
                }
            }

            return false;
        }

    }
}
