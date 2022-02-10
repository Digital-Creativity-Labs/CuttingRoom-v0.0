using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class OutputSelectionDecisionPointNode : DecisionPointNode
	{
		/// <summary>
		/// Object in the scene which this node represents.
		/// </summary>
		public OutputSelectionDecisionPoint outputSelectionDecisionPoint { get; private set; } = null;

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, OutputSelectionDecisionPoint outputSelectionDecisionPoint, Vector2 position, Vector2 size)
		{
			base.Init(narrativeSpaceEditor, outputSelectionDecisionPoint);

			this.outputSelectionDecisionPoint = outputSelectionDecisionPoint;

			windowRect = new Rect(position, size);
			windowTitle = "Output Selection Decision Point";
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);

			string methodName = outputSelectionDecisionPoint.outputSelectionMethodName.methodName;

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
