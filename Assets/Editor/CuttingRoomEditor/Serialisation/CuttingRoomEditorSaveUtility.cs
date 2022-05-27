using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace CuttingRoom.Editor
{
    public class CuttingRoomEditorSaveUtility
    {
        /// <summary>
        /// The graph view to be saved.
        /// </summary>
        private CuttingRoomEditorGraphView CuttingRoomEditorGraphView = null;

        private List<NarrativeObjectNode> NarrativeObjectNodes => CuttingRoomEditorGraphView.NarrativeObjectNodes;

        private List<ViewContainer> ViewContainers => CuttingRoomEditorGraphView.viewContainers;

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
        private string SavePath { get { return $"Assets/Resources/CuttingRoomEditor/{EditorSceneManager.GetActiveScene().name}.asset"; } }

        public CuttingRoomEditorSaveUtility(CuttingRoomEditorGraphView cuttingRoomEditorGraphView)
        {
            CuttingRoomEditorGraphView = cuttingRoomEditorGraphView;
        }

        public void Save()
        {
             CuttingRoomEditorGraphViewState graphViewState = ScriptableObject.CreateInstance<CuttingRoomEditorGraphViewState>();

            // Load the existing save state as a reference.
            CuttingRoomEditorGraphViewState loadedGraphViewState = Load();

            foreach (ViewContainer viewContainer in ViewContainers)
            {
                ViewContainerState viewContainerState = new ViewContainerState { narrativeObjectGuid = viewContainer.narrativeObjectGuid, narrativeObjectNodeGuids = viewContainer.narrativeObjectNodeGuids };
                graphViewState.viewContainerStates.Add(viewContainerState);

                NarrativeObject[] narrativeObjects = Object.FindObjectsOfType<NarrativeObject>();

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
                                        graphViewState.narrativeObjectNodeStates.Add(existingNodeState);
                                    }
                                    // or if the node is truly new, then just record its position at 0,0 and add it to the save data.
                                    else
                                    {
                                        graphViewState.narrativeObjectNodeStates.Add(new NarrativeObjectNodeState { narrativeObjectGuid = narrativeObjectGuid, position = narrativeObjectNode.GetPosition().position });
                                    }
                                }
                                else
                                {
                                    graphViewState.narrativeObjectNodeStates.Add(new NarrativeObjectNodeState { narrativeObjectGuid = narrativeObjectGuid, position = narrativeObjectNode.GetPosition().position });
                                }
                            }
                            else
                            {
                                // The nodes rect is valid, so save it with whatever its values are.
                                graphViewState.narrativeObjectNodeStates.Add(new NarrativeObjectNodeState { narrativeObjectGuid = narrativeObjectGuid, position = narrativeObjectNode.GetPosition().position });
                            }
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

                // Exit here.
                return;
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