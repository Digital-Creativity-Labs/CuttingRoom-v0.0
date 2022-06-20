using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CuttingRoom.Editor
{
    public class GroupNarrativeObjectNode : NarrativeObjectNode
    {
        /// <summary>
        /// The group narrative object represented by this node.
        /// </summary>
        private GroupNarrativeObject GroupNarrativeObject { get; set; } = null;

        /// <summary>
		/// The style sheet for this node.
		/// </summary>
		private StyleSheet StyleSheet = null;

        /// <summary>
		/// Button allowing contents to be viewed.
		/// </summary>
		private Button viewContentsButton = null;

        /// <summary>
		/// Invoked when the view contents button is clicked.
		/// </summary>
		public event Action<NarrativeObjectNode> OnClickViewContents;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="groupNarrativeObject"></param>
        public GroupNarrativeObjectNode(GroupNarrativeObject groupNarrativeObject, NarrativeObject parentNarrativeObject) : base(groupNarrativeObject, parentNarrativeObject)
        {
            GroupNarrativeObject = groupNarrativeObject;

            groupNarrativeObject.OnNarrativeObjectChanged += OnNarrativeObjectChanged;

            StyleSheet = Resources.Load<StyleSheet>("GroupNarrativeObjectNode");

            VisualElement titleElement = this.Q<VisualElement>("title");
            titleElement?.styleSheets.Add(StyleSheet);

            GenerateContents();
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

            // Add button to push view for this graph node onto the stack.
            viewContentsButton = new Button(() =>
            {
                OnClickViewContents?.Invoke(this);
            });
            viewContentsButton.text = "View Contents";
            viewContentsButton.name = "view-contents-button";
            viewContentsButton.styleSheets.Add(StyleSheet);
            contents?.Add(viewContentsButton);
        }

        /// <summary>
        /// Invoked when the group narrative object represented by this node is changed in the inspector.
        /// </summary>
        protected override void OnNarrativeObjectChanged()
        {
        }

        public override List<BlackboardRow> GetBlackboardRows()
        {
            List<BlackboardRow> blackboardRows = new List<BlackboardRow>(base.GetBlackboardRows());

            BlackboardRow groupSelectionMethodNameRow = CreateBlackboardRowTextField("Group Selection Method", GroupNarrativeObject.groupSelectionDecisionPoint.groupSelectionMethodName.methodName, (string newValue) =>
            {
                GroupNarrativeObject.groupSelectionDecisionPoint.groupSelectionMethodName.methodName = newValue;
            });

            groupSelectionMethodNameRow.expanded = true;

            blackboardRows.Add(groupSelectionMethodNameRow);

            return blackboardRows;
        }
    }
}