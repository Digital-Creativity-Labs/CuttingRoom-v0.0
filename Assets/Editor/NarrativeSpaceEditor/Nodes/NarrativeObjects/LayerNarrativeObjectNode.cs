using UnityEngine;

namespace CuttingRoom.Editor
{
	public class LayerNarrativeObjectNode : NarrativeObjectNode
	{
		public LayerNarrativeObject layerNarrativeObject = null;

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, LayerNarrativeObject layerNarrativeObject)
		{
			base.Init(narrativeSpaceEditor, layerNarrativeObject);

			this.layerNarrativeObject = layerNarrativeObject;

			windowTitle = "LayerNarrativeObject";
		}

		public override void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);
		}
	}
}