using UnityEngine;
using UnityEditor;

namespace CuttingRoom.Editor
{
	public class BaseNode : ScriptableObject
	{
		public Rect windowRect = default;
		public string windowTitle = string.Empty;

		/// <summary>
		/// A reference to the narrative space editor this node exists inside.
		/// </summary>
		protected NarrativeSpaceEditor narrativeSpaceEditor = null;

		/// <summary>
		/// A reference to the baseNode which contains this node. This allows hierarchical rendering.
		/// </summary>
		public BaseNode parentBaseNode { get; set; } = null;

		/// <summary>
		/// Flag to indicate whether this base node has been positioned initially.
		/// </summary>
		public bool InitialPositionSet { get; private set; } = false;

		/// <summary>
		/// Position of the baseNode.
		/// </summary>
		public Vector2 position { get { return windowRect.position; } }

		/// <summary>
		/// Initialisation routine for the node.
		/// </summary>
		/// <param name="narrativeSpaceEditor"></param>
		protected void Init(NarrativeSpaceEditor narrativeSpaceEditor)
		{
			this.narrativeSpaceEditor = narrativeSpaceEditor;
		}

		/// <summary>
		/// Render method for the node.
		/// </summary>
		public virtual void DrawWindow(GUIRenderingUtilities.RenderSettings renderSettings)
		{
			GUIContent content = new GUIContent(windowRect.position.ToString());

			GUIRenderingUtilities.RenderGUIElement(renderSettings, content, GUI.skin.label,
				(Vector2 position, Vector2 size) =>
				{
					GUI.Label(new Rect(position, size), content);
				});
		}

		public virtual void SetPosition(Vector2 position)
		{
			InitialPositionSet = true;

			windowRect.position = position;
		}

		public virtual void SetSize(Vector2 size)
		{
			windowRect.size = size;
		}

		public virtual void Pan(Vector2 panDelta)
		{
			windowRect.x -= panDelta.x;
			windowRect.y -= panDelta.y;
		}
	}
}
