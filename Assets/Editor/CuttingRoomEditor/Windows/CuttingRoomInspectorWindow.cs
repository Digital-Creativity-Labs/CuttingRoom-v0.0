using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class CuttingRoomInspectorWindow : EditorWindow
    {
        /// <summary>
        /// Reference to the open cutting room editor window if it exists.
        /// </summary>
        public CuttingRoomEditorWindow EditorWindow { get; set; } = null;

        /// <summary>
        /// Inspector visual element.
        /// </summary>
        public EditorInspector Inspector = null;

        /// <summary>
        /// Invoked whenever a constraint is added.
        /// </summary>
        public Action OnAddConstraint;

        /// <summary>
        /// Invoked whenever a constraint is removed.
        /// </summary>
        public Action OnRemoveConstraint;

        /// <summary>
        /// Menu option to open editor window.
        /// </summary>
        [MenuItem("Cutting Room/Inspector")]
        public static void OpenInspector()
        {
            CreateCuttingRoomInspectorWindow();
        }

        /// <summary>
        /// Create a new instance of the editor window.
        /// </summary>
        /// <returns></returns>
        public static CuttingRoomInspectorWindow CreateCuttingRoomInspectorWindow()
        {
            CuttingRoomInspectorWindow window = GetWindow<CuttingRoomInspectorWindow>();
            window.titleContent = new GUIContent(text: "Cutting Room Inspector");

            return window;
        }

        /// <summary>
        /// Invoked whenever the editor play mode state changes (e.g. entering or exiting play mode).
        /// </summary>
        /// <param name="playModeStateChange"></param>
        private void OnPlayModeStateChanged(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.EnteredEditMode || playModeStateChange == PlayModeStateChange.EnteredPlayMode)
            {
                // Whenever the play mode state changes, the editor ui is
                // invalidated so it must be regenerated.
                RegenerateContents();
            }
        }

        /// <summary>
        /// Ensure this window is connected to necessary windows in Cutting Room ecosystem.
        /// </summary>
        public void ConnectEditorWindow(CuttingRoomEditorWindow editorWindow)
        {
            EditorWindow = editorWindow;

            if (EditorWindow.InspectorWindow == null)
            {
                EditorWindow.InspectorWindow = this;
            }

            EditorWindow.OnDelete -= RegenerateContents;
            EditorWindow.OnDelete += RegenerateContents;

            EditorWindow.OnSelect -= RegenerateContents;
            EditorWindow.OnSelect += RegenerateContents;

            EditorWindow.OnDeselect -= RegenerateContents;
            EditorWindow.OnDeselect += RegenerateContents;

            EditorWindow.OnSelectionCleared -= RegenerateContents;
            EditorWindow.OnSelectionCleared += RegenerateContents;
        }

        public void Initialise()
        {
            CuttingRoomEditorWindow cuttingRoomEditorWindow = EditorWindowUtils.GetWindowIfOpen<CuttingRoomEditorWindow>();

            if (cuttingRoomEditorWindow != null && cuttingRoomEditorWindow.InspectorWindow != this)
            {
                cuttingRoomEditorWindow.ConnectToOtherWindows(true);
            }

            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (Inspector == null)
            {
                Inspector = new EditorInspector();

                Inspector.OnNarrativeObjectAddedConstraint += OnNarrativeObjectAddedConstraint;
                Inspector.OnNarrativeObjectRemovedConstraint += OnNarrativeObjectRemovedConstraint;

                Inspector.OnNarrativeObjectAddedVariable += OnNarrativeObjectAddedVariable;
                Inspector.OnNarrativeObjectRemovedVariable += OnNarrativeObjectRemovedVariable;
            }
        }

        /// <summary>
        /// Unity event invoked whenever this window is opened.
        /// </summary>
        private void OnEnable()
        {
            RegenerateContents();
        }

        /// <summary>
        /// Unity event invoked whenever this window is closed.
        /// </summary>
        private void OnDisable()
        {

        }

        private void RegenerateContents()
        {
            Initialise();

            if (EditorWindow == null)
            {
                // If the editor window doesnt exist, then show nothing.
                return;
            }

            AddInspector();

            AddObjectControls();
        }

        private void AddInspector()
        {
            rootVisualElement.Add(Inspector);
        }

        private void AddObjectControls()
        {
            List<ISelectable> selected = new List<ISelectable>();

            // When opening Unity, the window comes into existence without the editor window having initialised.
            // In this case, the "selected" list is going to be empty, rendering global settings (no editor selection).
            if (EditorWindow != null && EditorWindow.GraphView != null)
            {
                selected = EditorWindow.GraphView.selected;
            }

            // Gets or creates narrative space.
            NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

            if (selected.Count == 1)
            {
                if (selected[0] is NarrativeObjectNode)
                {
                    NarrativeObjectNode narrativeObjectNode = selected[0] as NarrativeObjectNode;

                    // When deleting a node, the narrative object will be null but the selection persists, so check this.
                    if (narrativeObjectNode.NarrativeObject == null)
                    {
                        // If the gameobject doesnt exist, then show global settings.
                        Inspector.UpdateContentForGlobal(narrativeSpace);
                    }
                    else
                    {
                        Inspector.UpdateContentForNarrativeObjectNode(narrativeObjectNode);
                    }
                }
                else if (selected[0] is Edge)
                {
                    Edge edge = selected[0] as Edge;

                    NarrativeObjectNode outputNarrativeObjectNode = EditorWindow.GraphView.GetNarrativeObjectNodeWithPort(edge.output);
                    NarrativeObjectNode inputNarrativeObjectNode = EditorWindow.GraphView.GetNarrativeObjectNodeWithPort(edge.input);

                    Inspector.UpdateContentForEdge(outputNarrativeObjectNode, inputNarrativeObjectNode);
                }
            }
            else
            {
                // No selection so show variables for global things.
                Inspector.UpdateContentForGlobal(narrativeSpace);
            }
        }

        private void OnNarrativeObjectAddedConstraint()
        {
            OnAddConstraint?.Invoke();

            RegenerateContents();
        }

        private void OnNarrativeObjectRemovedConstraint()
        {
            OnRemoveConstraint?.Invoke();

            RegenerateContents();
        }
        private void OnNarrativeObjectAddedVariable()
        {
            RegenerateContents();
        }

        private void OnNarrativeObjectRemovedVariable()
        {
            RegenerateContents();
        }
    }
}
