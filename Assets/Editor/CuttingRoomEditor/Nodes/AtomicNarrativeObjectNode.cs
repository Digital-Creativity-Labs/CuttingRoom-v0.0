using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class AtomicNarrativeObjectNode : NarrativeObjectNode
    {
        /// <summary>
        /// The graph narrative object represented by this node.
        /// </summary>
        private AtomicNarrativeObject AtomicNarrativeObject { get; set; } = null;

        /// <summary>
        /// The style sheet for this node.
        /// </summary>
        private StyleSheet StyleSheet = null;

        /// <summary>
        /// The toggle used to represent whether the atomic narrative object represented by this node has a media source assigned.
        /// </summary>
        private Toggle hasMediaSourceToggle = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="atomicNarrativeObject"></param>
        public AtomicNarrativeObjectNode(AtomicNarrativeObject atomicNarrativeObject, NarrativeObject parentNarrativeObject) : base(atomicNarrativeObject, parentNarrativeObject)
        {
            AtomicNarrativeObject = atomicNarrativeObject;

            atomicNarrativeObject.OnNarrativeObjectChanged += OnNarrativeObjectChanged;

            StyleSheet = Resources.Load<StyleSheet>("AtomicNarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            titleContainer.styleSheets.Add(StyleSheet);

            GenerateContents();

            SetContentsFields();
        }

        /// <summary>
        /// Generate the contents of the content container for this node.
        /// </summary>
        private void GenerateContents()
        {
            // Get the contents container for this node.
            VisualElement contents = this.Q<VisualElement>("contents");

            // Add a divider below the ports.
            VisualElement divider = new VisualElement();
            divider.name = "divider";
            divider.AddToClassList("horizontal");
            contents.Add(divider);

            // Add toggle to show whether this atomic has a media source.
            hasMediaSourceToggle = new Toggle("Has Media Source");
            hasMediaSourceToggle.SetEnabled(false);
            hasMediaSourceToggle.name = "media-toggle";
            hasMediaSourceToggle.styleSheets.Add(StyleSheet);
            contents.Add(hasMediaSourceToggle);
        }

        /// <summary>
        /// Set the fields representing the atomic narrative object in the contents container.
        /// </summary>
        private void SetContentsFields()
        {
            hasMediaSourceToggle.SetValueWithoutNotify(AtomicNarrativeObject.HasMediaSource);
        }

        /// <summary>
        /// Invoked when the atomic narrative object represented by this node is changed in the inspector.
        /// </summary>
        protected override void OnNarrativeObjectChanged()
        {
            SetContentsFields();
        }
    }
}