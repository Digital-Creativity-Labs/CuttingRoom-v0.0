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

        public void Initialise()
        {
            if (EditorWindow == null)
            {
                EditorWindow = EditorWindowUtils.GetWindowIfOpen<CuttingRoomEditorWindow>();

                if (EditorWindow != null)
                {
                    if (EditorWindow.InspectorWindow == null)
                    {
                        EditorWindow.InspectorWindow = this;
                    }

                    EditorWindow.OnSelect += () =>
                    {
                        RegenerateContents();
                    };

                    EditorWindow.OnDeselect += () =>
                    {
                        RegenerateContents();
                    };

                    EditorWindow.OnSelectionCleared += () =>
                    {
                        RegenerateContents();
                    };
                }
            }

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
            List<ISelectable> selected = EditorWindow.GraphView.selected;

            if (selected.Count == 1)
            {
                if (selected[0] is NarrativeObjectNode)
                {
                    NarrativeObjectNode narrativeObjectNode = selected[0] as NarrativeObjectNode;

                    Inspector.UpdateContentForNarrativeObjectNode(narrativeObjectNode);
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

                // Gets or creates narrative space.
                NarrativeSpace narrativeSpace = CuttingRoomEditorUtils.GetOrCreateNarrativeSpace();

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
