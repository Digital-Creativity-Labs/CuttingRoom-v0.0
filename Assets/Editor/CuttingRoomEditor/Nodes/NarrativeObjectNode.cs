using System;
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
        public NarrativeObject NarrativeObject { get; private set; } = null;

        /// <summary>
        /// The narrative object which is represented by the view which this node exists within.
        /// </summary>
        public NarrativeObject ParentNarrativeObject { get; private set; } = null;

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
        /// Event invoked when set as root is clicked.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsRoot;

        /// <summary>
        /// Event invoked when set as candidate is clicked in context menu.
        /// </summary>
        public event Action<NarrativeObjectNode> OnSetAsCandidate;

        /// <summary>
        /// Event invoked when remove as candidate is clicked in context menu.
        /// </summary>
        public event Action<NarrativeObjectNode> OnRemoveAsCandidate;

        /// <summary>
        /// Abstract method which must be implememented to update nodes when the
        /// narrative object they are representing has its values changed in the inspector.
        /// </summary>
        protected abstract void OnNarrativeObjectChanged();

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private Image rootImage = null;

        /// <summary>
        /// The root image visual element.
        /// </summary>
        private Image candidateImage = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="narrativeObject"></param>
        public NarrativeObjectNode(NarrativeObject narrativeObject, NarrativeObject parentNarrativeObject)
        {
            // Store reference to the narrative object being represented.
            NarrativeObject = narrativeObject;

            // Store reference to narrative object whose view container contains this node.
            ParentNarrativeObject = parentNarrativeObject;

            // Set the title of the node to the name of the game object it represents.
            title = narrativeObject.gameObject.name;

            InputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(float));
            InputPort.portName = "Input";

            OutputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(float));
            OutputPort.portName = "Output";

            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);

            StyleSheet = Resources.Load<StyleSheet>("NarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            // Get the contents container and add the stylesheet.
            VisualElement contents = this.Q<VisualElement>("contents");
            contents.styleSheets.Add(StyleSheet);
        }

        /// <summary>
        /// Enable the visual element for this being a root.
        /// </summary>
        public void EnableRootVisuals()
        {
            VisualElement titleElement = this.Q<VisualElement>("title");

            if (!titleElement.Contains(rootImage))
            {
                rootImage = new Image();

                rootImage.name = "root-icon";

                rootImage.styleSheets.Add(StyleSheet);

                Texture rootIcon = Resources.Load<Texture>("Icons/root-icon-24x24");

                rootImage.image = rootIcon;

                titleElement.Insert(0, rootImage);
            }
        }

        /// <summary>
        /// Enable the visual element for this being a candidate of a decision point.
        /// </summary>
        public void EnableCandidateVisuals()
        {
            VisualElement titleElement = this.Q<VisualElement>("title");

            if (!titleElement.Contains(candidateImage))
            {
                candidateImage = new Image();

                candidateImage.name = "candidate-icon";

                candidateImage.styleSheets.Add(StyleSheet);

                Texture candidateIcon = Resources.Load<Texture>("Icons/candidate-icon-24x24");

                candidateImage.image = candidateIcon;

                titleElement.Insert(0, candidateImage);
            }
        }

        /// <summary>
        /// Called when this node has to construct its contextual menu.
        /// </summary>
        /// <param name="evt"></param>
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Set as Root", OnSetAsRootFromContextualMenu, DropdownMenuAction.Status.Normal);

            // If null, then the object is on the root view level so can't be a candidate.
            if (ParentNarrativeObject != null)
            {
                // If this node exists inside a view which can have candidates.
                if (ParentNarrativeObject is GroupNarrativeObject)
                {
                    // If not currently candidate, add add candidate option, else remove candidate option.
                    GroupNarrativeObject groupNarrativeObject = ParentNarrativeObject.GetComponent<GroupNarrativeObject>();

                    if (groupNarrativeObject.groupSelectionDecisionPoint.Candidates.Contains(NarrativeObject.gameObject))
                    {
                        evt.menu.AppendAction("Remove as Candidate", OnRemoveAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                    else
                    {
                        evt.menu.AppendAction("Set as Candidate", OnSetAsCandidateFromContextualMenu, DropdownMenuAction.Status.Normal);
                    }
                }
            }

            evt.menu.AppendSeparator();
        }

        /// <summary>
        /// Callback for Set as Root option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsRootFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsRoot?.Invoke(this);
        }

        /// <summary>
        /// Callback for Set as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnSetAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnSetAsCandidate?.Invoke(this);
        }

        /// <summary>
        /// Callback for Remove as Candidate option in contextual menu.
        /// </summary>
        /// <param name="action"></param>
        private void OnRemoveAsCandidateFromContextualMenu(DropdownMenuAction action)
        {
            OnRemoveAsCandidate?.Invoke(this);
        }
    }
}
