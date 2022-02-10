using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CuttingRoom.Editor
{
	public class NarrativeSpaceEditor : EditorWindow
	{
		#region Const/Readonly

		/// <summary>
		/// Button index for the left mouse button.
		/// </summary>
		private const int LeftMouseButton = 0;

		/// <summary>
		/// Button index for the right mouse button.
		/// </summary>
		private const int RightMouseButton = 1;

		/// <summary>
		/// Size of NarrativeObjectNode on canvas.
		/// </summary>
		private readonly Vector2 defaultNodeSize = new Vector2(200.0f, 100.0f);

		/// <summary>
		/// Top left corner coordinates to ensure a 10px margin.
		/// </summary>
		private readonly Vector2 topLeftCorner = new Vector2(10.0f, 25.0f);

		// 2.25 as there is an object node and an output node for each.
		private Vector2 defaultNarrativeObjectNodeSpacing = Vector2.zero;

		#endregion

		/// <summary>
		/// Reference to this window.
		/// </summary>
		private static NarrativeSpaceEditor narrativeSpaceEditor = null;

		/// <summary>
		/// The current narrative space being edited.
		/// </summary>
		public NarrativeSpace narrativeSpace { get; private set; } = null;

		/// <summary>
		/// Nodes in this window.
		/// </summary>
		private List<BaseNode> nodes = new List<BaseNode>();

		/// <summary>
		/// Dictionary of narrative object nodes against their decision points.
		/// </summary>
		private Dictionary<NarrativeObjectNode, List<DecisionPointNode>> narrativeObjectNodeDecisionPoints = new Dictionary<NarrativeObjectNode, List<DecisionPointNode>>();

		/// <summary>
		/// Nodes which have been marked for deletion by the user.
		/// Must be deleted once the drawing logic for the windows is complete, hence caching in a list.
		/// </summary>
		private List<BaseNode> deletedNodes = new List<BaseNode>();

		/// <summary>
		/// Current mouse position within the editor window.
		/// </summary>
		private Vector2 mousePosition = Vector2.zero;

		/// <summary>
		/// Position drag starts.
		/// </summary>
		private Vector2 lastMouseDragPosition = Vector2.zero;

		/// <summary>
		/// Currently selected node.
		/// </summary>
		private BaseNode selectedNode = null;

		/// <summary>
		/// When adding a candidate, this is the decision point which it will be added to.
		/// </summary>
		private DecisionPointNode decisionPointNodeAddingCandidate = null;

		/// <summary>
		/// Container for information about a render level currently on the view stack.
		/// </summary>
		private class ViewStackInfo
		{
			public RenderMode renderMode = RenderMode.Undefined;

			public BaseNode baseNode = null;
		}

		/// <summary>
		/// The view stack for the window. This is used to record a hierarchy for rendering from inside other nodes.
		/// </summary>
		private Stack<ViewStackInfo> viewStack = new Stack<ViewStackInfo>();

		/// <summary>
		/// Enumerated values for ContextCallback method, to indicate the action to take.
		/// </summary>
		private enum ContextActions
		{
			Undefined,
			AddAtomicNarrativeObject,
			AddGroupNarrativeObject,
			AddLayerNarrativeObject,
			DeleteSelectedNode,
			AddCandidate,
			RemoveCandidate,
			AddGraphNarrativeObject,
		}

		/// <summary>
		/// Shows this editor window.
		/// </summary>

		// JG: Removed this as non functional and I dont want users trying it and getting errors (I know it has errors but I wont fix it due to BetaJester reimplementing).
		//[MenuItem("Cutting Room/Narrative Space Editor")]
		private static void ShowNarrativeSpaceEditor()
		{
			narrativeSpaceEditor = GetWindow<NarrativeSpaceEditor>("Narrative Space Editor");
			narrativeSpaceEditor.minSize = new Vector2(1024.0f, 576.0f);
		}

		/// <summary>
		/// Called when window is opened.
		/// </summary>
		private void Awake()
		{
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			Undo.undoRedoPerformed += OnUndoRedoPerformed;
			EditorApplication.hierarchyChanged += OnEditorApplicationHierarchyChanged;

			defaultNarrativeObjectNodeSpacing = new Vector2(defaultNodeSize.x * 3.0f, defaultNodeSize.y * 1.5f);

			// Ensure that there is the root view on the view stack.
			InitialiseViewStack();
		}

		/// <summary>
		/// Called when window is closed.
		/// </summary>
		private void OnDestroy()
		{
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;
			Undo.undoRedoPerformed -= OnUndoRedoPerformed;
			EditorApplication.hierarchyChanged -= OnEditorApplicationHierarchyChanged;
		}

		/// <summary>
		/// Validates the scene and ensures all CuttingRoom content is set up.
		/// </summary>
		private void ValidateScene()
		{
			// Find all decision points and validate.
			DecisionPoint[] decisionPoints = FindObjectsOfType<DecisionPoint>();

			for (int decisionPointCount = 0; decisionPointCount < decisionPoints.Length; decisionPointCount++)
			{
				// Validate the decision points.
				decisionPoints[decisionPointCount].Validate();
			}
		}

		/// <summary>
		/// Recreates the scene from scratch.
		/// </summary>
		private void Refresh()
		{
			ValidateScene();

			GenerateNarrativeSpaceGraph();
		}

		/// <summary>
		/// Called when undo or redo operation occurs in the editor.
		/// </summary>
		private void OnUndoRedoPerformed()
		{
			Refresh();
		}

		/// <summary>
		/// Called when the hierarchy is changed in the scene.
		/// </summary>
		private void OnEditorApplicationHierarchyChanged()
		{
				Refresh();
		}

		/// <summary>
		/// Used to parse the new scene and generate the graph related to it.
		/// </summary>
		/// <param name="previousScene"></param>
		/// <param name="currentScene"></param>
		private void OnActiveSceneChanged(Scene previousScene, Scene currentScene)
		{
			Refresh();
		}

		private enum RenderMode
		{
			Undefined,
			Overview,
			Group,
			Layer,
		}

		/// <summary>
		/// Method to generate the narrative graph.
		/// </summary>
		private void GenerateNarrativeSpaceGraph()
		{
			narrativeSpace = FindNarrativeSpace();

			if (narrativeSpace == null)
			{
				Debug.LogError("The active scene in the editor has no Narrative Space object. Please add one and try again.");

				return;
			}

			// Ensure the view stack is valid before rendering. Should always have something in it!
			if (viewStack.Count == 0)
			{
				InitialiseViewStack();
			}

			// Clear the nodes list to get rid of existing views.
			nodes.Clear();

			Dictionary<Type, List<BaseNode>> generatedNodes = new Dictionary<Type, List<BaseNode>>()
			{
				{ typeof(NarrativeObjectNode), new List<BaseNode>() },
				{ typeof(OutputSelectionDecisionPointNode), new List<BaseNode>() },
				{ typeof(GroupSelectionDecisionPointNode), new List<BaseNode>() },
			};

			Type narrativeObjectNodeType = typeof(NarrativeObjectNode);

			// Find all narrative object components in scene.
			NarrativeObject[] narrativeObjects = FindObjectsOfType<NarrativeObject>();

			// Loop through all Narrative objects and create nodes for them in the graph.
			for (int narrativeObjectCount = 0; narrativeObjectCount < narrativeObjects.Length; narrativeObjectCount++)
			{
				generatedNodes[narrativeObjectNodeType].Add(CreateNodeFromNarrativeObject(narrativeObjects[narrativeObjectCount]));
			}

			for (int index = 0; index < generatedNodes[narrativeObjectNodeType].Count; index++)
			{
				// Get the base node being examined.
				BaseNode baseNode = generatedNodes[narrativeObjectNodeType][index];

				// If it represents a group.
				if (baseNode is GroupNarrativeObjectNode)
				{
					GroupNarrativeObjectNode groupNarrativeObjectNode = baseNode as GroupNarrativeObjectNode;

					// Get the group selection candidates.
					NarrativeObject[] groupSelectionCandidates = groupNarrativeObjectNode.groupNarrativeObject.groupSelectionDecisionPoint.GetCandidatesAsNarrativeObjects();

					// For each candidate.
					for (int groupSelectionCandidateCount = 0; groupSelectionCandidateCount < groupSelectionCandidates.Length; groupSelectionCandidateCount++)
					{
						// Find the matching base node view.
						for (int nodeIndex = 0; nodeIndex < generatedNodes[narrativeObjectNodeType].Count; nodeIndex++)
						{
							// Get the narrative object node.
							NarrativeObjectNode narrativeObjectNode = generatedNodes[narrativeObjectNodeType][nodeIndex] as NarrativeObjectNode;

							// If the node is for the object we are looking for.
							if (groupSelectionCandidates[groupSelectionCandidateCount] == narrativeObjectNode.narrativeObject)
							{
								// Set this nodes parent as the group it is contained within.
								narrativeObjectNode.parentBaseNode = groupNarrativeObjectNode;
							}
						}
					}
				}
				else if (baseNode is LayerNarrativeObjectNode)
				{
					LayerNarrativeObjectNode layerNarrativeObjectNode = baseNode as LayerNarrativeObjectNode;

					// TODO: Set layer parents here.
				}
			}

			// Iterate all of the generated narrative object nodes.
			for (int narrativeObjectNodeCount = generatedNodes[narrativeObjectNodeType].Count - 1; narrativeObjectNodeCount >= 0 ; narrativeObjectNodeCount--)
			{
				// If the generated nodes parent doesnt equal the current viewStacks base node.
				if (generatedNodes[narrativeObjectNodeType][narrativeObjectNodeCount].parentBaseNode != viewStack.Peek().baseNode)
				{
					// Remove it as it shouldnt be rendered in any form.
					generatedNodes[narrativeObjectNodeType].RemoveAt(narrativeObjectNodeCount);
				}
			}

			switch (viewStack.Peek().renderMode)
			{
				case RenderMode.Overview:

					GenerateOverviewGraph(generatedNodes);

					break;

				case RenderMode.Group:

					GenerateGroupViewGraph(generatedNodes);

					break;

				case RenderMode.Layer:

					GenerateLayerGraph(generatedNodes);

					break;
			}

			// Populate the nodes list.
			foreach (KeyValuePair<Type, List<BaseNode>> pair in generatedNodes)
			{
				nodes.AddRange(pair.Value);
			}

			Repaint();
		}

		private void GenerateOverviewGraph(Dictionary<Type, List<BaseNode>> generatedNodes)
		{
			// Type for a narrative object node.
			Type narrativeObjectNodeType = typeof(NarrativeObjectNode);

			// Generate output selection decision points for the graph.
			GenerateOutputSelectionDecisionPointNodes(generatedNodes);

			NarrativeObject root = narrativeSpace.rootNarrativeObject;

			// Info about rendered graph section starting from root.
			GraphInfo rootGraphInfo = new GraphInfo();

			if (root != null)
			{
				// Use 10.0f, 25.0f for the start position - 10 in from left and 25 down (toolbar is around 15px tall).
				rootGraphInfo = RenderGraphFromNarrativeObject(root, generatedNodes, narrativeObjectNodeType, topLeftCorner, defaultNarrativeObjectNodeSpacing);
			}

			// This list will hold the roots of the orphaned objects (not connected to narrative space root).
			List<NarrativeObjectNode> orphanedNarrativeObjectNodeRoots = new List<NarrativeObjectNode>();

			// Get the nodes which are orphans (remove the nodes connected to root from list of all nodes).
			IEnumerable<BaseNode> orphanedBaseNodes = generatedNodes[narrativeObjectNodeType].Except(rootGraphInfo.linkedBaseNodes);

			Queue<NarrativeObjectNode> orphanedNarrativeObjectNodes = new Queue<NarrativeObjectNode>();

			// Add all orphans to a queue for processing one by one.
			foreach (BaseNode baseNode in orphanedBaseNodes)
			{
				if (baseNode is NarrativeObjectNode)
				{
					orphanedNarrativeObjectNodes.Enqueue(baseNode as NarrativeObjectNode);
				}
			}

			while (orphanedNarrativeObjectNodes.Count > 0)
			{
				// Get the orphaned narrative object node.
				NarrativeObjectNode narrativeObjectNode = orphanedNarrativeObjectNodes.Dequeue();

				bool isAnOutput = false;

				// For all other orphans.
				foreach (BaseNode baseNode in orphanedBaseNodes)
				{
					// If not the same node.
					if (baseNode != narrativeObjectNode)
					{
						// Check whether this object has the target object as an output. If it does, we know this is not a narrative object root.
						NarrativeObjectNode possibleInputNarrativeObjectNode = baseNode as NarrativeObjectNode;

						if (possibleInputNarrativeObjectNode.narrativeObject.outputSelectionDecisionPoint.HasCandidate(narrativeObjectNode.narrativeObject.gameObject))
						{
							isAnOutput = true;

							break;
						}
					}
				}

				if (!isAnOutput)
				{
					orphanedNarrativeObjectNodeRoots.Add(narrativeObjectNode);
				}
			}

			Vector2 orphanGraphPosition = new Vector2(topLeftCorner.x, rootGraphInfo.position.y + rootGraphInfo.size.y);

			for (int orphan = 0; orphan < orphanedNarrativeObjectNodeRoots.Count; orphan++)
			{
				GraphInfo orphanGraphInfo = RenderGraphFromNarrativeObject(orphanedNarrativeObjectNodeRoots[orphan].narrativeObject, generatedNodes, narrativeObjectNodeType, orphanGraphPosition, defaultNarrativeObjectNodeSpacing);

				orphanGraphPosition = new Vector2(topLeftCorner.x, orphanGraphInfo.position.y + orphanGraphInfo.size.y);
			}
		}

		private void GenerateOutputSelectionDecisionPointNodes(Dictionary<Type, List<BaseNode>> generatedNodes)
		{
			Type narrativeObjectNodeType = typeof(NarrativeObjectNode);
			Type outputSelectionDecisionPointNodeType = typeof(OutputSelectionDecisionPointNode);

			// Generate output decision point nodes.
			for (int nodeCount = 0; nodeCount < generatedNodes[narrativeObjectNodeType].Count; nodeCount++)
			{
				if (generatedNodes[narrativeObjectNodeType][nodeCount] is NarrativeObjectNode)
				{
					// The narrative object node to be setup.
					NarrativeObjectNode narrativeObjectNode = generatedNodes[narrativeObjectNodeType][nodeCount] as NarrativeObjectNode;

					// Create the output selection node.
					OutputSelectionDecisionPointNode outputSelectionDecisionPointNode = CreateOutputSelectionDecisionPointNode(narrativeObjectNode.narrativeObject.outputSelectionDecisionPoint, narrativeObjectNode.position, defaultNodeSize, defaultNodeSize);

					// Ensure this narrative object is in the dictionary.
					if (!narrativeObjectNodeDecisionPoints.ContainsKey(narrativeObjectNode))
					{
						narrativeObjectNodeDecisionPoints.Add(narrativeObjectNode, new List<DecisionPointNode>());
					}

					// Add the decision point against the narrative object in the dictionary.
					narrativeObjectNodeDecisionPoints[narrativeObjectNode].Add(outputSelectionDecisionPointNode);

					// Link the output selection node to the narrative object node.
					narrativeObjectNode.SetOutputSelectionDecisionPointNode(outputSelectionDecisionPointNode);

					// Container for candidate nodes of the narrative object node.
					List<NarrativeObjectNode> outputSelectionCandidateNodes = new List<NarrativeObjectNode>();

					// Get the candidate narrative objects.
					NarrativeObject[] candidateNarrativeObjects = outputSelectionDecisionPointNode.outputSelectionDecisionPoint.GetCandidatesAsNarrativeObjects();

					// For each candidate, we need to find the node associated with it.
					for (int candidateCount = 0; candidateCount < candidateNarrativeObjects.Length; candidateCount++)
					{
						// Get each candidate as a narrative object.
						NarrativeObject candidateNarrativeObject = candidateNarrativeObjects[candidateCount];

						// For the objects in the nodes list of narrative object node type.
						for (int narrativeObjectNodeCount = 0; narrativeObjectNodeCount < generatedNodes[narrativeObjectNodeType].Count; narrativeObjectNodeCount++)
						{
							if (generatedNodes[narrativeObjectNodeType][narrativeObjectNodeCount] is NarrativeObjectNode)
							{
								NarrativeObjectNode possibleCandidateNarrativeObjectNode = generatedNodes[narrativeObjectNodeType][narrativeObjectNodeCount] as NarrativeObjectNode;

								if (possibleCandidateNarrativeObjectNode.narrativeObject == candidateNarrativeObject)
								{
									outputSelectionCandidateNodes.Add(possibleCandidateNarrativeObjectNode);
								}
							}
						}
					}

					// Add the candidate node list to the output decision point so that it can render links.
					outputSelectionDecisionPointNode.SetCandidateNodes(outputSelectionCandidateNodes);

					// Keep the output selection decision point node in a list to be added to nodes later!
					generatedNodes[outputSelectionDecisionPointNodeType].Add(outputSelectionDecisionPointNode);
				}
			}
		}

		private void GenerateGroupViewGraph(Dictionary<Type, List<BaseNode>> generatedNodes)
		{
			// Peek at the stack to find the group we are inside.
			GroupNarrativeObjectNode groupNarrativeObjectNode = viewStack.Peek().baseNode as GroupNarrativeObjectNode;

			// Render a group selection node.
			GroupSelectionDecisionPointNode groupSelectionDecisionPointNode = CreateInstance(typeof(GroupSelectionDecisionPointNode)) as GroupSelectionDecisionPointNode;

			groupSelectionDecisionPointNode.Init(this, groupNarrativeObjectNode.groupNarrativeObject.groupSelectionDecisionPoint, topLeftCorner, defaultNodeSize);

			// Add the node for group selection.
			generatedNodes[typeof(GroupSelectionDecisionPointNode)].Add(groupSelectionDecisionPointNode);

			// Render each candidate and its graph.

			// Get all candidates of the group selection.
			NarrativeObject[] candidates = groupNarrativeObjectNode.groupNarrativeObject.groupSelectionDecisionPoint.GetCandidatesAsNarrativeObjects();

			List<NarrativeObject> outputsOfGroupSelectionCandidates = new List<NarrativeObject>();

			// For each candidate, generate the graph of its outputs.
			for (int candidateCount = 0; candidateCount < candidates.Length; candidateCount++)
			{
				GenerateGraphFromNarrativeObject(candidates[candidateCount], outputsOfGroupSelectionCandidates);
			}

			List<NarrativeObjectNode> candidateNodes = new List<NarrativeObjectNode>();

			// Create views for just the candidates.
			for (int candidateCount = 0; candidateCount < candidates.Length; candidateCount++)
			{
				candidateNodes.Add(CreateNodeFromNarrativeObject(candidates[candidateCount]) as NarrativeObjectNode);
			}

			// Link the group selection point to the candidate nodes.
			groupSelectionDecisionPointNode.SetCandidateNodes(candidateNodes);

			// Add these nodes to the generated nodes dictionary.
			generatedNodes[typeof(NarrativeObjectNode)].AddRange(candidateNodes);

			// The narrative objects which are not candidates of the group.
			IEnumerable<NarrativeObject> nonCandidateNarrativeObjects = outputsOfGroupSelectionCandidates.Except(candidates);

			// For all output narrative objects, generate the output selection points.
			foreach (NarrativeObject narrativeObject in nonCandidateNarrativeObjects)
			{
				generatedNodes[typeof(NarrativeObjectNode)].Add(CreateNodeFromNarrativeObject(narrativeObject));
			}

			// Generate the output selection nodes.
			GenerateOutputSelectionDecisionPointNodes(generatedNodes);

			float xPosition = topLeftCorner.x + defaultNarrativeObjectNodeSpacing.x;

			// Move right of the group selection node.
			Vector2 graphPosition = new Vector2(xPosition, topLeftCorner.y);

			for (int candidateCount = 0; candidateCount < candidates.Length; candidateCount++)
			{
				GraphInfo graphInfo = RenderGraphFromNarrativeObject(candidates[candidateCount], generatedNodes, typeof(NarrativeObjectNode), graphPosition, defaultNarrativeObjectNodeSpacing);

				graphPosition = new Vector2(xPosition, graphInfo.position.y + graphInfo.size.y);
			}
		}

		private void GenerateLayerGraph(Dictionary<Type, List<BaseNode>> generatedNodes)
		{
		}

		private class GraphInfo
		{
			public List<BaseNode> linkedBaseNodes = new List<BaseNode>();

			public Vector2 position = Vector2.zero;
			public Vector2 size = Vector2.zero;
		}

		private GraphInfo RenderGraphFromNarrativeObject(NarrativeObject narrativeObject, Dictionary<Type, List<BaseNode>> generatedNodes, Type generatedNodeType, Vector2 startPosition, Vector2 spacing)
		{
			GraphInfo graphInfo = new GraphInfo();

			Dictionary<int, List<NarrativeObject>> graphNarrativeObjectDepths = new Dictionary<int, List<NarrativeObject>>();

			// Get the layout of the nodes as a depth wise tree.
			GenerateNarrativeSpaceGraphDepth(narrativeObject, 0, graphNarrativeObjectDepths);

			// X position of the current depth being rendered.
			float xPosition = startPosition.x;
			float yPosition = startPosition.y;

			float largestYPosition = startPosition.y;

			// For each level of depth, create the nodes.
			foreach (KeyValuePair<int, List<NarrativeObject>> pair in graphNarrativeObjectDepths)
			{
				// Y position of this column.
				yPosition = startPosition.y;

				for (int narrativeObjectCount = 0; narrativeObjectCount < pair.Value.Count; narrativeObjectCount++)
				{
					NarrativeObject narrativeObjectAtDepth = pair.Value[narrativeObjectCount];

					bool nodeWasPositioned = false;

					for (int narrativeObjectNodeCount = 0; narrativeObjectNodeCount < generatedNodes[generatedNodeType].Count; narrativeObjectNodeCount++)
					{
						NarrativeObjectNode narrativeObjectNode = generatedNodes[generatedNodeType][narrativeObjectNodeCount] as NarrativeObjectNode;

						// If this node is for the object we are examining.
						if (narrativeObjectNode.narrativeObject == narrativeObjectAtDepth)
						{
							if (!narrativeObjectNode.InitialPositionSet)
							{
								nodeWasPositioned = true;

								narrativeObjectNode.SetPosition(new Vector2(xPosition, yPosition));
								narrativeObjectNode.SetSize(defaultNodeSize);

								// If the list of objects connected to root doesnt have this narrative object.
								if (!graphInfo.linkedBaseNodes.Contains(narrativeObjectNode))
								{
									graphInfo.linkedBaseNodes.Add(narrativeObjectNode);
								}
							}

							break;
						}
					}

					if (nodeWasPositioned)
					{
						yPosition += spacing.y;
					}

					if (yPosition > largestYPosition)
					{
						largestYPosition = yPosition;
					}
				}

				xPosition += spacing.x;
			}

			graphInfo.position = startPosition;
			graphInfo.size = new Vector2(xPosition - startPosition.x, largestYPosition - startPosition.y);

			return graphInfo;
		}

		private void GenerateGraphFromNarrativeObject(NarrativeObject narrativeObject, List<NarrativeObject> narrativeObjects)
		{
			// Add the object itself.
			if (!narrativeObjects.Contains(narrativeObject))
			{
				narrativeObjects.Add(narrativeObject);
			}

			NarrativeObject[] outputNarrativeObjects = narrativeObject.outputSelectionDecisionPoint.GetCandidatesAsNarrativeObjects();

			for (int candidateCount = 0; candidateCount < outputNarrativeObjects.Length; candidateCount++)
			{
				GenerateGraphFromNarrativeObject(outputNarrativeObjects[candidateCount], narrativeObjects);
			}
		}

		private void GenerateNarrativeSpaceGraphDepth(NarrativeObject narrativeObject, int depth, Dictionary<int, List<NarrativeObject>> graphNarrativeObjectDepths)
		{
			// Check whether this depth exists already in the dictionary, if not then add it.
			if (!graphNarrativeObjectDepths.ContainsKey(depth))
			{
				graphNarrativeObjectDepths.Add(depth, new List<NarrativeObject>());
			}

			// Add this narrative object to the depth it lies at.
			graphNarrativeObjectDepths[depth].Add(narrativeObject);

			NarrativeObject[] outputSelectionDecisionPointCandidateNarrativeObjects = narrativeObject.outputSelectionDecisionPoint.GetCandidatesAsNarrativeObjects();

			for (int outputCount = 0; outputCount < outputSelectionDecisionPointCandidateNarrativeObjects.Length; outputCount++)
			{
				GenerateNarrativeSpaceGraphDepth(outputSelectionDecisionPointCandidateNarrativeObjects[outputCount], depth + 1, graphNarrativeObjectDepths);
			}
		}

		private BaseNode CreateNodeFromNarrativeObject(NarrativeObject narrativeObject)
		{
			if (narrativeObject is AtomicNarrativeObject)
			{
				return AddAtomicNarrativeObjectNode(narrativeObject as AtomicNarrativeObject);
			}
			else if (narrativeObject is GroupNarrativeObject)
			{
				return AddGroupNarrativeObjectNode(narrativeObject as GroupNarrativeObject);
			}
			else if (narrativeObject is LayerNarrativeObject)
			{
				return AddLayerNarrativeObjectNode(narrativeObject as LayerNarrativeObject);
			}

			return null;
		}

		private NarrativeSpace FindNarrativeSpace()
		{
			return FindObjectOfType<NarrativeSpace>();
		}

		private void OnGUI()
		{
			// Get the current event.
			Event e = Event.current;

			// Save the current mouse position.
			mousePosition = e.mousePosition;

			// Do input based actions from the current event.
			ProcessInput(e);

			// Remove any nodes which were marked for deletion after processing last update.
			DeleteNodes();

			// Render links between nodes.
			DrawLinksBetweenNodes();

			// Render the new nodes.
			DrawNodes();

			// Render toolbar.
			DrawToolbar(e);
		}

		private void DrawToolbar(Event e)
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Back", EditorStyles.toolbarButton))
			{
				PopFromViewStack();

				// Event is "used"
				e.Use();
			}

			GUILayout.Space(10.0f);

			if (GUILayout.Button("Generate Narrative Space", EditorStyles.toolbarButton))
			{
				FindNarrativeSpace();

				if (narrativeSpace == null)
				{
					Debug.LogError("Narrative Space Editor requires a NarrativeSpace component within the scene. Please add one and try again.");
				}

				Refresh();

				// Event is "used"
				e.Use();
			}

			EditorGUILayout.EndHorizontal();
		}

		private void DrawNodes()
		{
			BeginWindows();

			for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
			{
				BaseNode node = nodes[nodeIndex];

				// Get the rect for the window node.
				node.windowRect = GUI.Window(nodeIndex, node.windowRect, DrawNode, node.windowTitle);
			}

			EndWindows();
		}

		private void DrawNode(int nodeIndex)
		{
			nodes[nodeIndex].DrawWindow(new GUIRenderingUtilities.RenderSettings() { position = new Vector2(5.0f, 20.0f), guiConstraints = new GUIRenderingUtilities.Constraints { lockedX = true } });
			GUI.DragWindow();
		}

		private void DrawLinksBetweenNodes()
		{
			for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
			{
				if (nodes[nodeIndex] is NarrativeObjectNode)
				{
					// Get the narrative object node.
					NarrativeObjectNode narrativeObjectNode = (NarrativeObjectNode)nodes[nodeIndex];

					// Render a link to the output selection decision point.
					DrawLineLink(narrativeObjectNode.windowRect, narrativeObjectNode.outputSelectionDecisionPointNode.windowRect);
				}
				else if (nodes[nodeIndex] is DecisionPointNode)
				{
					DecisionPointNode decisionPointNode = nodes[nodeIndex] as DecisionPointNode;

					// Render a link from decision point to candidates.
					for (int candidateNode = 0; candidateNode < decisionPointNode.candidateNodes.Count; candidateNode++)
					{
						DrawBezierLink(decisionPointNode.windowRect, decisionPointNode.candidateNodes[candidateNode].windowRect);
					}
				}
			}
		}

		private void DrawLineLink(Rect start, Rect end)
		{
			Vector3 startPosition = GetStartPositionFromRect(start);

			Vector3 endPosition = GetEndPositionFromRect(end);

			Handles.color = Color.black;

			Handles.DrawAAPolyLine(2.0f, 2, startPosition, endPosition);
		}

		private void DrawBezierLink(Rect start, Rect end)
		{
			Vector3 startPosition = GetStartPositionFromRect(start);

			Vector3 endPosition = GetEndPositionFromRect(end);

			DrawBezierLink(startPosition, endPosition);
		}

		private void DrawBezierLink(Vector3 startPosition, Vector3 endPosition)
		{
			Vector3 startTangent = startPosition + Vector3.right * 100;

			Vector3 endTangent = endPosition + Vector3.left * 100;

			Handles.DrawBezier(startPosition, endPosition, startTangent, endTangent, Color.black, null, 2.0f);
		}

		private Vector3 GetStartPositionFromRect(Rect start)
		{
			Vector3 startPosition = new Vector3(start.x + start.width, start.y + (start.height * 0.5f), 0.0f);

			return startPosition;
		}

		private Vector3 GetEndPositionFromRect(Rect end)
		{
			Vector3 endPosition = new Vector3(end.x, end.y + (end.height * 0.5f), 0.0f);

			return endPosition;
		}

		private void DeleteNodes()
		{
			// Loop through deleted indices and remove from nodes list, so they arent drawn (will be GC'd)
			for (int deletedIndex = 0; deletedIndex < deletedNodes.Count; deletedIndex++)
			{
				nodes.Remove(deletedNodes[deletedIndex]);
			}

			// Clear the deleted nodes list as they have now been removed.
			deletedNodes.Clear();
		}

		private void ProcessInput(Event e)
		{
			// TODO: e.type can be ContextClick (right click on windows, control click on mac). Handle this!

			// Look for mouse down events.
			if (e.type == EventType.MouseDown)
			{
				// Get the selected node.
				selectedNode = GetSelectedNode();

				if (selectedNode != null)
				{
					if (selectedNode is NarrativeObjectNode)
					{
						NarrativeObjectNode narrativeObjectNode = selectedNode as NarrativeObjectNode;

						Selection.activeGameObject = narrativeObjectNode.narrativeObject.gameObject;
					}
					else if (selectedNode is DecisionPointNode)
					{
						DecisionPointNode decisionPointNode = selectedNode as DecisionPointNode;

						// Iterate the dictionaty with narrative objects against their decision points.
						foreach (KeyValuePair<NarrativeObjectNode, List<DecisionPointNode>> pair in narrativeObjectNodeDecisionPoints)
						{
							// If the list of decision points contains the selected decision point node.
							if (pair.Value.Contains(decisionPointNode))
							{
								// Select the narrative object it is paired with.
								Selection.activeGameObject = pair.Key.narrativeObject.gameObject;

								break;
							}
						}
					}
				}

				if (e.button == LeftMouseButton)
				{
					// Store where the mouse button went down.
					lastMouseDragPosition = mousePosition;

					LeftClick(e);
				}
				else if (e.button == RightMouseButton)
				{
					RightClick(e);
				}

				// Force editor to repaint.
				Repaint();
			}
			else if (e.type == EventType.MouseDrag)
			{
				if (e.button == LeftMouseButton)
				{
					LeftDrag(e);
				}

				// Force editor to repaint.
				Repaint();
			}
			else if (e.type == EventType.KeyDown)
			{
				if (e.keyCode == KeyCode.Delete)
				{
					DeleteKey(e);
				}
			}

			// If adding a decision point candidate, then draw a link between that object and the mouse to indicate that process is taking place.
			if (decisionPointNodeAddingCandidate != null)
			{
				DrawBezierLink(GetStartPositionFromRect(decisionPointNodeAddingCandidate.windowRect), mousePosition);

				// Force a repaint to stick the end position to the cursor position.
				Repaint();
			}
		}

		private void LeftClick(Event e)
		{
			FinishAddCandidate();
		}

		private void RightClick(Event e)
		{
			if (selectedNode != null)
			{
				ModifyNode(e);
			}
			else
			{
				AddNode(e);
			}
		}

		private void LeftDrag(Event e)
		{
			if (selectedNode != null)
			{
				PanNode(e);
			}
			else
			{
				PanAllNodes(e);
			}
		}

		private void RightDrag(Event e)
		{
		}

		private void DeleteKey(Event e)
		{
			if (selectedNode != null)
			{
				DeleteSelectedNode();

				e.Use();
			}
		}

		private void DeleteSelectedNode()
		{
			if (selectedNode is NarrativeObjectNode)
			{
				// Get the narrative object node.
				NarrativeObjectNode narrativeObjectNode = selectedNode as NarrativeObjectNode;

				// Delete the nodes in the graph for that node.
				deletedNodes.Add(narrativeObjectNode);
				deletedNodes.Add(narrativeObjectNode.outputSelectionDecisionPointNode);

				// TODO: Groups and Layers will have extra nodes to delete here due to group selection/termination and layer selection.

				// Destroy the game object in the scene for the Narrative Object.
				// Also registers an undo step.
				Undo.DestroyObjectImmediate(narrativeObjectNode.narrativeObject.gameObject);
			}
		}

		private void AddNode(Event e)
		{
			GenericMenu genericMenu = new GenericMenu();

			// Add an item for Add Node. This calls ContextCallback when clicked with the argument ContextActions.AddNode.
			genericMenu.AddItem(new GUIContent("Add Atomic Narrative Object"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.AddAtomicNarrativeObject });
			genericMenu.AddItem(new GUIContent("Add Group Narrative Object"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.AddGroupNarrativeObject });
			genericMenu.AddItem(new GUIContent("Add Layer Narrative Object"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.AddLayerNarrativeObject });
			genericMenu.AddItem(new GUIContent("Add Graph Narrative Object"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.AddGraphNarrativeObject });
			
			// Show the menu.
			genericMenu.ShowAsContext();

			// Mark this event as consumed by this editor window.
			// This prevents multiple windows from reacting to the same events.
			e.Use();
		}

		private void ModifyNode(Event e)
		{
			GenericMenu genericMenu = new GenericMenu();

			if (selectedNode is NarrativeObjectNode)
			{
				genericMenu.AddItem(new GUIContent("Delete"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.DeleteSelectedNode });
			}
			else if (selectedNode is DecisionPointNode)
			{
				genericMenu.AddItem(new GUIContent("Add Candidate"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.AddCandidate });
				genericMenu.AddItem(new GUIContent("Remove Candidate"), false, null, null);

				DecisionPointNode decisionPointNode = selectedNode as DecisionPointNode;

				for (int candidateCount = 0; candidateCount < decisionPointNode.candidateNodes.Count; candidateCount++)
				{
					NarrativeObjectNode candidateNode = decisionPointNode.candidateNodes[candidateCount];

					genericMenu.AddItem(new GUIContent($"Remove Candidate/{candidateNode.narrativeObject.name}"), false, ContextCallback, new ContextCallbackArgs { contextAction = ContextActions.RemoveCandidate, data = new object[] { decisionPointNode, candidateNode } });
				}
			}

			genericMenu.ShowAsContext();

			e.Use();
		}

		private Vector2 GetPanDelta()
		{
			Vector2 panDelta = lastMouseDragPosition - mousePosition;
			lastMouseDragPosition = mousePosition;

			return panDelta;
		}

		private void PanNode(Event e)
		{
			Vector2 panDelta = GetPanDelta();

			selectedNode.Pan(panDelta);
		}

		private void PanAllNodes(Event e)
		{
			Vector2 panDelta = GetPanDelta();

			for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
			{
				nodes[nodeIndex].Pan(panDelta);
			}
		}

		private BaseNode GetSelectedNode()
		{
			// Find if another node was selected.
			for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
			{
				if (nodes[nodeIndex].windowRect.Contains(mousePosition))
				{
					return nodes[nodeIndex];
				}
			}

			return null;
		}

		private class ContextCallbackArgs
		{
			public ContextActions contextAction = ContextActions.Undefined;
			public object[] data = null;
		}

		private void ContextCallback(object obj)
		{
			ContextCallbackArgs args = null;

			try
			{
				args = (ContextCallbackArgs)obj;
			}
			catch (InvalidCastException e)
			{
				Debug.LogError("ContextCallback can only be called with a ContextCallbackArgs object.");
				throw e;
			}

			// Get the context action.
			ContextActions contextAction = args.contextAction;

			switch (contextAction)
			{
				case ContextActions.AddAtomicNarrativeObject:

					AtomicNarrativeObject atomicNarrativeObject = CuttingRoomContextMenus.InstantiateAtomicNarrativeObject();

					AtomicNarrativeObjectNode atomicNarrativeObjectNode = AddAtomicNarrativeObjectNode(atomicNarrativeObject);

					break;

				case ContextActions.AddGroupNarrativeObject:

					GroupNarrativeObject groupNarrativeObject = CuttingRoomContextMenus.InstantiateGroupNarrativeObject();

					GroupNarrativeObjectNode groupNarrativeObjectNode = AddGroupNarrativeObjectNode(groupNarrativeObject);

					break;

				case ContextActions.AddLayerNarrativeObject:

					LayerNarrativeObject layerNarrativeObject = CuttingRoomContextMenus.InstantiateLayerNarrativeObject();

					LayerNarrativeObjectNode layerNarrativeObjectNode = AddLayerNarrativeObjectNode(layerNarrativeObject);

					break;

				case ContextActions.AddGraphNarrativeObject:

					GraphNarrativeObject graphNarrativeObject = CuttingRoomContextMenus.InstantiateGraphNarrativeObject();

					// TODO: Add node here for graph. Currently this is dep'd.
					
					break;

				case ContextActions.DeleteSelectedNode:

					DeleteSelectedNode();

					break;

				case ContextActions.AddCandidate:

					StartAddCandidate();

					break;

				case ContextActions.RemoveCandidate:

					DecisionPointNode decisionPointNode = args.data[0] as DecisionPointNode;
					NarrativeObjectNode candidateNode = args.data[1] as NarrativeObjectNode;

					RemoveCandidate(decisionPointNode, candidateNode);

					break;
			}
		}

		private AtomicNarrativeObjectNode AddAtomicNarrativeObjectNode(AtomicNarrativeObject atomicNarrativeObject)
		{
			AtomicNarrativeObjectNode atomicNarrativeObjectNode = CreateInstance(typeof(AtomicNarrativeObjectNode)) as AtomicNarrativeObjectNode;

			atomicNarrativeObjectNode.Init(this, atomicNarrativeObject);

			return atomicNarrativeObjectNode;
		}

		private GroupNarrativeObjectNode AddGroupNarrativeObjectNode(GroupNarrativeObject groupNarrativeObject)
		{
			GroupNarrativeObjectNode groupNarrativeObjectNode = CreateInstance(typeof(GroupNarrativeObjectNode)) as GroupNarrativeObjectNode;

			groupNarrativeObjectNode.Init(this, groupNarrativeObject);

			return groupNarrativeObjectNode;
		}

		private LayerNarrativeObjectNode AddLayerNarrativeObjectNode(LayerNarrativeObject layerNarrativeObject)
		{
			LayerNarrativeObjectNode layerNarrativeObjectNode = CreateInstance(typeof(LayerNarrativeObjectNode)) as LayerNarrativeObjectNode;

			layerNarrativeObjectNode.Init(this, layerNarrativeObject);

			return layerNarrativeObjectNode;
		}

		private OutputSelectionDecisionPointNode CreateOutputSelectionDecisionPointNode(OutputSelectionDecisionPoint outputSelectionDecisionPoint, Vector2 position, Vector2 parentNarrativeObjectNodeSize, Vector2 size)
		{
			// Create the output selection node.
			OutputSelectionDecisionPointNode outputSelectionDecisionPointNode = CreateInstance(typeof(OutputSelectionDecisionPointNode)) as OutputSelectionDecisionPointNode;

			outputSelectionDecisionPointNode.Init(this, outputSelectionDecisionPoint, position + new Vector2(parentNarrativeObjectNodeSize.x * 1.5f, 0.0f), size);

			return outputSelectionDecisionPointNode;
		}

		private void StartAddCandidate()
		{
			// Store out the decision point to add the candidate to.
			decisionPointNodeAddingCandidate = selectedNode as DecisionPointNode;
		}

		private void FinishAddCandidate()
		{
			if (decisionPointNodeAddingCandidate != null)
			{
				// If something new was selected.
				if (selectedNode != null)
				{
					// If its a narrative object node.
					if (selectedNode is NarrativeObjectNode)
					{
						// Get the narrative object node from the base node instance.
						NarrativeObjectNode narrativeObjectNode = selectedNode as NarrativeObjectNode;

						// Link it to the decision point.
						decisionPointNodeAddingCandidate.AddCandidate(narrativeObjectNode);
					}
				}

				// Clear the reference as attempt has been made to connect.
				decisionPointNodeAddingCandidate = null;
			}
		}

		private void RemoveCandidate(DecisionPointNode decisionPointNode, NarrativeObjectNode narrativeObjectNode)
		{
			decisionPointNode.RemoveCandidate(narrativeObjectNode);
		}

		public void PushToViewStack(GroupNarrativeObjectNode groupNarrativeObjectNode)
		{
			PushToViewStack(new ViewStackInfo() { renderMode = RenderMode.Group, baseNode = groupNarrativeObjectNode });
		}

		public void PushToViewStack(LayerNarrativeObjectNode layerNarrativeObjectNode)
		{
			PushToViewStack(new ViewStackInfo() { renderMode = RenderMode.Layer, baseNode = layerNarrativeObjectNode });
		}

		private void PushToViewStack(ViewStackInfo viewStackInfo)
		{
			viewStack.Push(viewStackInfo);

			GenerateNarrativeSpaceGraph();
		}

		private void InitialiseViewStack()
		{
			PushToViewStack(new ViewStackInfo() { renderMode = RenderMode.Overview, baseNode = null });
		}

		private void PopFromViewStack()
		{
			if (viewStack.Count > 1)
			{
				viewStack.Pop();
			}

			// TODO: Regenerate graph here.
			GenerateNarrativeSpaceGraph();
		}
	}
}
