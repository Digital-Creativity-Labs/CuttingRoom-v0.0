using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class EditorSaveUtility
    {
        /// <summary>
        /// The name of the unsaved scene in the Unity Editor.
        /// </summary>
        private const string untitledSceneName = "Untitled";

        /// <summary>
        /// Get the name of the active scene in the editor.
        /// </summary>
        public string ActiveSceneName
        {
            get
            {
                string activeSceneName = EditorSceneManager.GetActiveScene().name;

                if (string.IsNullOrEmpty(activeSceneName))
                {
                    activeSceneName = untitledSceneName;
                }

                return activeSceneName;
            }
        }

        /// <summary>
        /// The graph view to be saved.
        /// </summary>
        private EditorGraphView CuttingRoomEditorGraphView = null;

        /// <summary>
        /// The narrative object nodes which currently exist on the graph view.
        /// </summary>
        private List<NarrativeObjectNode> NarrativeObjectNodes => CuttingRoomEditorGraphView.NarrativeObjectNodes;

        /// <summary>
        /// The view containers which currently exist within the graph view.
        /// </summary>
        private List<ViewContainer> ViewContainers => CuttingRoomEditorGraphView.viewContainers;

        /// <summary>
        /// Get the guids of the view containers on the view stack.
        /// </summary>
        private List<string> ViewContainerStackGuids
        {
            get
            {
                List<string> viewContainerStackGuids = new List<string>();

                foreach (ViewContainer viewContainer in CuttingRoomEditorGraphView.ViewContainerStack.ToList())
                {
                    viewContainerStackGuids.Add(viewContainer.narrativeObjectGuid);
                }

                return viewContainerStackGuids;
            }
        }

        /// <summary>
        /// Returns the save path for the currently open scene.
        /// </summary>
        private string SavePath { get { return $"Assets/Resources/CuttingRoomEditor/{ActiveSceneName}.asset"; } }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cuttingRoomEditorGraphView"></param>
        public EditorSaveUtility(EditorGraphView cuttingRoomEditorGraphView)
        {
            CuttingRoomEditorGraphView = cuttingRoomEditorGraphView;
        }

        /// <summary>
        /// Save the current graph view layout and data.
        /// </summary>
        public void Save(List<Tuple<string, Vector2>> createdNodes = null)
        {
             CuttingRoomEditorGraphViewState graphViewState = ScriptableObject.CreateInstance<CuttingRoomEditorGraphViewState>();

            // Load the existing save state as a reference.
            CuttingRoomEditorGraphViewState loadedGraphViewState = Load();

            // If there was no existing state for the scene being saved, then perhaps this is it's first save,
            // so load the untitled scenes data (which will contain the current layout of the graph).
            if (loadedGraphViewState == null)
            {
                loadedGraphViewState = Load(untitledSceneName);
            }

            foreach (ViewContainer viewContainer in ViewContainers)
            {
                ViewContainerState viewContainerState = new ViewContainerState { narrativeObjectGuid = viewContainer.narrativeObjectGuid, narrativeObjectNodeGuids = viewContainer.narrativeObjectNodeGuids };
                graphViewState.viewContainerStates.Add(viewContainerState);

                NarrativeObject[] narrativeObjects = UnityEngine.Object.FindObjectsOfType<NarrativeObject>();

                foreach (string narrativeObjectGuid in viewContainer.narrativeObjectNodeGuids)
                {
                    // Find if the object still exists in the scene.
                    bool narrativeObjectExists = narrativeObjects.Where(narrativeObject => narrativeObject.guid == narrativeObjectGuid).FirstOrDefault() != null;

                    // The object exists.
                    if (narrativeObjectExists)
                    {
                        // Check if it has a live node on the screen (so must be part of the visible view container)
                        NarrativeObjectNode narrativeObjectNode = NarrativeObjectNodes.Where(narrativeObjectNode => narrativeObjectNode.NarrativeObject.guid == narrativeObjectGuid).FirstOrDefault();

                        // If there is a node currently visible, save that.
                        if (narrativeObjectNode != null)
                        {
                            Rect rect = narrativeObjectNode.GetPosition();

                            bool nodeStateAdded = false;

                            // This is a hack to work around the position rect not being valid on the frame it is created.
                            // If node is created and then the rect examined on the same frame, it hasn't come into existence yet
                            // (has position 0,0 despite node being positioned elsewhere, height and width are NaN etc.).
                            if (rect.height.ToString() == "NaN" || rect.width.ToString() == "NaN")
                            {
                                if (loadedGraphViewState != null)
                                {
                                    // Check the loaded save file for existing saved state for the narrative object.
                                    NarrativeObjectNodeState existingNodeState = loadedGraphViewState.narrativeObjectNodeStates.Where(narrativeObjectNodeState => narrativeObjectNodeState.narrativeObjectGuid == narrativeObjectGuid).FirstOrDefault();

                                    // If there is an existing state, then it persists onto the new save rather than saving the invalid rects position.
                                    if (existingNodeState != null)
                                    {
                                        nodeStateAdded = true;

                                        graphViewState.narrativeObjectNodeStates.Add(existingNodeState);
                                    }
                                }
                            }

                            if (!nodeStateAdded)
                            {
                                Vector2 nodePosition = narrativeObjectNode.GetPosition().position;

                                // Check newly created nodes and find what position they have been instantiated at and save that.
                                // This is necessary as if you get position of a new node, it will be 0,0 as rect of the node is uninitialised until next window repaint.
                                if (createdNodes != null)
                                {
                                    Tuple<string, Vector2> createdNodeData = createdNodes.Where(createdNode => createdNode.Item1 == narrativeObjectNode.NarrativeObject.guid).FirstOrDefault();

                                    if (createdNodeData != null)
                                    {
                                        nodePosition = createdNodeData.Item2;
                                    }
                                }

                                // The nodes rect is valid, so save it with whatever its values are.
                                graphViewState.narrativeObjectNodeStates.Add(new NarrativeObjectNodeState { narrativeObjectGuid = narrativeObjectGuid, position = nodePosition });
                            }
                        }
                        else
                        {
                            if (loadedGraphViewState != null)
                            {
                                // Check the loaded save file for existing saved state for the narrative object.
                                NarrativeObjectNodeState existingNodeState = loadedGraphViewState.narrativeObjectNodeStates.Where(narrativeObjectNodeState => narrativeObjectNodeState.narrativeObjectGuid == narrativeObjectGuid).FirstOrDefault();

                                // If there is an existing state, then it persists onto the new save.
                                if (existingNodeState != null)
                                {
                                    graphViewState.narrativeObjectNodeStates.Add(existingNodeState);
                                }
                            }
                        }
                    }
                }
            }

            // Save the view stack.
            graphViewState.viewContainerStackGuids = ViewContainerStackGuids;

            // Create resources folder if it doesn't exist.
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            // Create CuttingRoomEditor folder inside Resources if it doesn't exist.
            if (!AssetDatabase.IsValidFolder("Assets/Resources/CuttingRoomEditor"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "CuttingRoomEditor");
            }

            // Get the name of the current scene in the editor.
            string sceneName = ActiveSceneName;

            // Try to load any existing saved asset for this scene.
            CuttingRoomEditorGraphViewState existingGraphViewState = Load();

            // If a saved asset doesn't exist, create a new one, otherwise overwrite the values in the old saved asset.
            if (existingGraphViewState == null)
            {
                AssetDatabase.CreateAsset(graphViewState, SavePath);
            }
            else
            {
                existingGraphViewState.narrativeObjectNodeStates = graphViewState.narrativeObjectNodeStates;
                existingGraphViewState.viewContainerStates = graphViewState.viewContainerStates;
                existingGraphViewState.viewContainerStackGuids = graphViewState.viewContainerStackGuids;

                EditorUtility.SetDirty(existingGraphViewState);
            }

            AssetDatabase.SaveAssets();
        }

        /// <summary>
        /// Attempt to load an existing graph view state for the currently active scene in the editor.
        /// </summary>
        /// <returns></returns>
        public CuttingRoomEditorGraphViewState Load()
        {
            // Get the name of the current scene in the editor.
            string sceneName = ActiveSceneName;

            return Load(sceneName);
        }

        /// <summary>
        /// Attempt to load the graph view state for the specified scene name.
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        private CuttingRoomEditorGraphViewState Load(string sceneName)
        {
            // Generate the path that this scenes graph should be saved at.
            string savePath = $"Assets/Resources/CuttingRoomEditor/{sceneName}.asset";

            // Try to load any existing saved asset for this scene.
            UnityEngine.Object existingGraphViewState = AssetDatabase.LoadAssetAtPath(savePath, typeof(CuttingRoomEditorGraphViewState));

            if (existingGraphViewState == null)
            {
                return null;
            }

            return existingGraphViewState as CuttingRoomEditorGraphViewState;
        }
    }
}