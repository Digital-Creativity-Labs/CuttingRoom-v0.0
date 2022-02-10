using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class AtomicNarrativeObjectNode : NarrativeObjectNode
	{
		/// <summary>
		/// The atomic narrative object in the scene which this node represents.
		/// </summary>
		private AtomicNarrativeObject atomicNarrativeObject = null;

		// Thumbnail for the texture.
		private Texture2D thumbnail = null;

		public void Init(NarrativeSpaceEditor narrativeSpaceEditor, AtomicNarrativeObject atomicNarrativeObject)
		{
			base.Init(narrativeSpaceEditor, atomicNarrativeObject);

			this.atomicNarrativeObject = atomicNarrativeObject;

			windowTitle = atomicNarrativeObject.gameObject.name;
		}

		public override async void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			base.DrawWindow(renderSettings);

			if (thumbnail == null)
			{
				thumbnail = await atomicNarrativeObject.GetThumbnail();
			}
			else
			{
				Vector2 thumbnailSize = new Vector2(150.0f, 84.0f);

				GUIRenderingUtilities.RenderGUIElement(renderSettings, thumbnailSize,
					(Vector2 position, Vector2 size) =>
					{
						EditorGUI.DrawPreviewTexture(new Rect(position, size), thumbnail);
					});
			}

			windowRect.size = renderSettings.size * new Vector2(1.06f, 1.22f);
		}
	}
}
