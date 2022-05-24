using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public abstract class NarrativeObjectNode : Node
    {
        /// <summary>
        /// The narrative object represented by this node.
        /// </summary>
        public NarrativeObject NarrativeObject { get; set; } = null;

        /// <summary>
        /// The input port for this node.
        /// </summary>
        public Port InputPort { get; private set; } = null;

        /// <summary>
        /// The output port for this node.
        /// </summary>
        public Port OutputPort { get; private set; } = null;

        /// <summary>
        /// The style sheet for this node.
        /// </summary>
        private StyleSheet StyleSheet = null;

        /// <summary>
        /// Abstract method which must be implememented to update nodes when the
        /// narrative object they are representing has its values changed in the inspector.
        /// </summary>
        protected abstract void OnNarrativeObjectChanged();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="narrativeObject"></param>
        public NarrativeObjectNode(NarrativeObject narrativeObject)
        {
            // Store reference to the narrative object being represented.
            NarrativeObject = narrativeObject;

            // Set the title of the node to the name of the game object it represents.
            title = narrativeObject.gameObject.name;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "Input";

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.portName = "Output";

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            // On mouse down, select the narrative object in the editor/inspector. 
            RegisterCallback<MouseDownEvent>((mouseDownEvent) =>
            {
                Selection.activeGameObject = NarrativeObject.gameObject;
            });

            StyleSheet = Resources.Load<StyleSheet>("NarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);
        }

        /// <summary>
        /// Called when this node has to construct its contextual menu.
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Set as Root", OnSetAsRootFromContextualMenu, DropdownMenuAction.Status.Normal);
            evt.menu.AppendSeparator();
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsRootFromContextualMenu(DropdownMenuAction action)
        {
            Debug.LogError("Set as root not implemented.");
        }
    }
}