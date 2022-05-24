using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
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
        private const string rootViewContainerGuid = "0";

        /// <summary>
        /// View stack for rendering the recursive layers of the narrative space.
        /// </summary>
        public Stack<BaseViewContainer> viewContainerStack = new Stack<BaseViewContainer>();

        /// <summary>
        /// The collection of containers which currently exists.
        /// These may or may not currently be on the view stack.
        /// </summary>
        public List<BaseViewContainer> viewContainers = new List<BaseViewContainer>();

        public CuttingRoomEditorGraphView(CuttingRoomEditorWindow window)
        {
            window.OnWindowCleared += OnWindowCleared;

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
            viewContainers.Add(new BaseViewContainer(rootViewContainerGuid));

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
        public NarrativeObjectNode AddNode<T>(NarrativeObject narrativeObject) where T : NarrativeObjectNode
        {
            // Careful here as the constructor is being reflected based on the parameters.
            // If parameters change, this method call will still be valid but fail to find ctor.
            NarrativeObjectNode narrativeObjectNode = Activator.CreateInstance(typeof(T), new object[] { narrativeObject }) as NarrativeObjectNode;

            NarrativeObjectNodes.Add(narrativeObjectNode);

            return narrativeObjectNode;
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
        /// Invoked when the graph view is changed.
        /// </summary>
        /// <param name="graphViewChange"></param>
        /// <returns></returns>
        private GraphViewChange OnGraphViewChangedEvent(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
            {
                Debug.LogError("Element has been deleted. Ensure that this is modifying the Narrative Space.");

                foreach (GraphElement graphElement in graphViewChange.elementsToRemove)
                {
                    // If an element has been removed.
                    if (graphElement is Edge)
                    {
                        // Find the edge
                        EdgeState deletedEdgeState = EdgeStates.Where(edgeState => edgeState.Edge == graphElement as Edge).FirstOrDefault();

                        if (deletedEdgeState == null)
                        {
                            Debug.LogError("EdgeState was not found when deleting edge. This should never happen.");

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
                        NarrativeObjectNodes.Remove(narrativeObjectNode);
                    }
                }
            }

            if (graphViewChange.edgesToCreate != null && graphViewChange.edgesToCreate.Count > 0)
            {
                Debug.LogError("Edge has been created. Ensure that this is modifying the Narrative Space.");

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
            BaseViewContainer viewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == narrativeObjectNode.NarrativeObject.guid).FirstOrDefault();

            if (viewContainer == null)
            {
                viewContainer = new BaseViewContainer(narrativeObjectNode.NarrativeObject.guid);

                viewContainers.Add(viewContainer);
            }

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
            IEnumerable<BaseViewContainer> viewContainersWithNarrativeObject = viewContainers.ToList().Where(viewContainer => viewContainer.ContainsNode(narrativeObject.guid));

            // If no view containers contain the narrative object, it has just been created and therefore should be visible.
            if (viewContainersWithNarrativeObject.Count() == 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Populate the graph view based on the contents of the scene currently open.
        /// </summary>
        /// <returns>Whether the graph view contents have been changed.</returns>
        public bool Populate(CuttingRoomEditorGraphViewState graphViewState, NarrativeObject[] narrativeObjects)
        {
            // Whether this method has altered the contents of the graph view.
            bool graphViewChanged = false;

            if (graphViewState != null)
            {
                // If the number of view containers is different to the save, then a view container has been added or removed
                // or if there view stack has been pushed/popped.
                // This change must be saved.
                if (viewContainers.Count != graphViewState.viewContainerStates.Count ||
                    viewContainerStack.Count != graphViewState.viewContainerStackGuids.Count)
                {
                    graphViewChanged = true;
                }

                // Ensure all view containers exist.
                foreach (ViewContainerState viewContainerState in graphViewState.viewContainerStates)
                {
                    BaseViewContainer baseViewContainer = viewContainers.Where(viewContainer => viewContainer.narrativeObjectGuid == viewContainerState.narrativeObjectGuid).FirstOrDefault();

                    if (baseViewContainer == null)
                    {
                        viewContainers.Add(new BaseViewContainer(viewContainerState.narrativeObjectGuid) { narrativeObjectNodeGuids = viewContainerState.narrativeObjectNodeGuids });
                    }
                }
            }

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
                        narrativeObjectNode = AddNode<AtomicNarrativeObjectNode>(narrativeObject);
                    }
                    else if (narrativeObject is GraphNarrativeObject)
                    {
                        narrativeObjectNode = AddNode<GraphNarrativeObjectNode>(narrativeObject);

                        GraphNarrativeObjectNode graphNarrativeObjectNode = narrativeObjectNode as GraphNarrativeObjectNode;

                        graphNarrativeObjectNode.OnClickViewContents -= PushViewContainer;
                        graphNarrativeObjectNode.OnClickViewContents += PushViewContainer;
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

                    AddElement(narrativeObjectNode);
                }
                else
                {
                    narrativeObjectNode = existingNodesForNarrativeObject.First();
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
                        // Node state for this node doesn't exist. Graph view is different from it's save state.
                        graphViewChanged = true;
                    }
                }
                else
                {
                    // At least one node has been added and there is no save state so create one.
                    graphViewChanged = true;
                }
            }

            // Find the nodes which are in the current view container. These will be rendered.
            IEnumerable<NarrativeObjectNode> visibleNarrativeObjectNodes = NarrativeObjectNodes.Where(narrativeObjectNode => viewContainerStack.Peek().ContainsNode(narrativeObjectNode.NarrativeObject.guid));

            // For each node, make sure all edges exist.
            foreach (NarrativeObjectNode narrativeObjectNode in visibleNarrativeObjectNodes)
            {
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
                            Debug.LogError($"Narrative Object Node not found for guid: {candidateNarrativeObject.guid}");

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

            return graphViewChanged;
        }
    }
}
