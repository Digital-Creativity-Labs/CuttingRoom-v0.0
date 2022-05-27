using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System;
using UnityEditor.Experimental.GraphView;

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
        private CuttingRoomEditorGraphView GraphView { get; set; } = null;

        /// <summary>
        /// Cutting Room toolbar with controls for editing narrative spaces.
        /// </summary>
        private CuttingRoomEditorToolbar Toolbar { get; set; } = null;

        /// <summary>
        /// The navigation toolbar containing view stack breadcrumbs.
        /// </summary>
        private NavigationToolbar NavigationToolbar { get; set; } = null;

        /// <summary>
        /// Cutting Room dev toolbar.
        /// </summary>
        private CuttingRoomEditorDevToolbar DevToolbar { get; set; } = null;

        /// <summary>
        /// Save utility instance.
        /// </summary>
        private CuttingRoomEditorSaveUtility SaveUtility { get; set; } = null;

        /// <summary>
        /// Invoked when the editor window is cleared.
        /// </summary>
        public event Action OnWindowCleared;

        /// <summary>
        /// Menu option to open editor window.
        /// </summary>
        [MenuItem("Cutting Room/Open Editor")]
        public static void OpenEditor()
        {
            CuttingRoomEditorWindow window = GetWindow<CuttingRoomEditorWindow>();
            window.titleContent = new GUIContent(text: "Cutting Room");
        }

        /// <summary>
        /// Initialise the required components for this window.
        /// </summary>
        private void Initialise()
        {
            if (GraphView == null)
            {
                GraphView = new CuttingRoomEditorGraphView(this);

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
                        RegenerateContents(true);
                    }
                };

                GraphView.OnViewContainerPushed += OnViewContainerPushed;

                GraphView.OnRootNarrativeObjectChanged += OnRootNarrativeObjectChanged;
            }

            if (Toolbar == null)
            {
                Toolbar = new CuttingRoomEditorToolbar();

                Toolbar.OnClickToggleDevToolbar += () =>
                {
                    DevToolbarEnabled = !DevToolbarEnabled;

                    RegenerateContents(false);
                };

                Toolbar.OnClickPopViewContainer += () =>
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

                Toolbar.OnClickAddAtomicNarrativeObjectNode += () =>
                {
                    CuttingRoomContextMenus.CreateAtomicNarrativeObject();

                    RegenerateContents(false);
                };

                Toolbar.OnClickAddGraphNarrativeObjectNode += () =>
                {
                    CuttingRoomContextMenus.CreateGraphNarrativeObject();

                    RegenerateContents(false);
                };
            }

            if (NavigationToolbar == null)
            {
                NavigationToolbar = new NavigationToolbar();

                NavigationToolbar.OnClickNavigationButton += OnClickNavigationButton;
            }

            if (DevToolbar == null)
            {
                DevToolbar = new CuttingRoomEditorDevToolbar();
            }
        }

        /// <summary>
        /// Invoked whenever a navigation button is clicked on the Navigation Toolbar.
        /// </summary>
        /// <param name="viewContainer"></param>
        private void OnClickNavigationButton(ViewContainer viewContainer)
        {
            if (GraphView.PopViewContainersToViewContainer(viewContainer))
            {
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
        /// Invoked whenever the root narrative object of the narrative space or any narrative object changes.
        /// </summary>
        private void OnRootNarrativeObjectChanged()
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
            }

            bool graphViewChanged = GraphView.Populate(SaveUtility.Load(), narrativeObjects);

            if (graphViewChanged)
            {
                SaveUtility.Save();
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
            // Whenever the play mode state changes, the editor ui is
            // invalidated so it must be regenerated.
            RegenerateContents(true);
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
            SaveUtility = new CuttingRoomEditorSaveUtility(GraphView);

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
            rootVisualElement.Add(Toolbar.Toolbar);
        }

        /// <summary>
        /// Add the navigation toolbar to this window.
        /// </summary>
        private void AddNavigationToolbar()
        {
            rootVisualElement.Add(NavigationToolbar.Toolbar);
        }

        /// <summary>
        /// Add the dev toolbar to this window.
        /// </summary>
        private void AddDevToolbar()
        {
            rootVisualElement.Add(DevToolbar.Toolbar);
        }

        /// <summary>
        /// Remove an existing dev toolbar from this window.
        /// </summary>
        private void RemoveDevToolbar()
        {
            if (rootVisualElement.Contains(DevToolbar.Toolbar))
            {
                rootVisualElement.Remove(DevToolbar.Toolbar);
            }
        }
    }
}
