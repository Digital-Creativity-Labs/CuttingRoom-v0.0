using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CuttingRoom.Editor
{
	public class GroupSelectionDecisionPointNode : DecisionPointNode
	{
		/// <summary>
		/// Decision point represented by this node.
		/// </summary>
		public GroupSelectionDecisionPoint groupSelectionDecisionPoint = null;

		/// <summary>
		/// Initialisation routine.
		/// </summary>
		/// <param name="groupSelectionDecisionPoint"></param>
		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, GroupSelectionDecisionPoint groupSelectionDecisionPoint, Vector2 position, Vector2 size)
		{
			base.Init(narrativeSpaceEditor, groupSelectionDecisionPoint);

			this.groupSelectionDecisionPoint = groupSelectionDecisionPoint;

			windowRect = new Rect(position, size);
			windowTitle = "Group Selection Decision Point";
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);

			string methodName = groupSelectionDecisionPoint.groupSelectionMethodName.methodName;

			if (string.IsNullOrEmpty(methodName))
			{
				methodName = "Undefined";
			}

			GUIContent labelContent = new GUIContent($"Method Name: {methodName}");

			GUIRenderingUtilities.RenderGUIElement(renderSettings, labelContent, GUI.skin.label,
				(Vector2 position, Vector2 size) =>
				{
					GUI.Label(new Rect(position, size), labelContent);
				});
		}
	}
}
