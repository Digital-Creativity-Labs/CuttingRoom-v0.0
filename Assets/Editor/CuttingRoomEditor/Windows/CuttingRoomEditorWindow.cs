using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using UnityEditor.Experimental.GraphView;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace CuttingRoom.Editor
{
    public class CuttingRoomEditorWindow : EditorWindow
    {
        // Palette: https://coolors.co/264653-2a7221-ba1200-1f0322-f0bcd4

        /// <summary>
        /// Whether the dev toolbar is enabled or not.
        /// </summary>
        private bool DevToolbarEnabled { get; set; } = false;

        /// <summary>
        /// Cutting Room graph view which contains nodes.
        /// </summary>
        public EditorGraphView GraphView { get; private set; } = null;

        /// <summary>
        /// Cutting Room toolbar with controls for editing narrative spaces.
        /// </summary>
        public EditorToolbar Toolbar { get; private set; } = null;

        /// <summary>
        /// The navigation toolbar containing view stack breadcrumbs.
        /// </summary>
        private NavigationToolbar NavigationToolbar { get; set; } = null;

        /// <summary>
        /// Cutting Room dev toolbar.
        /// </summary>
        private EditorDevToolbar DevToolbar { get; set; } = null;

        /// <summary>
        /// Save utility instance.
        /// </summary>
        private EditorSaveUtility SaveUtility { get; set; } = null;

        /// <summary>
        /// Invoked when the editor window is cleared.
        /// </summary>
        public event Action OnWindowCleared;

        /// <summary>
        /// Invoked whenever a new narrative object is created through the toolbar.
        /// </summary>
        public event Action<NarrativeObject> OnNarrativeObjectCreated;

        /// <summary>
        /// Invoked whenever a narrative object node is selected.
        /// </summary>
        public event Action OnSelect;

        /// <summary>
        /// Invoked whenever a narrative object node is deselected.
        /// </summary>
        public event Action OnDeselect;

        /// <summary>
        /// Invoked whenever the graph view selection is cleared.
        /// </summary>
        public event Action OnSelectionCleared;

        /// <summary>
        /// Invoked whenever an object is deleted via the graph view.
        /// </summary>
        public event Action OnDelete;

        /// <summary>
        /// Reference to the open cutting room inspector window if it exists.
        /// </summary>
        public CuttingRoomInspectorWindow InspectorWindow { get; set; } = null;

        /// <summary>
        /// If constraints have been modified.
        /// </summary>
        public bool NarrativeObjectConstraintsModified { get; set; } = false;

        /// <summary>
        /// Menu option to open editor window.
        /// </summary>
        [MenuItem("Cutting Room/Editor")]
        public static void OpenEditor()
        {
            CreateCuttingRoomEditorWindow();
        }

        /// <summary>
        /// Create a new instance of the editor window.
        /// </summary>
        /// <returns></returns>
        public static CuttingRoomEditorWindow CreateCuttingRoomEditorWindow()
        {
            CuttingRoomEditorWindow window = GetWindow<CuttingRoomEditorWindow>();
            window.titleContent = new GUIContent(text: "Cutting Room");
            return window;
        }

        /// <summary>
        /// Ensure this window is connected to necessary windows in Cutting Room ecosystem.
        /// </summary>
        public void ConnectToOtherWindows(bool reconnect)
        {
            if (InspectorWindow == null || reconnect)
            {
                InspectorWindow = EditorWindowUtils.GetWindowIfOpen<CuttingRoomInspectorWindow>();

                if (InspectorWindow != null)
                {
                    InspectorWindow.ConnectEditorWindow(this);
                }
            }
        }

        /// <summary>
        /// Whenever the scene is saved, make sure that a corresponding save resource exists which matches the scene name.
        /// </summary>
        /// <param name="scene"></param>
        private void OnSceneSaved(Scene scene)
        {
            SaveUtility.Save();
        }

        /// <summary>
        /// Initialise the required components for this window.
        /// </summary>
        private void Initialise()
        {
            ConnectToOtherWindows(false);

            // Make sure that when the scene is saved, the contents are serialised once.
            EditorSceneManager.sceneSaved -= OnSceneSaved;
            EditorSceneManager.sceneSaved += OnSceneSaved;

            if (GraphView == null)
            {
                GraphView = new EditorGraphView(this);

                // Stretch graph view to size of window.
                GraphView.StretchToParentSize();

                // Register callback for graph view changing to save nodes.
                GraphView.OnGraphViewChanged += (GraphViewChange graphViewChange) =>
                {
                    // If elements have moved around then save the positions.
                    if (graphViewChange.movedElements != null && graphViewChange.movedElements.Count > 0)
                    {
                        SaveUtility.Save();
                    }

                    if (graphViewChange.elementsToRemove != null && graphViewChange.elementsToRemove.Count > 0)
                    {
                        OnDelete?.Invoke();

                        RegenerateContents(true);
                    }
                };

                GraphView.OnViewContainerPushed += OnViewContainerPushed;

                GraphView.OnRootNarrativeObjectChanged += OnRootNarrativeObjectChanged;
                GraphView.OnNarrativeObjectCandidatesChanged += OnNarrativeObjectCandidatesChanged;

                GraphView.OnNarrativeObjectNodeSelected += OnGraphViewNarrativeObjectNodeSelected;
                GraphView.OnNarrativeObjectNodeDeselected += OnGraphViewNarrativeObjectNodeDeselected;
                GraphView.OnEdgeSelected += OnGraphViewEdgeSelected;
                GraphView.OnEdgeDeselected += OnGraphViewEdgeDeselected;
                GraphView.OnClearSelection += OnGraphViewClearSelection;
            }

            if (Toolbar == null)
            {
                Toolbar = new EditorToolbar();

                Toolbar.OnClickToggleDevToolbar += () =>
                {
                    DevToolbarEnabled = !DevToolbarEnabled;

                    RegenerateContents(false);
                };

                Toolbar.OnClickAddAtomicNarrativeObjectNode += () =>
                {
                    AtomicNarrativeObject atomicNarrativeObject = CuttingRoomContextMenus.CreateAtomicNarrativeObject();

                    atomicNarrativeObject.transform.parent = GraphView.VisibleViewContainerNarrativeObject?.transform;

                    OnNarrativeObjectCreated?.Invoke(atomicNarrativeObject);

                    RegenerateContents(false);
                };

                Toolbar.OnClickAddGraphNarrativeObjectNode += () =>
                {
                    GraphNarrativeObject graphNarrativeObject = CuttingRoomContextMenus.CreateGraphNarrativeObject();

                    graphNarrativeObject.transform.parent = GraphView.VisibleViewContainerNarrativeObject?.transform;

                    OnNarrativeObjectCreated?.Invoke(graphNarrativeObject);

                    RegenerateContents(false);
                };

                Toolbar.OnClickAddGroupNarrativeObjectNode += () =>
                {
                    GroupNarrativeObject groupNarrativeObject = CuttingRoomContextMenus.CreateGroupNarrativeObject();

                    groupNarrativeObject.transform.parent = GraphView.VisibleViewContainerNarrativeObject?.transform;

                    OnNarrativeObjectCreated?.Invoke(groupNarrativeObject);

                    RegenerateContents(true);
                };
            }

            if (NavigationToolbar == null)
            {
                NavigationToolbar = new NavigationToolbar();

                NavigationToolbar.OnClickNavigationButton += OnClickNavigationButton;

                NavigationToolbar.OnClickViewBackButton += () =>
                {
                    // Only do a regen if a view is actually popped.
                    if (GraphView.PopViewContainer())
                    {
                        // Save here to ensure that the altered view stack is preserved.
                        // When regenerating the contents of the window, the viewstack is loaded
                        // and recreated, then saved again. If it's not saved before Regenerating then
                        // the viewstack in the save file still has the view which has just been popped
                        // at the top and you can never escape it!
                        SaveUtility.Save();

                        // New view container so clear the window as the old nodes are not visible anymore.
                        RegenerateContents(true);
                    }
                };
            }

            if (DevToolbar == null)
            {
                DevToolbar = new EditorDevToolbar();
            }
        }

        /// <summary>
        /// Invoked whenever an edge is deselected on the graph view.
        /// </summary>
        /// <param name="selected"></param>
        private void OnGraphViewEdgeDeselected()
        {
            OnDeselect?.Invoke();
        }

        /// <summary>
        /// Invoked whenever an edge is selected on the graph view.
        /// </summary>
        /// <param name="outputNarrativeObjectNode"></param>
        /// <param name="inputNarrativeObjectNode"></param>
        private void OnGraphViewEdgeSelected()
        {
            OnSelect?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a narrative object node is deselected on the graph view.
        /// </summary>
        private void OnGraphViewNarrativeObjectNodeDeselected()
        {
            OnDeselect?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a narrative object node is selected on the graph view.
        /// </summary>
        /// <param name="narrativeObjectNode"></param>
        private void OnGraphViewNarrativeObjectNodeSelected()
        {
            OnSelect?.Invoke();
        }

        /// <summary>
        /// Invoked whenever the selection is cleared on the graph view.
        /// </summary>
        /// <param name="selected"></param>
        private void OnGraphViewClearSelection()
        {
            OnSelectionCleared?.Invoke();
        }

        /// <summary>
        /// Invoked whenever a navigation button is clicked on the Navigation Toolbar.
        /// </summary>
        /// <param name="viewContainer"></param>
        private void OnClickNavigationButton(ViewContainer viewContainer)
        {
            if (GraphView.PopViewContainersToViewContainer(viewContainer))
            {
                // Save the view has been popped before regenerating (which loads existing data,
                // which without this save will still have the popped containers in it).
                SaveUtility.Save();

                RegenerateContents(true);
            }
        }

        /// <summary>
        /// Invoked whenever the outputs of a narrative object change.
        /// </summary>
        private void OnNarrativeObjectOutputCandidatesChanged()
        {
            RegenerateContents(true);
        }

        /// <summary>
        /// Invoked whenever the candidates for a group narrative objects group selection decision point change.
        /// </summary>
        private void OnGroupNarrativeObjectGroupSelectionCandidatesChanged()
        {
            RegenerateContents(true);
        }

        /// <summary>
        /// Invoked whenever the root narrative object of the narrative space or any narrative object changes.
        /// </summary>
        private void OnRootNarrativeObjectChanged()
        {
            RegenerateContents(true);
        }

        /// <summary>
        /// Invoked whenever the candidates of the current view container change.
        /// </summary>
        private void OnNarrativeObjectCandidatesChanged()
        {
            RegenerateContents(true);
        }

        /// <summary>
        /// Populate the graph view for this window.
        /// </summary>
        private void PopulateGraphView()
        {
            // Find Narrative Objects in the scene. These will be displayed on the Graph View as nodes.
            NarrativeObject[] narrativeObjects = FindObjectsOfType<NarrativeObject>();

            foreach (NarrativeObject narrativeObject in narrativeObjects)
            {
                narrativeObject.outputSelectionDecisionPoint.OnCandidatesChanged -= OnNarrativeObjectOutputCandidatesChanged;
                narrativeObject.outputSelectionDecisionPoint.OnCandidatesChanged += OnNarrativeObjectOutputCandidatesChanged;

                if (narrativeObject is GroupNarrativeObject)
                {
                    GroupNarrativeObject groupNarrativeObject = narrativeObject.GetComponent<GroupNarrativeObject>();

                    groupNarrativeObject.groupSelectionDecisionPoint.OnCandidatesChanged -= OnGroupNarrativeObjectGroupSelectionCandidatesChanged;
                    groupNarrativeObject.groupSelectionDecisionPoint.OnCandidatesChanged += OnGroupNarrativeObjectGroupSelectionCandidatesChanged;
                }
            }

            EditorGraphView.PopulateResult populateResult = GraphView.Populate(SaveUtility.Load(), narrativeObjects);

            if (populateResult.GraphViewChanged)
            {
                SaveUtility.Save(populateResult.CreatedNodes);
            }
        }

        /// <summary>
        /// Unity event invoked whenever this window is opened.
        /// </summary>
        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;

            RegenerateContents(false);
        }

        /// <summary>
        /// Unity event invoked whenever this window is closed.
        /// </summary>
        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// Invoked whenever the editor play mode state changes (e.g. entering or exiting play mode).
        /// </summary>
        /// <param name="playModeStateChange"></param>
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode || playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                ConnectToOtherWindows(true);

                // Whenever the play mode state changes, the editor ui is
                // invalidated so it must be regenerated.
                RegenerateContents(true);
            }
        }

        /// <summary>
        /// Invoked whenever the scene hierarchy is changed.
        /// </summary>
        private void OnHierarchyChanged()
        {
            // Whenever the hierarchy changes, regenerate as narrative
            // objects might have been destroyed.
            RegenerateContents(true);
        }

        /// <summary>
        /// Invoked when the graph views view container changes.
        /// This is either entering or exiting a new view container.
        /// </summary>
        private void OnViewContainerPushed()
        {
            // Clear the window as the previous nodes are not visible anymore.
            RegenerateContents(true);
        }

        /// <summary>
        /// Wipe the contents of this window.
        /// </summary>
        private void ClearWindow()
        {
            // Clear the root visual element for components to be re-added.
            rootVisualElement.Clear();

            // Invoke window cleared event.
            OnWindowCleared?.Invoke();
        }

        /// <summary>
        /// Regenerate the editor windows contents.
        /// </summary>
        /// <param name="clearWindow"></param>
        private void RegenerateContents(bool clearWindow)
        {
            Initialise();

            // When coming from playmode change events, the window must be totally regenerated
            // (as all editor variables are discarded so old contents is now invalid).
            if (clearWindow)
            {
                // Clear the window.
                ClearWindow();
            }

            AddGraphView();
            AddToolbar();
            AddNavigationToolbar();

            NavigationToolbar.GenerateContents(GraphView.ViewContainerStack);

            if (DevToolbarEnabled)
            {
                AddDevToolbar();
            }
            else
            {
                RemoveDevToolbar();
            }

            // Create a save utility instance with the current graph as it's target.
            SaveUtility = new EditorSaveUtility(GraphView);

            PopulateGraphView();
        }

        /// <summary>
        /// Add graph view component to this window.
        /// </summary>
        private void AddGraphView()
        {
            // Add the graph view to the root element of this window.
            rootVisualElement.Add(GraphView);
        }

        /// <summary>
        /// Add the toolbar to this window.
        /// </summary>
        private void AddToolbar()
        {
            rootVisualElement.Add(Toolbar);
        }

        /// <summary>
        /// Add the navigation toolbar to this window.
        /// </summary>
        private void AddNavigationToolbar()
        {
            rootVisualElement.Add(NavigationToolbar);
        }

        /// <summary>
        /// Add the dev toolbar to this window.
        /// </summary>
        private void AddDevToolbar()
        {
            rootVisualElement.Add(DevToolbar);
        }

        /// <summary>
        /// Remove an existing dev toolbar from this window.
        /// </summary>
        private void RemoveDevToolbar()
        {
            if (rootVisualElement.Contains(DevToolbar))
            {
                rootVisualElement.Remove(DevToolbar);
            }
        }
    }
}
