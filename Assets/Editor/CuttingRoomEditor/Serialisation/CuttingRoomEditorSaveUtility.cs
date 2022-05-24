using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.EditorCoroutines.Editor;

namespace CuttingRoom.Editor
{
    public class CuttingRoomEditorSaveUtility
    {
        /// <summary>
        /// The graph view to be saved.
        /// </summary>
        private CuttingRoomEditorGraphView CuttingRoomEditorGraphView = null;

        private List<NarrativeObjectNode> NarrativeObjectNodes => CuttingRoomEditorGraphView.NarrativeObjectNodes;

        private List<BaseViewContainer> ViewContainers => CuttingRoomEditorGraphView.viewContainers;

        private List<string> ViewContainerStackGuids
        {
            get
            {
                List<string> viewContainerStackGuids = new List<string>();

                foreach (BaseViewContainer viewContainer in CuttingRoomEditorGraphView.viewContainerStack.ToList())
                {
                    viewContainerStackGuids.Add(viewContainer.narrativeObjectGuid);
                }

                return viewContainerStackGuids;
            }
        }

        /// <summary>
        /// Returns the save path for the currently open scene.
        /// </summary>
        private string SavePath { get { return $"Assets/Resources/CuttingRoomEditor/{EditorSceneManager.GetActiveScene().name}.asset"; } }

        public CuttingRoomEditorSaveUtility(CuttingRoomEditorGraphView cuttingRoomEditorGraphView)
        {
            CuttingRoomEditorGraphView = cuttingRoomEditorGraphView;
        }

        public void Save()
        {
            EditorCoroutineUtility.StartCoroutine(SaveCoroutine(), this);
        }

        public IEnumerator SaveCoroutine()
        {
            // Wait a frame.
            // When creating nodes on the graph, setting the position and then saving
            // on the same frame results in the position retaining the value as before
            // setting the position. Internally, despite set position call, the node is
            // only being updated on the next repaint of the editor and not immediately.
            yield return null;

            CuttingRoomEditorGraphViewState graphViewState = ScriptableObject.CreateInstance<CuttingRoomEditorGraphViewState>();

            // Load the existing save state as a reference.
            CuttingRoomEditorGraphViewState loadedGraphViewState = Load();

            foreach (BaseViewContainer baseViewContainer in ViewContainers)
            {
                ViewContainerState viewContainerState = new ViewContainerState { narrativeObjectGuid = baseViewContainer.narrativeObjectGuid, narrativeObjectNodeGuids = baseViewContainer.narrativeObjectNodeGuids };
                graphViewState.viewContainerStates.Add(viewContainerState);

                NarrativeObject[] narrativeObjects = Object.FindObjectsOfType<NarrativeObject>();

                foreach (string narrativeObjectGuid in baseViewContainer.narrativeObjectNodeGuids)
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
                            graphViewState.narrativeObjectNodeStates.Add(new NarrativeObjectNodeState { narrativeObjectGuid = narrativeObjectGuid, position = narrativeObjectNode.GetPosition().position });
                        }
                        else
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
            string sceneName = EditorSceneManager.GetActiveScene().name;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Cutting Room Graph View could not be saved as scene has not been saved. Please save your scene and try again.");

                // Exit coroutine here.
                yield break;
            }

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
            string sceneName = EditorSceneManager.GetActiveScene().name;

            if (string.IsNullOrEmpty(sceneName))
            {
                Debug.LogWarning("Cutting Room Graph View could not be saved as scene has not been saved. Please save your scene and try again.");

                return null;
            }

            // Generate the path that this scenes graph should be saved at.
            string savePath = $"Assets/Resources/CuttingRoomEditor/{sceneName}.asset";

            // Try to load any existing saved asset for this scene.
            Object existingGraphViewState = AssetDatabase.LoadAssetAtPath(savePath, typeof(CuttingRoomEditorGraphViewState));

            if (existingGraphViewState == null)
            {
                return null;
            }

            return existingGraphViewState as CuttingRoomEditorGraphViewState;
        }
    }
}