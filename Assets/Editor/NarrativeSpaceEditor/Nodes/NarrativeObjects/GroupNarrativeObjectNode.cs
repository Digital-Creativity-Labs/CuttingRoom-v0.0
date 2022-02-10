using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class GroupNarrativeObjectNode : NarrativeObjectNode
	{
		public GroupNarrativeObject groupNarrativeObject { get; private set; } = null;

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, GroupNarrativeObject groupNarrativeObject)
		{
			base.Init(narrativeSpaceEditor, groupNarrativeObject);

			this.groupNarrativeObject = groupNarrativeObject;

			windowTitle = "GroupNarrativeObject";
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);

			GUIContent buttonText = new GUIContent("View Group Contents");

			GUIRenderingUtilities.RenderGUIElement(renderSettings, buttonText, GUI.skin.button,
				(Vector2 position, Vector2 size) =>
				{
					if (GUI.Button(new Rect(position, size), buttonText))
					{
						narrativeSpaceEditor.PushToViewStack(this);
					}
				});
		}
	}
}