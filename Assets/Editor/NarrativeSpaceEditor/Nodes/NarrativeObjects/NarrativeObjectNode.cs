using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class NarrativeObjectNode : BaseNode
	{
		/// <summary>
		/// Texture to change colour of editor.
		/// </summary>
		public static Texture2D backgroundTexture = null;

		/// <summary>
		/// The narrative object represented by this node.
		/// </summary>
		public NarrativeObject narrativeObject = null;

		/// <summary>
		/// The output selection node for this narrative object. All narrative objects have output selection.
		/// </summary>
		public OutputSelectionDecisionPointNode outputSelectionDecisionPointNode { get; private set; } = null;

		private static GUIStyle rootNarrativeObjectLabelGUIStyle
		{
			get
			{
				GUIStyle rootNarrativeObjectLabelGUIStyle = new GUIStyle(EditorStyles.boldLabel);

				rootNarrativeObjectLabelGUIStyle.normal.textColor = Color.red;

				return rootNarrativeObjectLabelGUIStyle;
			}
		}

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, NarrativeObject narrativeObject)
		{
			base.Init(narrativeSpaceEditor);

			this.narrativeObject = narrativeObject;
		}

		public override void SetPosition(Vector2 position)
		{
			base.SetPosition(position);

			outputSelectionDecisionPointNode?.SetPosition(position + new Vector2(windowRect.size.x + outputSelectionDecisionPointNode.windowRect.size.x + 50.0f, 0.0f));
		}

		public void SetOutputSelectionDecisionPointNode(OutputSelectionDecisionPointNode outputSelectionDecisionPointNode)
		{
			this.outputSelectionDecisionPointNode = outputSelectionDecisionPointNode;
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);

			if (narrativeSpaceEditor.narrativeSpace.rootNarrativeObject == narrativeObject)
			{
				GUIContent content = new GUIContent("Root Narrative Object");

				GUIRenderingUtilities.RenderGUIElement(renderSettings, content, rootNarrativeObjectLabelGUIStyle,
					(Vector2 position, Vector2 size) =>
					{
						GUI.Label(new Rect(position, size), content, rootNarrativeObjectLabelGUIStyle);
					});
			}
		}
	}
}
